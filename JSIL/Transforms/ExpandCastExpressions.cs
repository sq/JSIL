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

            if (targetType.FullName == "System.ValueType") {
                var replacement = ce.Expression;
                ParentNode.ReplaceChild(ce, replacement);
                VisitReplacement(replacement);
                return;
            } else if (targetType.MetadataType == MetadataType.Char) {
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
                var isNullable = TypeUtil.IsNullable(currentType);

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
                    if (isNullable) {
                        newExpression = JSIL.ValueOfNullable(
                            ce.Expression
                        );
                    } else {
                        newExpression = JSInvocationExpression.InvokeMethod(
                            JS.valueOf(targetType), ce.Expression, null, true
                        );
                    }
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
                TypeUtil.IsNumeric(currentType) &&
                targetType != currentType
            ) {
                if (currentType.MetadataType == MetadataType.Int64) {
                    if (targetType.MetadataType == MetadataType.UInt64) {
                        newExpression = JSInvocationExpression
                            .InvokeMethod(
                                TypeSystem.Int64,
                                new JSFakeMethod("ToUInt64", TypeSystem.Int32, new TypeReference[] { }, MethodTypeFactory),
                                ce.Expression);
                    }
                    else {
                        newExpression = JSInvocationExpression
                            .InvokeMethod(
                                TypeSystem.Int64,
                                new JSFakeMethod("ToNumber", TypeSystem.Int32, new TypeReference[] { }, MethodTypeFactory),
                                ce.Expression);
                    }
                }
                else if (currentType.MetadataType == MetadataType.UInt64) { 
                    if (targetType.MetadataType == MetadataType.Int64) { 
                        newExpression = JSInvocationExpression
                            .InvokeMethod(
                                TypeSystem.Int64,
                                new JSFakeMethod("ToInt64", TypeSystem.Int32, new TypeReference[] { }, MethodTypeFactory),
                                ce.Expression);
                    }
                    else {
                        newExpression = JSInvocationExpression
                            .InvokeMethod(
                                TypeSystem.Int64,
                                new JSFakeMethod("ToNumber", TypeSystem.Int32, new TypeReference[] { }, MethodTypeFactory),
                                ce.Expression);
                    }
                }
                else if (targetType.MetadataType == MetadataType.Int64) {
                    newExpression = JSInvocationExpression.InvokeStatic(
                        new JSType(TypeSystem.Int64),
                        new JSFakeMethod("FromNumber", TypeSystem.Int64, new[] { currentType }, MethodTypeFactory),
                        new[] { ce.Expression },
                        true);
                }
                else if (targetType.MetadataType == MetadataType.UInt64) {
                    newExpression = JSInvocationExpression.InvokeStatic(
                        new JSType(TypeSystem.UInt64),
                        new JSFakeMethod("FromNumber", TypeSystem.UInt64, new[] { currentType }, MethodTypeFactory),
                        new[] { ce.Expression },
                        true);
                }
                else if (
                    TypeUtil.IsIntegral(currentType) ||
                    !TypeUtil.IsIntegral(targetType))
                {
                    newExpression = ce.Expression;
                }
                else
                {
                    newExpression = new JSTruncateExpression(ce.Expression);
                }
            } else {
                // newExpression = JSIL.Cast(ce.Expression, targetType);
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
