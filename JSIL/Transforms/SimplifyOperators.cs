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

        public SimplifyOperators (JSILIdentifier jsil, TypeSystem typeSystem) {
            JSIL = jsil;
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
                    if (ie.Arguments.Count >= 2) {
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
                    } else {
                        var firstArg = ie.Arguments.FirstOrDefault();

                        ParentNode.ReplaceChild(
                            ie, firstArg
                        );

                        if (firstArg != null)
                            VisitChildren(firstArg);
                    }
                    return;
                } else if (
                    (method != null) &&
                    ILBlockTranslator.IsDelegateType(method.Method.DeclaringType) &&
                    ILBlockTranslator.IsDelegateType(target.Target.GetExpectedType(TypeSystem)) &&
                    (method.Method.Name == "Invoke")
                ) {
                    ie.ReplaceChild(target, target.Target);
                } else if (
                    (method != null) &&
                    (method.Method.DeclaringType.FullName == "System.Type") &&
                    (method.Method.Name == "GetTypeFromHandle")
                ) {
                    var typeObj = ie.Arguments.FirstOrDefault();
                    ParentNode.ReplaceChild(ie, typeObj);

                    Visit(typeObj);
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
            JSExpression left, right;

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
                (boe.Left.IsConstant || (leftVar != null))
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
            } else if (
                isAssignment && (leftNew != null) &&
                (rightNew != null)
            ) {
                var rightType = rightNew.Type as JSDotExpression;
                if (
                    (rightType != null) &&
                    (rightType.Member.Identifier == "CollectionInitializer")
                ) {
                    var newInvocation = new JSInvocationExpression(
                        new JSDotExpression(
                            boe.Left,
                            new JSIdentifier("__Initialize__", boe.Left.GetExpectedType(TypeSystem))
                        ),
                        new JSArrayExpression(
                            TypeSystem.Object,
                            rightNew.Arguments.ToArray()
                        )
                    );

                    ParentNode.ReplaceChild(boe, newInvocation);
                    Visit(newInvocation);

                    return;
                }
            } else if (
                isAssignment && (leftVar != null) &&
                leftVar.IsThis
            ) {
                var leftType = leftVar.GetExpectedType(TypeSystem);
                if (!EmulateStructAssignment.IsStruct(leftType)) {
                    ParentNode.ReplaceChild(boe, new JSUntranslatableExpression(boe));

                    return;
                } else {
                    var newInvocation = new JSInvocationExpression(
                        JSIL.CopyMembers,
                        boe.Right, leftVar
                    );

                    ParentNode.ReplaceChild(boe, newInvocation);
                    Visit(newInvocation);

                    return;
                }
            }

            VisitChildren(boe);
        }
    }
}
