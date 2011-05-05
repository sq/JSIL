using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EmulateStructAssignment : JSAstVisitor {
        public readonly CLRSpecialIdentifiers CLR;
        public readonly TypeSystem TypeSystem;

        public EmulateStructAssignment (TypeSystem typeSystem, CLRSpecialIdentifiers clr) {
            TypeSystem = typeSystem;
            CLR = clr;
        }

        protected static bool IsStruct (TypeReference type) {
            var typedef = type.Resolve();

            if (typedef != null) {
                if (typedef.IsEnum)
                    return false;
            }

            return type.IsValueType && !type.IsPrimitive;
        }

        protected bool IsCopyNeeded (JSExpression value) {
            var valueType = value.GetExpectedType(TypeSystem);

            if (!IsStruct(valueType))
                return false;

            if (
                (value is JSLiteral) ||
                (value is JSInvocationExpression) ||
                (value is JSNewExpression)
            ) {
                return false;
            }
            
            return true;
        }

        protected JSInvocationExpression MakeCopy (JSExpression value) {
            return new JSInvocationExpression(new JSDotExpression(value, CLR.MemberwiseClone));
        }

        public void VisitNode (JSPairExpression pair) {
            if (IsCopyNeeded(pair.Value)) {
                Debug.WriteLine(String.Format("struct copy introduced for object value {0}", pair.Value));
                pair.Value = MakeCopy(pair.Value);
            }

            VisitChildren(pair);
        }

        public void VisitNode (JSInvocationExpression invocation) {
            for (int i = 0, c = invocation.Arguments.Count; i < c; i++) {
                var argument = invocation.Arguments[i];

                if (IsCopyNeeded(argument)) {
                    Debug.WriteLine(String.Format("struct copy introduced for argument {0}", argument));
                    invocation.Arguments[i] = MakeCopy(argument);
                }
            }

            VisitChildren(invocation);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if (boe.Operator != JSOperator.Assignment) {
                base.VisitNode(boe);
                return;
            }

            if (IsCopyNeeded(boe.Right)) {
                Debug.WriteLine(String.Format("struct copy introduced for assignment rhs {0}", boe.Right));
                boe.Right = MakeCopy(boe.Right);
            }

            VisitChildren(boe);
        }
    }
}
