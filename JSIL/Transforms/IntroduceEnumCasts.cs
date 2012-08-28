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
        private readonly HashSet<JSOperator> BitwiseOperators;

        public readonly Dictionary<JSAssignmentOperator, JSBinaryOperator> ReverseCompoundAssignments = new Dictionary<JSAssignmentOperator, JSBinaryOperator> {
            { JSOperator.AddAssignment, JSOperator.Add },
            { JSOperator.SubtractAssignment, JSOperator.Subtract },
            { JSOperator.MultiplyAssignment, JSOperator.Multiply },
            { JSOperator.DivideAssignment, JSOperator.Divide },
            { JSOperator.RemainderAssignment, JSOperator.Remainder },
            { JSOperator.ShiftLeftAssignment, JSOperator.ShiftLeft },
            { JSOperator.ShiftRightAssignment, JSOperator.ShiftRight },
            { JSOperator.ShiftRightUnsignedAssignment, JSOperator.ShiftRightUnsigned },
            { JSOperator.BitwiseAndAssignment, JSOperator.BitwiseAnd },
            { JSOperator.BitwiseOrAssignment, JSOperator.BitwiseOr },
            { JSOperator.BitwiseXorAssignment, JSOperator.BitwiseXor },
        };

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

            BitwiseOperators = new HashSet<JSOperator>() {
                JSOperator.BitwiseAnd,
                JSOperator.BitwiseOr,
                JSOperator.BitwiseXor
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

        private JSBinaryOperatorExpression MakeUnaryMutation (
            JSExpression expressionToMutate, JSBinaryOperator mutationOperator,
            TypeReference type
        ) {
            var newValue = new JSBinaryOperatorExpression(
                mutationOperator, expressionToMutate, JSLiteral.New(1),
                TypeSystem.Int32
            );
            var assignment = new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                expressionToMutate, JSCastExpression.New(
                    newValue, type, TypeSystem, true
                ), type
            );

            return assignment;
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var type = uoe.Expression.GetActualType(TypeSystem);
            var isEnum = IsEnumOrNullableEnum(type);

            if (isEnum) {
                var castToInt = JSInvocationExpression.InvokeMethod(
                    JS.valueOf(TypeSystem.Int32), uoe.Expression, null, true
                );

                if (LogicalOperators.Contains(uoe.Operator)) {
                    uoe.ReplaceChild(uoe.Expression, castToInt);
                } else if (uoe.Operator == JSOperator.Negation) {
                    uoe.ReplaceChild(uoe.Expression, castToInt);
                } else if (uoe.Operator is JSUnaryMutationOperator) {
                    if (
                        (uoe.Operator == JSOperator.PreIncrement) || 
                        (uoe.Operator == JSOperator.PreDecrement)
                    ) {
                        var assignment = MakeUnaryMutation(
                            uoe.Expression,
                            (uoe.Operator == JSOperator.PreDecrement) 
                                ? JSOperator.Subtract 
                                : JSOperator.Add,
                            type
                        );

                        ParentNode.ReplaceChild(uoe, assignment);
                        VisitReplacement(assignment);
                        return;

                    } else if (
                        (uoe.Operator == JSOperator.PostIncrement) || 
                        (uoe.Operator == JSOperator.PostDecrement)
                    ) {
                        // FIXME: Terrible hack oh god (also not strict mode safe)
                        var tempVariable = new JSRawOutputIdentifier(
                            (output) => output.WriteRaw("$temp"), type
                        );
                        var makeTempCopy = new JSBinaryOperatorExpression(
                            JSOperator.Assignment, tempVariable, uoe.Expression, type
                        );
                        var assignment = MakeUnaryMutation(
                            uoe.Expression,
                            (uoe.Operator == JSOperator.PostDecrement)
                                ? JSOperator.Subtract
                                : JSOperator.Add,
                            type
                        );

                        var comma = new JSCommaExpression(
                            makeTempCopy,
                            assignment,
                            tempVariable
                        );

                        ParentNode.ReplaceChild(uoe, comma);
                        VisitReplacement(comma);
                        return;

                    } else {
                        throw new NotImplementedException("Unary mutation of enum not supported: " + uoe.ToString());
                    }
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

            var eitherIsEnum = leftIsEnum || rightIsEnum;

            var assignmentOperator = boe.Operator as JSAssignmentOperator;
            JSBinaryOperator replacementOperator;

            if (LogicalOperators.Contains(boe.Operator)) {
                if (eitherIsEnum) {
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
            } else if (BitwiseOperators.Contains(boe.Operator)) {
                var parentCast = ParentNode as JSCastExpression;
                if (resultIsEnum && ((parentCast == null) || (parentCast.NewType != resultType))) {
                    var cast = JSCastExpression.New(
                        boe, resultType, TypeSystem, true
                    );

                    ParentNode.ReplaceChild(boe, cast);
                    VisitReplacement(cast);
                }
            } else if (
                (assignmentOperator != null) &&
                ReverseCompoundAssignments.TryGetValue(assignmentOperator, out replacementOperator) &&
                leftIsEnum
            ) {
                var replacement = new JSBinaryOperatorExpression(
                    JSOperator.Assignment, boe.Left, 
                    JSCastExpression.New(
                        new JSBinaryOperatorExpression(
                            replacementOperator, boe.Left, boe.Right, TypeSystem.Int32
                        ), leftType, TypeSystem, true
                    ),
                    leftType
                );

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
                return;
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
