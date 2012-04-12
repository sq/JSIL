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
    public class SimplifyOperators : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly JSILIdentifier JSIL;
        public readonly JSSpecialIdentifiers JS;

        public readonly Dictionary<JSBinaryOperator, JSBinaryOperator> InvertedOperators = new Dictionary<JSBinaryOperator, JSBinaryOperator> {
            { JSOperator.LessThan, JSOperator.GreaterThanOrEqual },
            { JSOperator.LessThanOrEqual, JSOperator.GreaterThan },
            { JSOperator.GreaterThan, JSOperator.LessThanOrEqual },
            { JSOperator.GreaterThanOrEqual, JSOperator.LessThan },
            { JSOperator.Equal, JSOperator.NotEqual },
            { JSOperator.NotEqual, JSOperator.Equal }
        };
        public readonly Dictionary<JSBinaryOperator, JSAssignmentOperator> CompoundAssignments = new Dictionary<JSBinaryOperator, JSAssignmentOperator> {
            { JSOperator.Add, JSOperator.AddAssignment },
            { JSOperator.Subtract, JSOperator.SubtractAssignment },
            { JSOperator.Multiply, JSOperator.MultiplyAssignment }
        };
        public readonly Dictionary<JSBinaryOperator, JSUnaryOperator> PrefixOperators = new Dictionary<JSBinaryOperator, JSUnaryOperator> {
            { JSOperator.Add, JSOperator.PreIncrement },
            { JSOperator.Subtract, JSOperator.PreDecrement }
        };

        public SimplifyOperators (JSILIdentifier jsil, JSSpecialIdentifiers js, TypeSystem typeSystem) {
            JSIL = jsil;
            JS = js;
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var isBoolean = ILBlockTranslator.IsBoolean(uoe.GetActualType(TypeSystem));

            if (isBoolean) {
                if (uoe.Operator == JSOperator.IsTrue) {
                    ParentNode.ReplaceChild(
                        uoe, uoe.Expression
                    );

                    VisitReplacement(uoe.Expression);
                    return;
                } else if (uoe.Operator == JSOperator.LogicalNot) {
                    var nestedUoe = uoe.Expression as JSUnaryOperatorExpression;
                    var boe = uoe.Expression as JSBinaryOperatorExpression;

                    JSBinaryOperator newOperator;
                    if ((boe != null) && 
                        InvertedOperators.TryGetValue(boe.Operator, out newOperator)
                    ) {
                        var newBoe = new JSBinaryOperatorExpression(
                            newOperator, boe.Left, boe.Right, boe.ExpectedType
                        );

                        ParentNode.ReplaceChild(uoe, newBoe);
                        VisitReplacement(newBoe);

                        return;
                    } else if (
                        (nestedUoe != null) && 
                        (nestedUoe.Operator == JSOperator.LogicalNot)
                    ) {
                        var nestedExpression = nestedUoe.Expression;

                        ParentNode.ReplaceChild(uoe, nestedExpression);
                        VisitReplacement(nestedExpression);

                        return;
                    }
                }
            }

            VisitChildren(uoe);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            JSExpression left, right, nestedLeft;

            if (!JSReferenceExpression.TryDereference(JSIL, boe.Left, out left))
                left = boe.Left;
            if (!JSReferenceExpression.TryDereference(JSIL, boe.Right, out right))
                right = boe.Right;

            var nestedBoe = right as JSBinaryOperatorExpression;
            var isAssignment = (boe.Operator == JSOperator.Assignment);
            var leftNew = left as JSNewExpression;
            var rightNew = right as JSNewExpression;
            var leftVar = left as JSVariable;

            if (
                isAssignment && (nestedBoe != null) && 
                (left.IsConstant || (leftVar != null) || left is JSDotExpression) &&
                !(ParentNode is JSVariableDeclarationStatement)
            ) {
                JSUnaryOperator prefixOperator;
                JSAssignmentOperator compoundOperator;

                if (!JSReferenceExpression.TryDereference(JSIL, nestedBoe.Left, out nestedLeft))
                    nestedLeft = nestedBoe.Left;

                var rightLiteral = nestedBoe.Right as JSIntegerLiteral;
                var areEqual = left.Equals(nestedLeft);

                if (
                    areEqual &&
                    PrefixOperators.TryGetValue(nestedBoe.Operator, out prefixOperator) &&
                    (rightLiteral != null) && (rightLiteral.Value == 1)
                ) {
                    var newUoe = new JSUnaryOperatorExpression(
                        prefixOperator, boe.Left,
                        boe.GetActualType(TypeSystem)
                    );

                    ParentNode.ReplaceChild(boe, newUoe);
                    VisitReplacement(newUoe);

                    return;
                } else if (
                    areEqual && 
                    CompoundAssignments.TryGetValue(nestedBoe.Operator, out compoundOperator)
                ) {
                    var newBoe = new JSBinaryOperatorExpression(
                        compoundOperator, boe.Left, nestedBoe.Right,
                        boe.GetActualType(TypeSystem)
                    );

                    ParentNode.ReplaceChild(boe, newBoe);
                    VisitReplacement(newBoe);

                    return;
                }
            } else if (
                isAssignment && (leftNew != null) &&
                (rightNew != null)
            ) {
                var rightType = rightNew.Type as JSDotExpression;
                if (
                    (rightType != null) &&
                    (rightType.Member.Identifier == "CollectionInitializer")
                ) {
                    var newInitializer = new JSInitializerApplicationExpression(
                        boe.Left, new JSArrayExpression(
                            TypeSystem.Object,
                            rightNew.Arguments.ToArray()
                        )
                    );

                    ParentNode.ReplaceChild(boe, newInitializer);
                    VisitReplacement(newInitializer);

                    return;
                }
            } else if (
                isAssignment && (leftVar != null) &&
                leftVar.IsThis
            ) {
                var leftType = leftVar.GetActualType(TypeSystem);
                if (!EmulateStructAssignment.IsStruct(leftType)) {
                    ParentNode.ReplaceChild(boe, new JSUntranslatableExpression(boe));

                    return;
                } else {
                    var newInvocation = JSInvocationExpression.InvokeStatic(
                        JSIL.CopyMembers, new[] { boe.Right, leftVar }
                    );

                    ParentNode.ReplaceChild(boe, newInvocation);
                    VisitReplacement(newInvocation);

                    return;
                }
            }

            VisitChildren(boe);
        }
    }
}
