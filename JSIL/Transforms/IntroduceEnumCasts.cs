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
        public readonly TypeInfoProvider TypeInfo;
        public readonly MethodTypeFactory MethodTypes;
        public readonly JSSpecialIdentifiers JS;

        private readonly HashSet<JSOperator> LogicalOperators;

        public IntroduceEnumCasts (TypeSystem typeSystem, JSSpecialIdentifiers js, TypeInfoProvider typeInfo, MethodTypeFactory methodTypes) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            MethodTypes = methodTypes;
            JS = js;

            LogicalOperators = new HashSet<JSOperator>() {
                JSOperator.LogicalAnd,
                JSOperator.LogicalOr,
                JSOperator.LogicalNot
            };
        }

        public static bool IsEnumOrNullableEnum (TypeReference tr) {
            tr = TypeUtil.DereferenceType(tr, false);

            if (TypeUtil.IsEnum(tr))
                return true;

            var git = tr as GenericInstanceType;
            if ((git != null) && (git.Name == "Nullable`1")) {
                if (TypeUtil.IsEnum(git.GenericArguments[0]))
                    return true;
            }

            return false;
        }

        public void VisitNode (JSIndexerExpression ie) {
            var indexType = ie.Index.GetActualType(TypeSystem);

            if (
                !TypeUtil.IsIntegral(indexType) &&
                IsEnumOrNullableEnum(indexType)
            ) {
                var cast = JSInvocationExpression.InvokeMethod(
                    JS.valueOf(TypeSystem.Int32), ie.Index, null, true
                );

                ie.ReplaceChild(ie.Index, cast);
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var type = uoe.Expression.GetActualType(TypeSystem);
            var isEnum = IsEnumOrNullableEnum(type);

            if (isEnum) {
                var cast = JSInvocationExpression.InvokeMethod(
                    JS.valueOf(TypeSystem.Int32), uoe.Expression, null, true
                );

                if (LogicalOperators.Contains(uoe.Operator)) {
                    uoe.ReplaceChild(uoe.Expression, cast);
                } else if (uoe.Operator == JSOperator.Negation) {
                    uoe.ReplaceChild(uoe.Expression, cast);
                }
            }

            VisitChildren(uoe);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var leftIsEnum = IsEnumOrNullableEnum(leftType);
            var rightType = boe.Right.GetActualType(TypeSystem);
            var rightIsEnum = IsEnumOrNullableEnum(rightType);
            var resultType = boe.GetActualType(TypeSystem);
            var resultIsEnum = IsEnumOrNullableEnum(resultType);

            if ((leftIsEnum || rightIsEnum) && LogicalOperators.Contains(boe.Operator)) {
                if (leftIsEnum) {
                    var cast = JSInvocationExpression.InvokeMethod(
                        JS.valueOf(TypeSystem.Int32), boe.Left, null, true
                    );

                    boe.ReplaceChild(boe.Left, cast);
                }

                if (rightIsEnum) {
                    var cast = JSInvocationExpression.InvokeMethod(
                        JS.valueOf(TypeSystem.Int32), boe.Right, null, true
                    );

                    boe.ReplaceChild(boe.Right, cast);
                }
            }

            VisitChildren(boe);
        }

        public void VisitNode (JSSwitchStatement ss) {
            var conditionType = ss.Condition.GetActualType(TypeSystem);

            if (
                !TypeUtil.IsIntegral(conditionType) &&
                IsEnumOrNullableEnum(conditionType)
            ) {
                var cast = JSInvocationExpression.InvokeMethod(
                    JS.valueOf(TypeSystem.Int32), ss.Condition, null, true
                );

                ss.ReplaceChild(ss.Condition, cast);
            }

            VisitChildren(ss);
        }
    }
}
