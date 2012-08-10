using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;

namespace JSIL.Transforms {
    class ConvertPropertyAccessesToInvocations : JSAstVisitor {
        public static bool CanConvertToInvocation (JSPropertyAccess pa) {
            return (!pa.Property.Property.Metadata.HasAttribute("JSIL.Meta.JSAlwaysAccessAsProperty"));
        }

        public void VisitNode (JSPropertyAccess pa) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;

            if (
                !pa.IsWrite &&
                (parentBoe == null) &&
                CanConvertToInvocation(pa)
            ) {
                // getter
                var invocation = JSInvocationExpression.InvokeMethod(
                    pa.OriginalMethod, pa.ThisReference
                );

                JSExpression replacement;
                if (TypeUtil.IsStruct(pa.Property.Property.ReturnType))
                    replacement = new JSResultReferenceExpression(invocation);
                else
                    replacement = invocation;

                ParentNode.ReplaceChild(pa, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(pa);
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var pa = boe.Left as JSPropertyAccess;

            if (
                (pa != null) &&
                pa.IsWrite &&
                (boe.Operator == JSOperator.Assignment) &&
                CanConvertToInvocation(pa)
            ) {
                // setter
                var invocation = JSInvocationExpression.InvokeMethod(
                    pa.OriginalMethod, pa.ThisReference,
                    new[] { boe.Right }
                );

                ParentNode.ReplaceChild(boe, invocation);
                VisitReplacement(invocation);
            } else {
                VisitChildren(boe);
            }
        }
    }
}
