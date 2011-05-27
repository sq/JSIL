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
            var targetType = ie.Target.GetExpectedType(TypeSystem);
            var indexType = ie.Index.GetExpectedType(TypeSystem);
            var expectedType = ie.GetExpectedType(TypeSystem);

            if (!ILBlockTranslator.IsIntegral(indexType)) {
                var indexTypeDef = ILBlockTranslator.GetTypeDefinition(indexType);

                if (indexTypeDef.IsEnum) {
                    var cast = new JSDotExpression(
                        ie.Index, new JSStringIdentifier("value", TypeSystem.Int32)
                    );

                    ie.ReplaceChild(ie.Index, cast);
                }
            }

            VisitChildren(ie);
        }
    }
}
