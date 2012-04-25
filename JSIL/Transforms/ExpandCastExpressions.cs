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
        public readonly MethodTypeFactory MethodTypeFactory;

        public ExpandCastExpressions (TypeSystem typeSystem, JSSpecialIdentifiers js, JSILIdentifier jsil, ITypeInfoSource typeInfo, MethodTypeFactory methodTypeFactory) {
            TypeSystem = typeSystem;
            JS = js;
            JSIL = jsil;
            TypeInfo = typeInfo;
            MethodTypeFactory = methodTypeFactory;
        }

        public void VisitNode (JSCastExpression ce) {
            var currentType = ce.Expression.GetActualType(TypeSystem);
            var targetType = ce.NewType;

            JSExpression newExpression = null;

            if (targetType.MetadataType == MetadataType.Char) {
                newExpression = JSInvocationExpression.InvokeStatic(
                    JS.fromCharCode, new[] { ce.Expression }, true
                );
            } else if (
                (currentType.MetadataType == MetadataType.Char) &&
                TypeUtil.IsIntegral(targetType)
            ) {
                newExpression = JSInvocationExpression.InvokeMethod(
                    JS.charCodeAt, ce.Expression, new[] { JSLiteral.New(0) }, true
                );
            } else if (
                IntroduceEnumCasts.IsEnumOrNullableEnum(currentType)
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
                } else if (TypeUtil.IsNumeric(targetType)) {
                    newExpression = JSInvocationExpression.InvokeStatic(
                        JS.Number(targetType), new[] { ce.Expression }, true
                    );
                } else if (targetType.FullName == "System.Enum") {
                    newExpression = ce.Expression;
                } else {
                    // Debugger.Break();
                }
            } else if (
                targetType.MetadataType == MetadataType.Boolean
            ) {
                newExpression = new JSBinaryOperatorExpression(
                    JSBinaryOperator.NotEqual,
                    ce.Expression, new JSDefaultValueLiteral(currentType),
                    TypeSystem.Boolean
                );
            } else if (
                TypeUtil.IsNumeric(targetType) &&
                TypeUtil.IsNumeric(currentType)
            ) {
                if (currentType == TypeSystem.Int64) {
                    if (TypeUtil.IsIntegral(targetType)) {
                        newExpression = JSInvocationExpression
                            .InvokeMethod(TypeSystem.Int64, new JSFakeMethod("toInt", TypeSystem.Int32, new TypeReference[] { }, MethodTypeFactory), ce.Expression);
                    }
                    else {
                        newExpression = JSInvocationExpression
                            .InvokeMethod(TypeSystem.Int64, new JSFakeMethod("toNumber", TypeSystem.Double, new TypeReference[] { }, MethodTypeFactory), ce.Expression);
                    }
                }
                else if (targetType == TypeSystem.Int64) {
                    if (TypeUtil.IsIntegral(currentType)) {
                        newExpression = JSInvocationExpression.InvokeStatic(
                            JSAstBuilder.StringIdentifier("goog").Dot("math").Dot("Long").FakeMethod("fromInt", TypeSystem.Int64, new[] { currentType }, MethodTypeFactory).GetExpression(),
                            new[] { ce.Expression });
                    }
                    else {
                        newExpression = JSInvocationExpression.InvokeStatic(
                            JSAstBuilder.StringIdentifier("goog").Dot("math").Dot("Long").FakeMethod("fromNumber", TypeSystem.Int64, new[] { currentType }, MethodTypeFactory).GetExpression(),
                            new[] { ce.Expression });
                    }
                }
                else if (
                    TypeUtil.IsIntegral(currentType) ||
                    !TypeUtil.IsIntegral(targetType)) {
                    newExpression = ce.Expression;
                }
                else {
                    newExpression = JSInvocationExpression.InvokeStatic(JS.floor, new[] { ce.Expression }, true);
                }
            } else {
                newExpression = JSIL.Cast(ce.Expression, targetType);
            }

            if (newExpression != null) {
                ParentNode.ReplaceChild(ce, newExpression);
                VisitReplacement(newExpression);
            } else {
                // Debugger.Break();
                VisitChildren(ce);
            }
        }
    }
}
