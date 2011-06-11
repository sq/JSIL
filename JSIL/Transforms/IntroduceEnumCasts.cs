using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class IntroduceEnumCasts : JSAstVisitor {
        public readonly TypeSystem TypeSystem;

        public IntroduceEnumCasts (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSIndexerExpression ie) {
            var indexType = ie.Index.GetExpectedType(TypeSystem);

            if (
                !ILBlockTranslator.IsIntegral(indexType) &&
                ILBlockTranslator.IsEnum(indexType)
            ) {
                var cast = JSInvocationExpression.InvokeStatic(
                    new JSFakeMethod("Number", TypeSystem.Int32, indexType), new[] { ie.Index }, true
                );

                ie.ReplaceChild(ie.Index, cast);
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSSwitchStatement ss) {
            var conditionType = ss.Condition.GetExpectedType(TypeSystem);

            if (
                !ILBlockTranslator.IsIntegral(conditionType) &&
                ILBlockTranslator.IsEnum(conditionType)
            ) {
                var cast = JSInvocationExpression.InvokeStatic(
                    new JSFakeMethod("Number", TypeSystem.Int32, conditionType), new[] { ss.Condition }, true
                );

                ss.ReplaceChild(ss.Condition, cast);
            }

            VisitChildren(ss);
        }
    }
}
