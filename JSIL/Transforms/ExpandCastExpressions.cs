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
    public class ExpandCastExpressions : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;
        public readonly JSILIdentifier JSIL;

        public ExpandCastExpressions (TypeSystem typeSystem, JSSpecialIdentifiers js, JSILIdentifier jsil) {
            TypeSystem = typeSystem;
            JS = js;
            JSIL = jsil;
        }

        public void VisitNode (JSCastExpression ce) {
            var currentType = ce.Expression.GetExpectedType(TypeSystem);
            var targetType = ce.NewType;

            JSExpression newExpression = null;

            if (targetType.MetadataType == MetadataType.Char) {
                newExpression = JSInvocationExpression.InvokeStatic(
                    JS.fromCharCode, new[] { ce.Expression }, true
                );
            } else if (
                (currentType.MetadataType == MetadataType.Char) &&
                ILBlockTranslator.IsIntegral(targetType)
            ) {
                newExpression = JSInvocationExpression.InvokeMethod(
                    JS.charCodeAt, ce.Expression, new[] { JSLiteral.New(0) }, true
                );
            } else if (
                ILBlockTranslator.IsEnum(currentType) &&
                ILBlockTranslator.IsIntegral(targetType)
            ) {
                newExpression = new JSDotExpression(
                    ce.Expression, new JSStringIdentifier("value", targetType)
                );
            } else {
                newExpression = JSIL.Cast(ce.Expression, targetType);
            }

            if (newExpression != null) {
                ParentNode.ReplaceChild(ce, newExpression);
                VisitReplacement(newExpression);
            } else {
                VisitChildren(ce);
            }
        }
    }
}
