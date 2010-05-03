﻿module IronJS.Compiler.Core

open IronJS
open IronJS.Aliases
open IronJS.Tools
open IronJS.Tools.Dlr
open IronJS.Compiler
open IronJS.Compiler.Types
open IronJS.Compiler.Wrap

let private buildVarsMap (scope:Ast.Types.Scope) =
  let createVar (var:Ast.Types.Variable) =
    let clrTyp = Runtime.Utils.Type.jsToClr var.UsedAs
    if Ast.Variable.isClosedOver var
      then Expr.param var.Name (Type.strongBoxType.MakeGenericType([|clrTyp|]))
      else Expr.param var.Name clrTyp

  let createProxy (var:Ast.Types.Variable) =
    Expr.param (sprintf "%s_proxy" var.Name) scope.ArgTypes.[var.Index]

  scope.Variables
    |> Map.map (
      fun _ var -> 
        match Ast.Variable.isParameter var with
        | true -> 
          match Ast.Variable.needsProxy var with
          | true  -> Proxied(createVar var, createProxy var)
          | false -> Variable(createVar var, Param)
        | false -> 
          Variable(createVar var, Local)
      )

let private isLocal (_, var:Variable) =
  match var with
  | Variable(_, Local) -> true
  | Proxied(_, _) -> true
  | _ -> false

let private isParameter (_, var:Variable) =
  match var with
  | Variable(_, Param) -> true
  | Proxied(_, _) -> true
  | _ -> false

let private toParm (_, var:Variable) =
  match var with
  | Variable(p, Param) -> p
  | Proxied(_, p)  -> p
  | _ -> failwith "Que?"

let private toLocal (_, var:Variable) =
  match var with
  | Variable(p, Local) -> p
  | Proxied(l, _)  -> l
  | _ -> failwith "Que?"

let private isProxied (_, var:Variable) =
  match var with
  | Proxied(_, _) -> true
  | _ -> false

let private builder (ctx:Context) (ast:Ast.Node) =
  match ast with
  //Simple
  | Ast.String(value)  -> static' (Expr.constant value)
  | Ast.Number(value)  -> static' (Expr.constant value)
  | Ast.Integer(value) -> static' (Expr.constant value)
  | Ast.Null           -> static' (Expr.null')

  //Assign
  | Ast.Assign(left, right) -> Assign.build ctx left right

  //Block
  | Ast.Block(nodes) -> 
    volatile'(
      Expr.block [
        for n in nodes -> unwrap (ctx.Build n)
      ]
    )

  //Functions
  | Ast.Function(astId) -> Function.define ctx astId
  | Ast.Invoke(target, args, scopeLevels) -> Function.invoke ctx target args

  //Objects
  | Ast.Object(properties, id) -> Object.build ctx properties id
  | Ast.Property(object', name) -> Object.getProperty ctx (ctx.Build object') name None 

  //Loops
  | Ast.ForIter(init, test, incr, body) -> Loops.forIter ctx init test incr body
  | Ast.BinaryOp(op, left, right) -> BinaryOp.build ctx op left right

  //Variable access
  | Ast.Global(name, _) -> Variables.getGlobal ctx name (Context.temporaryType ctx name)

  | _ -> failwithf "No builder for '%A'" ast

