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
        public readonly MethodReference Method;

        public ReplaceMethodCalls (
            MethodReference method, JSILIdentifier jsil, JSSpecialIdentifiers js, TypeSystem typeSystem
        ) {
            Method = method;
            JSIL = jsil;
            JS = js;
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSPublicInterfaceOfExpression poe) {
            VisitChildren(poe);

            // Replace foo.__Type__.__PublicInterface__ with foo
            var innerTypeOf = poe.Inner as ITypeOfExpression;
            if (innerTypeOf != null) {
                var replacement = new JSType(innerTypeOf.Type);

                ParentNode.ReplaceChild(poe, replacement);
                VisitReplacement(replacement);
            }
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
                        case ".ctor": {
                            var replacement = new JSNullExpression();
                            ParentNode.ReplaceChild(ie, replacement);
                            VisitReplacement(replacement);

                            return;
                        }

                        case "ReferenceEquals": {
                            var lhs = ie.Arguments[0];
                            var rhs = ie.Arguments[1];

                            var lhsType = lhs.GetActualType(TypeSystem);
                            var rhsType = rhs.GetActualType(TypeSystem);

                            JSNode replacement;

                            // Structs can never compare equal with ReferenceEquals
                            if (TypeUtil.IsStruct(lhsType) || TypeUtil.IsStruct(rhsType))
                                replacement = JSLiteral.New(false);
                            else
                                replacement = new JSBinaryOperatorExpression(
                                    JSBinaryOperator.Equal,
                                    lhs, rhs,
                                    TypeSystem.Boolean
                                );

                            ParentNode.ReplaceChild(ie, replacement);
                            VisitReplacement(replacement);

                            return;
                        }

                        case "GetType": {
                            JSNode replacement;

                            var thisType = JSExpression.DeReferenceType(thisExpression.GetActualType(TypeSystem), false);
                            if ((thisType is GenericInstanceType) && thisType.FullName.StartsWith("System.Nullable")) {
                                replacement = new JSType(thisType);
                            } else {
                                replacement = JSIL.GetTypeOf(thisExpression);
                            }

                            ParentNode.ReplaceChild(ie, replacement);
                            VisitReplacement(replacement);

                            return;
                        }
                    }
                } else if (
                    (type != null) &&
                    (type.Type.FullName == "System.ValueType")
                ) {
                    switch (method.Method.Member.Name) {
                        case "Equals": {
                            var replacement = JSIL.StructEquals(ie.ThisReference, ie.Arguments.First());
                            ParentNode.ReplaceChild(ie, replacement);
                            VisitReplacement(replacement);

                            return;
                        }
                    }
                } else if (
                    (type != null) &&
                    IsNullable(type.Type)
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
                    TypeUtil.TypesAreEqual(TypeSystem.String, type.Type) &&
                    (method.Method.Name == "Concat")
                ) {
                    if (ie.Arguments.Count > 2) {
                        if (ie.Arguments.All(
                            (arg) => TypeUtil.TypesAreEqual(
                                TypeSystem.String, arg.GetActualType(TypeSystem)
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
                        var lhsType = TypeUtil.DereferenceType(lhs.GetActualType(TypeSystem));
                        if (!(
                            TypeUtil.TypesAreEqual(TypeSystem.String, lhsType) ||
                            TypeUtil.TypesAreEqual(TypeSystem.Char, lhsType)
                        )) {
                            lhs = JSInvocationExpression.InvokeMethod(lhsType, JS.toString, lhs, null);
                        }

                        var rhs = ie.Arguments[1];
                        var rhsType = TypeUtil.DereferenceType(rhs.GetActualType(TypeSystem));
                        if (!(
                            TypeUtil.TypesAreEqual(TypeSystem.String, rhsType) ||
                            TypeUtil.TypesAreEqual(TypeSystem.Char, rhsType)
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
                        TypeUtil.GetTypeDefinition(ie.Arguments[0].GetActualType(TypeSystem)).FullName == "System.Array"
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
                    TypeUtil.IsDelegateType(method.Reference.DeclaringType) &&
                    (method.Method.Name == "Invoke")
                ) {
                    var newIe = new JSDelegateInvocationExpression(
                        thisExpression, ie.GetActualType(TypeSystem), ie.Arguments.ToArray()
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
                    var arrayType = array.GetActualType(TypeSystem);
                    var field = ie.Arguments[1].SelfAndChildrenRecursive.OfType<JSField>().First();
                    var initializer = JSArrayExpression.UnpackArrayInitializer(arrayType, field.Field.Member.InitialValue);

                    var copy = JSIL.ShallowCopy(array, initializer, arrayType);
                    ParentNode.ReplaceChild(ie, copy);
                    VisitReplacement(copy);
                    return;
                } else if (
                    method.Reference.DeclaringType.FullName == "System.Reflection.Assembly"
                ) {
                    switch (method.Reference.Name) {
                        case "GetExecutingAssembly": {
                            var assembly = Method.DeclaringType.Module.Assembly;
                            var asmNode = new JSReflectionAssembly(assembly);
                            ParentNode.ReplaceChild(ie, asmNode);
                            VisitReplacement(asmNode);

                            return;
                        }
                        case "GetType": {
                            switch (method.Method.Parameters.Length) {
                                case 1:
                                case 2:
                                    JSExpression throwOnFail = new JSBooleanLiteral(false);
                                    if (method.Method.Parameters.Length == 2)
                                        throwOnFail = ie.Arguments[1];

                                    var invocation = JSIL.GetTypeFromAssembly(
                                        ie.ThisReference, ie.Arguments[0], throwOnFail
                                    );
                                    ParentNode.ReplaceChild(ie, invocation);
                                    VisitReplacement(invocation);

                                    return;
                            }

                            break;
                        }
                    }
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

        protected bool IsNullable (TypeReference type) {
            var git = TypeUtil.DereferenceType(type) as GenericInstanceType;

            return (git != null) && (git.Name == "Nullable`1");
        }

        public void VisitNode (JSPropertyAccess pa) {
            var targetType = pa.Target.GetActualType(TypeSystem);

            if (IsNullable(targetType)) {
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
            var expectedType = dvl.GetActualType(TypeSystem);

            if (
                IsNullable(expectedType)
            ) {
                ParentNode.ReplaceChild(
                    dvl, JSLiteral.Null(expectedType)
                );
            } else {
                VisitChildren(dvl);
            }
        }

        public void VisitNode (JSNewExpression ne) {
            var expectedType = ne.GetActualType(TypeSystem);
            if (
                IsNullable(expectedType)
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
