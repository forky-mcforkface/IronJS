﻿using System;
using System.Text;
using Antlr.Runtime.Tree;
using IronJS.Runtime2.Js;
using System.Collections.Generic;

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

namespace IronJS.Compiler.Ast {
	public class Void : Node {
		public INode Target { get { return Children[0]; } }

		public Void(INode target, ITree node)
			: base(NodeType.Void, node) {
			Children = new[] { target };
		}

		public override Type Type {
			get {
				return IjsTypes.Undefined;
			}
		}
	}
}
