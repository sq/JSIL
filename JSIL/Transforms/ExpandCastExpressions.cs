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
        public readonly ITypeInfoSource TypeInfo;

        public ExpandCastExpressions (TypeSystem typeSystem, JSSpecialIdentifiers js, JSILIdentifier jsil, ITypeInfoSource typeInfo) {
            TypeSystem = typeSystem;
            JS = js;
            JSIL = jsil;
            TypeInfo = typeInfo;
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
                ILBlockTranslator.IsEnum(currentType)
            ) {
                var enumInfo = TypeInfo.Get(currentType);

                if (targetType.MetadataType == MetadataType.Boolean) {
                    EnumMemberInfo enumMember;
                    if (enumInfo.ValueToEnumMember.TryGetValue(0, out enumMember)) {
                        newExpression = new JSBinaryOperatorExpression(
                            JSOperator.NotEqual, ce.Expression,
                            new JSEnumLiteral(enumMember.Value, enumMember), TypeSystem.Boolean
                        );
                    } else if (enumInfo.ValueToEnumMember.TryGetValue(1, out enumMember)) {
                        newExpression = new JSBinaryOperatorExpression(
                            JSOperator.Equal, ce.Expression,
                            new JSEnumLiteral(enumMember.Value, enumMember), TypeSystem.Boolean
                        );
                    } else {
                        newExpression = new JSUntranslatableExpression(String.Format(
                            "Could not cast enum of type '{0}' to boolean because it has no zero value or one value",
                            currentType.FullName
                        ));
                    }
                } else if (ILBlockTranslator.IsNumeric(targetType)) {
                    newExpression = new JSDotExpression(
                        ce.Expression, new JSStringIdentifier("value", targetType)
                    );
                } else {
                    Debugger.Break();
                }
            } else if (
                !currentType.IsValueType &&
                targetType.MetadataType == MetadataType.Boolean
            ) {
                newExpression = new JSBinaryOperatorExpression(
                    JSBinaryOperator.NotEqual,
                    ce.Expression, new JSNullLiteral(currentType),
                    TypeSystem.Boolean
                );
            } else {
                newExpression = JSIL.Cast(ce.Expression, targetType);
            }

            if (newExpression != null) {
                ParentNode.ReplaceChild(ce, newExpression);
                VisitReplacement(newExpression);
            } else {
                Debugger.Break();
                VisitChildren(ce);
            }
        }
    }
}
