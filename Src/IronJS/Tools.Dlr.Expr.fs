﻿namespace IronJS.Tools.Dlr

open IronJS.Utils
open System.Linq.Expressions

(*Tools for working with DLR expressions*)

module Expr =

  //Constants
  let private optionType = 
    typedefof<option<_>>

  let empty = 
    AstUtils.Empty() :> Et

  let objDefault =
    Et.Default(typeof<obj>) :> Et

  let typeDefault<'a> =
    Et.Default(typeof<'a>) :> Et

  //Functions
  let param name typ =
    Et.Parameter(typ, name)

  let paramT<'a> name = 
    param name typeof<'a>

  let label name =
    Et.Label(typeof<obj>, name)

  let labelExpr label =
    Et.Label(label, Et.Default(typeof<obj>)) :> Et

  let blockWithLocals (parms:EtParam list) (exprs:Et list) =
    Et.Block(parms, if exprs.Length = 0 then [AstUtils.Empty() :> Et] else exprs) :> Et

  let block = 
    blockWithLocals []

  let lambda (parms:EtParam list) (body:Et) = 
    Et.Lambda(body, parms)

  let field expr name =
    Et.PropertyOrField(expr, name) :> Et

  let call (expr:Et) name (args:Et list) =
    let mutable mi = expr.Type.GetMethod(name)
    
    if mi.ContainsGenericParameters then 
      mi <- mi.MakeGenericMethod(List.toArray [for arg in args -> arg.Type])

    Et.Call(expr, mi, args) :> Et

  let constant value =
    Et.Constant(value, value.GetType()) :> Et

  let refEq left right =
    Et.ReferenceEqual(left, right) :> Et

  let cast expr typ =
    Et.Convert(expr, typ) :> Et

  let castT<'a> expr = 
    cast expr typeof<'a> 

  let makeReturn label (value:Et) =
    Et.Return(label, value) :> Et

  let assign (left:Et) (right:Et) =
    Et.Assign(left, right) :> Et

  let index (left:Et) (i:int) =
    Et.ArrayIndex(left, constant i) :> Et

  let newInstance (typ:System.Type) =
    Et.New(typ) :> Et

  let newGeneric (typ:System.Type) (types:ClrType seq) =
    newInstance (typ.MakeGenericType(Seq.toArray types))

  let newArgs (typ:System.Type) (args:Et seq) =
    let typ_ctor = IronJS.Utils.getCtor typ [for arg in args -> arg.Type]
    Et.New(typ_ctor, args) :> Et

  let newGenericArgs (typ:System.Type) (types:ClrType seq) (args:Et seq) =
    newArgs (typ.MakeGenericType(Seq.toArray types)) args

  let throw (typ:System.Type) (args:Et seq) =
    Et.Throw(newArgs typ args) :> Et

  let delegateType (types:ClrType seq) =
    Et.GetDelegateType(Seq.toArray types)

  let invoke (func:Et) (args:Et list) =
    Et.Invoke(func, args)

  let dynamic binder typ (args:Et seq) =
    Et.Dynamic(binder, typ, args) :> Et