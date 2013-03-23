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
    public class IntroducePackedArrays : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;

        public IntroducePackedArrays (TypeSystem typeSystem, MethodTypeFactory methodTypes) {
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;
        }

        public void VisitNode (JSNewArrayExpression nae) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;

            if (parentBoe != null) {
                var leftField = parentBoe.Left as JSFieldAccess;
                if (
                    (leftField != null) &&
                    PackedArrayUtil.IsPackedArrayType(leftField.Field.Field.FieldType)
                ) {
                    JSNewPackedArrayExpression replacement;
                    if (nae.Dimensions != null) {
                        replacement = new JSNewPackedArrayExpression(leftField.Field.Field.FieldType, nae.ElementType, nae.Dimensions, nae.SizeOrArrayInitializer);
                    } else {
                        replacement = new JSNewPackedArrayExpression(leftField.Field.Field.FieldType, nae.ElementType, nae.SizeOrArrayInitializer);
                    }
                    ParentNode.ReplaceChild(nae, replacement);
                    VisitReplacement(replacement);
                    return;
                }
            }

            VisitChildren(nae);
        }

        public void VisitNode (JSArrayExpression ae) {
            VisitChildren(ae);
        }

        public void VisitNode (JSNewPackedArrayExpression npae) {
            VisitChildren(npae);
        }
    }
}
