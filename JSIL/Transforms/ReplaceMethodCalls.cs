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
    public class ReplaceMethodCalls : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly JSILIdentifier JSIL;
        public readonly JSSpecialIdentifiers JS;

        public ReplaceMethodCalls (JSILIdentifier jsil, JSSpecialIdentifiers js, TypeSystem typeSystem) {
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
                    (type.Type.FullName == "System.Object")
                ) {
                    switch (method.Method.Member.Name) {
                        case "GetType":
                            JSNode replacement;

                            var thisType = JSExpression.DeReferenceType(thisExpression.GetExpectedType(TypeSystem), false);
                            if ((thisType is GenericInstanceType) && thisType.FullName.StartsWith("System.Nullable")) {
                                replacement = new JSType(thisType);
                            } else {
                                replacement = JSIL.GetType(thisExpression);
                            }

                            ParentNode.ReplaceChild(ie, replacement);
                            VisitReplacement(replacement);

                            return;
                    }
                } else if (
                    (type != null) &&
                    (type.Type.FullName.StartsWith("System.Nullable")) &&
                    (type.Type is GenericInstanceType)
                ) {
                    var t = (type.Type as GenericInstanceType).GenericArguments[0];
                    var @null = JSLiteral.Null(t);
                    var @default = new JSDefaultValueLiteral(t);

                    switch (method.Method.Member.Name) {
                        case ".ctor":
                            JSExpression value;
                            if (ie.Arguments.Count == 0) {
                                value = @null;
                            } else {
                                value = ie.Arguments[0];
                            }

                            var boe = new JSBinaryOperatorExpression(
                                JSOperator.Assignment, ie.ThisReference, value, type.Type
                            );
                            ParentNode.ReplaceChild(ie, boe);
                            VisitReplacement(boe);

                            break;
                        case "GetValueOrDefault":
                            var isNull = new JSBinaryOperatorExpression(
                                JSOperator.Equal, ie.ThisReference, @null, TypeSystem.Boolean
                            );

                            JSTernaryOperatorExpression ternary;
                            if (ie.Arguments.Count == 0) {
                                ternary = new JSTernaryOperatorExpression(
                                    isNull, @default, ie.ThisReference, type.Type
                                );
                            } else {
                                ternary = new JSTernaryOperatorExpression(
                                    isNull, ie.Arguments[0], ie.ThisReference, type.Type
                                );
                            }

                            ParentNode.ReplaceChild(ie, ternary);
                            VisitReplacement(ternary);

                            break;
                        default:
                            throw new NotImplementedException(method.Method.Member.FullName);
                    }

                    return;
                } else if (
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
                    var field = ie.Arguments[1].SelfAndChildrenRecursive.OfType<JSField>().First();
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

        public void VisitNode (JSPropertyAccess pa) {
            var targetType = pa.Target.GetExpectedType(TypeSystem);

            if (targetType.FullName.StartsWith("System.Nullable")) {
                var @null = JSLiteral.Null(targetType);

                switch (pa.Property.Property.Member.Name) {
                    case "HasValue":
                        var replacement = new JSBinaryOperatorExpression(
                            JSOperator.NotEqual, pa.Target, @null, TypeSystem.Boolean
                        );
                        ParentNode.ReplaceChild(pa, replacement);
                        VisitReplacement(replacement);

                        break;
                    case "Value":
                        ParentNode.ReplaceChild(pa, pa.Target);
                        VisitReplacement(pa.Target);

                        break;
                    default:
                        throw new NotImplementedException(pa.Property.Property.Member.FullName);
                }

                return;
            }

            VisitChildren(pa);
        }

        public void VisitNode (JSDefaultValueLiteral dvl) {
            var expectedType = dvl.GetExpectedType(TypeSystem);
            if (
                (expectedType != null) &&
                expectedType.FullName.StartsWith("System.Nullable")
            ) {
                ParentNode.ReplaceChild(
                    dvl, JSLiteral.Null(expectedType)
                );
            } else {
                VisitChildren(dvl);
            }
        }

        public void VisitNode (JSNewExpression ne) {
            var expectedType = ne.GetExpectedType(TypeSystem);
            if (
                (expectedType != null) &&
                expectedType.FullName.StartsWith("System.Nullable")
            ) {
                if (ne.Arguments.Count == 0) {
                    ParentNode.ReplaceChild(
                        ne, JSLiteral.Null(expectedType)
                    );
                } else {
                    ParentNode.ReplaceChild(
                        ne, ne.Arguments[0]
                    );
                    VisitReplacement(ne.Arguments[0]);
                }
            } else {
                VisitChildren(ne);
            }
        }
    }
}
