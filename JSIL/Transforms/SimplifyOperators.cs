using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class SimplifyOperators : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
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

        public SimplifyOperators (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSInvocationExpression ie) {
            var target = ie.Target as JSDotExpression;
            if (target != null) {
                var type = target.Target as JSType;
                var method = target.Member as JSMethod;

                if (
                    (type != null) && (method != null) &&
                    ILBlockTranslator.TypesAreEqual(TypeSystem.String, type.Type) &&
                    (method.Method.Name == "Concat") &&
                    (ie.Arguments.All(
                        (arg) => ILBlockTranslator.TypesAreEqual(
                            TypeSystem.String, arg.GetExpectedType(TypeSystem)
                        )
                    ))
                ) {
                    var boe = JSBinaryOperatorExpression.New(
                        JSOperator.Add,
                        ie.Arguments,
                        TypeSystem.String
                    );

                    ParentNode.ReplaceChild(
                        ie,
                        boe
                    );

                    VisitChildren(boe);
                    return;
                }
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var isBoolean = ILBlockTranslator.TypesAreEqual(
                TypeSystem.Boolean, 
                uoe.Expression.GetExpectedType(TypeSystem)
            );

            if (isBoolean) {
                if (uoe.Operator == JSOperator.IsTrue) {
                    ParentNode.ReplaceChild(
                        uoe, uoe.Expression
                    );

                    Visit(uoe.Expression);
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
                        Visit(newBoe);

                        return;
                    } else if (
                        (nestedUoe != null) && 
                        (nestedUoe.Operator == JSOperator.LogicalNot)
                    ) {
                        var nestedExpression = nestedUoe.Expression;

                        ParentNode.ReplaceChild(uoe, nestedExpression);
                        Visit(nestedExpression);

                        return;
                    }
                }
            }

            VisitChildren(uoe);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var nestedBoe = boe.Right as JSBinaryOperatorExpression;

            if (
                (boe.Operator == JSOperator.Assignment) && 
                (nestedBoe != null) && (boe.Left.IsConstant)
            ) {
                JSUnaryOperator prefixOperator;
                JSAssignmentOperator compoundOperator;
                var rightLiteral = nestedBoe.Right as JSIntegerLiteral;
                var areEqual = boe.Left.Equals(nestedBoe.Left);

                if (
                    areEqual &&
                    PrefixOperators.TryGetValue(nestedBoe.Operator, out prefixOperator) &&
                    (rightLiteral != null) && (rightLiteral.Value == 1)
                ) {
                    var newUoe = new JSUnaryOperatorExpression(
                        prefixOperator, boe.Left,
                        boe.GetExpectedType(TypeSystem)
                    );

                    ParentNode.ReplaceChild(boe, newUoe);
                    Visit(newUoe);

                    return;
                } else if (
                    areEqual && 
                    CompoundAssignments.TryGetValue(nestedBoe.Operator, out compoundOperator)
                ) {
                    var newBoe = new JSBinaryOperatorExpression(
                        compoundOperator, boe.Left, nestedBoe.Right,
                        boe.GetExpectedType(TypeSystem)
                    );

                    ParentNode.ReplaceChild(boe, newBoe);
                    Visit(newBoe);

                    return;
                }
            }

            VisitChildren(boe);
        }
    }
}