(*Compiles a Ast.Node tree into a DLR Expression-tree*)
let compileAst (env:Runtime.Environment) (delegateType:ClrType) (closureType:ClrType) (scope:Ast.Types.Scope) (ast:Ast.Node) =

  let ctx = {
    Context.New with
      Scope = scope
      Variables = buildVarsMap scope
      Builder = builder
      Environment = env
      Internal = 
      {
        Types.InternalVariables.New with 
          Closure = Dlr.Expr.param "~closure" closureType
      }
  }

  let initGlobals = 
    let expr = Expr.field (Context.environmentExpr ctx) "Globals"
    Expr.assign ctx.Internal.Globals expr

  let initClosure = 
    let expr = Expr.field ctx.Internal.Function "Closure"
    Expr.assign ctx.Internal.Closure (Expr.cast closureType expr)

  let initEnvironment = 
    let expr = Expr.field ctx.Internal.Function "Environment"
    Expr.assign ctx.Internal.Environment expr

  (*Initialize proxied parameters*)
  let initProxied = 
    ctx.Variables
      |>  Map.toSeq
      |>  Seq.filter isProxied
      |>  Seq.map (fun (_, Proxied(var, proxy)) ->
            if Type.isStrongBox var.Type 
              then Expr.assign var (Expr.newArgs var.Type [proxy])
              else Expr.assign var (Expr.cast var.Type proxy)
          )
      #if DEBUG
      |>  Seq.toArray
      #endif

  (*Initialize closed over variables and parameters*)
  let initClosedOver = 
    ctx.Scope.Variables
      |>  Map.toSeq
      |>  Seq.map (fun pair -> snd pair)
      |>  Seq.filter (fun lv -> Ast.Variable.isClosedOver lv)
      |>  Seq.filter (fun lv -> not (Ast.Variable.needsProxy lv))
      |>  Seq.map (fun lv -> 
            let expr = Context.variableExpr ctx lv.Name
            Expr.assign expr (Expr.new' expr.Type) 
          )
      #if DEBUG
      |>  Seq.toArray
      #endif

  (*Initialize variables that need to be set as undefined*)
  let initUndefined =
    ctx.Scope.Variables
      |>  Map.toSeq
      |>  Seq.map (fun pair -> snd pair)
      |>  Seq.filter (fun lv -> Ast.Variable.initToUndefined lv)
      |>  Seq.map (fun lv -> 
            let expr = Context.variableExpr ctx lv.Name
            Expr.assign expr Runtime.Undefined.InstanceExpr
          )
      #if DEBUG
      |>  Seq.toArray
      #endif

  (*Builds the if-statements and the end of each
  function that updates the object caches*)
  let objectCacheUpdateExpressions = 
    ctx.ObjectCaches 
    |>  Map.toSeq 
    |>  Seq.map (fun pair ->
          let oc = snd pair
          let last = Expr.field oc "LastCreated"
          (Expr.if'  
            (Expr.andChain 
            [
              (Expr.notEq last Expr.defaultT<Runtime.Object>)
              (Expr.notEq (Expr.field last "ClassId") (Expr.field oc "ClassId"))
            ])
            (Expr.block 
            [
              (Expr.assign (Expr.field oc "ClassId") (Expr.field last "ClassId"))
              (Expr.assign (Expr.field oc "Class") (Expr.field last "Class"))
              (Expr.assign (Expr.field oc "InitSize") (Expr.property (Expr.field last "Properties") "Length"))
            ])
          )
        )
        
  (*Assemble the function body expression*)
  let wrap = ctx.Build ast
  let body = 
    (Expr.labelExprT<Runtime.Box> ctx.Return :: [])
      |>  Seq.append objectCacheUpdateExpressions
      |>  Seq.append (unwrap wrap :: [])
      |>  Seq.append initUndefined
      |>  Seq.append initClosedOver
      |>  Seq.append initProxied
      |>  Seq.append (initEnvironment :: initGlobals :: initClosure :: [])
      #if DEBUG
      |>  Seq.toArray
      |>  fun x -> Expr.block x
      #endif
    
  (*Resolve all locals that are parameters*)
  let parms  = 
    ctx.Variables
      |>  Map.toSeq
      |>  Seq.filter isParameter
      |>  Seq.map toParm
      |>  Seq.append (Context.internalParams ctx)
      #if DEBUG
      |>  Seq.toArray
      #endif

  (*Resolve all locals that are normal variables*)
  let locals = 
    ctx.Variables
      |>  Map.toSeq
      |>  Seq.filter isLocal 
      |>  Seq.map toLocal
      |>  Seq.append (Context.internalLocals ctx)
      #if DEBUG
      |>  Seq.toArray
      #endif

  #if DEBUG
  let lmb = Dlr.Expr.lambda delegateType parms (Dlr.Expr.blockWithLocals locals [body])
  #else
  let lmb = Dlr.Expr.lambda delegateType parms (Dlr.Expr.blockWithLocals locals body)
  #endif

  #if DEBUG
  printf "%A" (Fsi.dbgViewProp.GetValue(lmb :> Et, null))
  #endif

  lmb