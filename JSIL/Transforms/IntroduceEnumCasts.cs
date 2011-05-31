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

        public void VisitNode (JSSwitchStatement ss) {
            var conditionType = ss.Condition.GetExpectedType(TypeSystem);

            if (!ILBlockTranslator.IsIntegral(conditionType)) {
                var indexTypeDef = ILBlockTranslator.GetTypeDefinition(conditionType);

                if (indexTypeDef.IsEnum) {
                    var cast = JSInvocationExpression.InvokeStatic(
                        new JSFakeMethod("Number", TypeSystem.Int32, indexTypeDef), new[] { ss.Condition }, true
                    );

                    ss.ReplaceChild(ss.Condition, cast);
                }
            }

            VisitChildren(ss);
        }
    }
}
