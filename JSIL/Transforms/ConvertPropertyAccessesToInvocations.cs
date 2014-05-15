using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    class ConvertPropertyAccessesToInvocations : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;

        public ConvertPropertyAccessesToInvocations (TypeSystem typeSystem, ITypeInfoSource typeInfo) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
        }

        public static bool CanConvertToInvocation (JSPropertyAccess pa) {
            var prop = pa.Property.Property;

            if (prop.IsAutoProperty &&
                !prop.IsVirtual &&
                !prop.DeclaringType.IsInterface
            ) {
                return false;
            } else {
                return
                    !prop.Metadata.HasAttribute("JSIL.Meta.JSAlwaysAccessAsProperty");
            }
        }

        private JSExpression ConstructInvocation (
            JSPropertyAccess pa, JSExpression argument = null
        ) {
            JSExpression[] arguments;

            if (argument == null)
                arguments = new JSExpression[0];
            else
                arguments = new JSExpression[] { argument };

            var originalMethod = pa.OriginalMethod;
            var declaringType = originalMethod.Reference.DeclaringType;
            var declaringTypeDef = TypeUtil.GetTypeDefinition(originalMethod.Reference.DeclaringType);
            var thisReferenceType = pa.ThisReference.GetActualType(TypeSystem);
            var isSelf = TypeUtil.TypesAreAssignable(
                TypeInfo, thisReferenceType, declaringType
            );

            // ILSpy converts compound assignments from:
            //  x.set_Value(x.get_Value + n)
            // to
            //  x.get_Value += n;
            // so we have to detect this and reconstruct the correct method
            //  references.
            var actualMethod = originalMethod;
            var correctName = String.Format(
                "{0}_{1}", pa.IsWrite ? "set" : "get",
                pa.Property.Property.ShortName
            );

            if (!actualMethod.Reference.Name.Contains(correctName)) {
                var dt = originalMethod.Method.DeclaringType;

                var actualMethodInfo = dt.Members.Values
                    .OfType<MethodInfo>().FirstOrDefault(
                        (m) => (
                            m.Name.Contains(correctName) &&
                            m.DeclaringType == originalMethod.Method.DeclaringType
                        )
                    );

                MethodReference actualMethodReference = actualMethodInfo.Member;
                if (originalMethod.Reference is GenericInstanceMethod) {
                    throw new InvalidDataException("Reconstructing an invocation of a generic instance method? Shouldn't be possible.");
                } else if (declaringType is GenericInstanceType) {
                    var declaringGit = (GenericInstanceType)declaringType;
                    var returnType = actualMethodReference.ReturnType;

                    if (TypeUtil.IsOpenType(returnType)) {
                        var actualReturnType = JSExpression.SubstituteTypeArgs(TypeInfo, actualMethodReference.ReturnType, actualMethodReference);
                        returnType = actualReturnType;
                    }

                    actualMethodReference = new MethodReference(
                        actualMethodReference.Name, returnType, declaringGit
                    );
                }

                actualMethod = new JSMethod(
                    actualMethodReference, actualMethodInfo,
                    originalMethod.MethodTypes, originalMethod.GenericArguments
                );
            }

            bool needsExplicitThis = !pa.IsVirtualCall && ILBlockTranslator.NeedsExplicitThis(
                declaringType, declaringTypeDef,
                originalMethod.Method.DeclaringType,
                isSelf, thisReferenceType,
                originalMethod.Method
            );
                
            JSInvocationExpressionBase invocation;
            if (pa.Property.Property.IsStatic)
                invocation = JSInvocationExpression.InvokeStatic(
                    actualMethod.Reference.DeclaringType,
                    actualMethod, 
                    arguments
                );
            else if (needsExplicitThis)
                invocation = JSInvocationExpression.InvokeBaseMethod(
                    actualMethod.Reference.DeclaringType,
                    actualMethod, pa.ThisReference,
                    arguments
                );
            else
                invocation = JSInvocationExpression.InvokeMethod(
                    pa.OriginalType, actualMethod, pa.ThisReference, arguments
                );

            JSExpression replacement;
            if (TypeUtil.IsStruct(pa.Property.Property.ReturnType))
                replacement = new JSResultReferenceExpression(invocation);
            else
                replacement = invocation;

            return replacement;
        }

        public bool IsPropertyGetterInvocation (JSPropertyAccess pa) {
            if (Stack.OfType<JSBinaryOperatorExpression>().Any(
                (boe) => 
                    (boe.Operator is JSAssignmentOperator) &&
                    (boe.Left.SelfAndChildrenRecursive.Contains(pa))
            ))
                return false;
            else if (Stack.OfType<JSUnaryOperatorExpression>().Any(
                (uoe) => 
                    uoe.Operator is JSUnaryMutationOperator
            ))
                return false;

            return !pa.IsWrite &&
                CanConvertToInvocation(pa);
        }

        public void VisitNode (JSPropertyAccess pa) {
            if (IsPropertyGetterInvocation(pa)) {
                // getter
                var replacement = ConstructInvocation(pa);

                ParentNode.ReplaceChild(pa, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(pa);
            }
        }

        public bool IsPropertySetterInvocation (JSBinaryOperatorExpression boe, out JSPropertyAccess pa) {
            var isValidParent =
                (ParentNode is JSExpressionStatement) ||
                (ParentNode is JSCommaExpression);

            pa = boe.Left as JSPropertyAccess;

            return (pa != null) &&
                pa.IsWrite &&
                (boe.Operator == JSOperator.Assignment) &&
                isValidParent &&
                CanConvertToInvocation(pa);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            JSPropertyAccess pa;
            if (IsPropertySetterInvocation(boe, out pa)) {
                // setter
                var invocation = ConstructInvocation(pa, boe.Right);

                ParentNode.ReplaceChild(boe, invocation);
                VisitReplacement(invocation);
            } else {
                VisitChildren(boe);
            }
        }
    }
}
