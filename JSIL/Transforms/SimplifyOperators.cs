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

        public void VisitNode (JSInvocationExpression ie) {
            var type = ie.JSType;
            var method = ie.JSMethod;
            var thisExpression = ie.ThisReference;

            if (method != null) {
                if (
                    (type != null) &&
                    ILBlockTranslator.TypesAreEqual(TypeSystem.String, type.Type) &&
                    (method.Method.Name == "Concat")
                ) {
                    if (ie.Arguments.Count > 2) {
                        if (ie.Arguments.All(
                            (arg) => ILBlockTranslator.TypesAreEqual(
                                TypeSystem.String, arg.GetExpectedType(TypeSystem)
                            )
                        )) {
                            var boe = JSBinaryOperatorExpression.New(
                                JSOperator.Add,
                                ie.Arguments,
                                TypeSystem.String
                            );

                            ParentNode.ReplaceChild(
                                ie,
                                boe
                            );

                            VisitReplacement(boe);
                        }
                    } else if (
                        ie.Arguments.Count == 2
                    ) {
                        var lhs = ie.Arguments[0];
                        var lhsType = ILBlockTranslator.DereferenceType(lhs.GetExpectedType(TypeSystem));
                        if (!(
                            ILBlockTranslator.TypesAreEqual(TypeSystem.String, lhsType) ||
                            ILBlockTranslator.TypesAreEqual(TypeSystem.Char, lhsType)
                        )) {
                            lhs = JSInvocationExpression.InvokeMethod(lhsType, JS.toString, lhs, null);
                        }

                        var rhs = ie.Arguments[1];
                        var rhsType = ILBlockTranslator.DereferenceType(rhs.GetExpectedType(TypeSystem));
                        if (!(
                            ILBlockTranslator.TypesAreEqual(TypeSystem.String, rhsType) ||
                            ILBlockTranslator.TypesAreEqual(TypeSystem.Char, rhsType)
                        )) {
                            rhs = JSInvocationExpression.InvokeMethod(rhsType, JS.toString, rhs, null);
                        }

                        var boe = new JSBinaryOperatorExpression(
                            JSOperator.Add, lhs, rhs, TypeSystem.String
                        );

                        ParentNode.ReplaceChild(
                            ie, boe
                        );

                        VisitReplacement(boe);
                    } else if (
                        ILBlockTranslator.GetTypeDefinition(ie.Arguments[0].GetExpectedType(TypeSystem)).FullName == "System.Array"
                    ) {
                    } else {
                        var firstArg = ie.Arguments.FirstOrDefault();

                        ParentNode.ReplaceChild(
                            ie, firstArg
                        );

                        if (firstArg != null)
                            VisitReplacement(firstArg);
                    }
                    return;
                } else if (
                    ILBlockTranslator.IsDelegateType(method.Reference.DeclaringType) &&
                    (method.Method.Name == "Invoke")
                ) {
                    var newIe = new JSDelegateInvocationExpression(
                        thisExpression, ie.GetExpectedType(TypeSystem), ie.Arguments.ToArray()
                    );
                    ParentNode.ReplaceChild(ie, newIe);

                    VisitReplacement(newIe);
                    return;
                } else if (
                    (method.Reference.DeclaringType.FullName == "System.Type") &&
                    (method.Method.Name == "GetTypeFromHandle")
                ) {
                    var typeObj = ie.Arguments.FirstOrDefault();
                    ParentNode.ReplaceChild(ie, typeObj);

                    VisitReplacement(typeObj);
                    return;
                } else if (
                    (method.Reference.DeclaringType.Name == "RuntimeHelpers") &&
                    (method.Method.Name == "InitializeArray")
                ) {
                    var array = ie.Arguments[0];
                    var arrayType = array.GetExpectedType(TypeSystem);
                    var field = ie.Arguments[1].AllChildrenRecursive.OfType<JSField>().First();
                    var initializer = JSArrayExpression.UnpackArrayInitializer(arrayType, field.Field.Member.InitialValue);

                    var copy = JSIL.ShallowCopy(array, initializer, arrayType); 
                    ParentNode.ReplaceChild(ie, copy);
                    VisitReplacement(copy);
                    return;
                } else if (
                    method.Method.DeclaringType.Definition.FullName == "System.Array" &&
                    (ie.Arguments.Count == 1)
                ) {
                    switch (method.Method.Name) {
                        case "GetLength":
                        case "GetUpperBound": {
                            var index = ie.Arguments[0] as JSLiteral;
                            if (index != null) {
                                var newDot = JSDotExpression.New(thisExpression, new JSStringIdentifier(
                                    String.Format("length{0}", Convert.ToInt32(index.Literal)), 
                                    TypeSystem.Int32
                                ));

                                if (method.Method.Name == "GetUpperBound") {
                                    var newExpr = new JSBinaryOperatorExpression(
                                        JSOperator.Subtract, newDot, JSLiteral.New(1), TypeSystem.Int32
                                    );
                                    ParentNode.ReplaceChild(ie, newExpr);
                                } else {
                                    ParentNode.ReplaceChild(ie, newDot);
                                }
                            }
                            break;
                        }
                        case "GetLowerBound":
                            ParentNode.ReplaceChild(ie, JSLiteral.New(0));
                            break;
                    }
                }
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var isBoolean = ILBlockTranslator.IsBoolean(uoe.GetExpectedType(TypeSystem));

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
                        boe.GetExpectedType(TypeSystem)
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
                        boe.GetExpectedType(TypeSystem)
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
                    var newInvocation = JSInvocationExpression.InvokeMethod(
                        new JSStringIdentifier("__Initialize__", boe.Left.GetExpectedType(TypeSystem)), 
                        boe.Left, new [] { new JSArrayExpression(
                            TypeSystem.Object,
                            rightNew.Arguments.ToArray()
                        ) }
                    );

                    ParentNode.ReplaceChild(boe, newInvocation);
                    VisitReplacement(newInvocation);

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
