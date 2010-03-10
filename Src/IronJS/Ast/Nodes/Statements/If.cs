﻿using System;
using Antlr.Runtime.Tree;
using IronJS.Runtime2.Js;

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

namespace IronJS.Ast.Nodes {
	public class If : Base {
		public INode Test { get { return Children[0]; } }
		public INode TrueBranch { get { return Children[1]; } }
		public INode ElseBranch { get { return Children[2]; } }
		public bool HasElseBranch { get { return ElseBranch != null; } }
		public bool IsTernary { get; protected set; }

		public If(INode test, INode trueBranch, INode elseBranch, bool isTernary, ITree node)
			: base(NodeType.If, node) {
			Children = new[] { test, trueBranch, elseBranch };
			IsTernary = isTernary;
		}

		public override Type Type {
			get {
				if (IsTernary && TrueBranch.Type == ElseBranch.Type) {
					return TrueBranch.Type;
				}

				return IjsTypes.Dynamic;
			}
		}
	}
}