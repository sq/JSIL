using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EmulateStructAssignment : JSAstVisitor {
        public readonly TypeSystem TypeSystem;

        public EmulateStructAssignment (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSInvocationExpression invocation) {
            foreach (var argument in invocation.Arguments)
                if (argument.GetExpectedType(TypeSystem).IsValueType) {
                    // Console.WriteLine("arg {0}", argument);
                }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if (boe.Operator != JSOperator.Assignment) {
                base.VisitNode(boe);
                return;
            }

            if ((boe.Left.GetExpectedType(TypeSystem).IsValueType) || (boe.Right.GetExpectedType(TypeSystem).IsValueType)) {
                // Console.WriteLine("{0} = {1}", boe.Left, boe.Right);
            }
        }
    }
}
