﻿open IronJS
open System
open Antlr.Runtime
open IronJS.CSharp.Parser

let jsLexer = new ES3Lexer(new ANTLRFileStream("Testing.js"))
let jsParser = new ES3Parser(new CommonTokenStream(jsLexer))
let program = jsParser.program()

let ast = Ast.generator program.Tree

let globals = Runtime.globalClosure()
let compiled = (IronJS.Compiler.compile ast [typeof<Runtime.Closure>]) :?> Func<Runtime.Closure, System.Object>

compiled.Invoke(globals)
