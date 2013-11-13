using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Internal {
    public static class PackedArrayUtil {
        public static bool IsPackedArrayType (TypeReference type) {
            return type.FullName.StartsWith("JSIL.Runtime.IPackedArray");
        }

        public static TypeReference GetElementType (TypeReference arrayType) {
            var git = (GenericInstanceType)arrayType;
            return git.GenericArguments[0];
        }

        public static TypeReference MakePackedArrayType (TypeReference arrayType, TypeReference attributeType) {
            var at = arrayType as ArrayType;
            if (at == null)
                throw new InvalidOperationException("Cannot apply JSPackedArray to a non-array type");

            var elementType = at.ElementType;
            if (!TypeUtil.ExpandPositionalGenericParameters(elementType, out elementType))
                elementType = at.ElementType;
            
            if (!TypeUtil.IsStruct(elementType))
                throw new InvalidOperationException("Cannot apply JSPackedArray to a non-struct array");

            var attributeTypeDefinition = attributeType.Resolve();
            var linkedTypeAttribute = attributeTypeDefinition.CustomAttributes.First(
                (ca) => ca.AttributeType.FullName == "JSIL.Runtime.LinkedTypeAttribute"
            );

            var openTypeReference = (TypeReference)linkedTypeAttribute.ConstructorArguments[0].Value;

            var closedTypeReference = new GenericInstanceType(openTypeReference);
            closedTypeReference.GenericArguments.Add(at.ElementType);

            return closedTypeReference;
        }

        public static void CheckInvocationSafety (MethodInfo method, JSExpression[] argumentValues, TypeSystem typeSystem) {
            if (method.Metadata.HasAttribute("JSIL.Meta.JSAllowPackedArrayArgumentsAttribute"))
                return;

            TypeReference temp;
            string[] argumentNames = GetPackedArrayArgumentNames(method, out temp);

            for (var i = 0; i < method.Parameters.Length; i++) {
                if (i >= argumentValues.Length)
                    continue;

                var valueType = argumentValues[i].GetActualType(typeSystem);

                if (!IsPackedArrayType(valueType)) {
                    if ((argumentNames != null) && argumentNames.Contains(method.Parameters[i].Name))
                        throw new ArgumentException(
                            "Invalid attempt to pass a normal array as parameter '" + method.Parameters[i].Name + "' to method '" + method.Name + "'. " +
                            "This parameter must be a packed array."
                        );
                } else {
                    if ((argumentNames == null) || !argumentNames.Contains(method.Parameters[i].Name))
                        throw new ArgumentException(
                            "Invalid attempt to pass a packed array as parameter '" + method.Parameters[i].Name + "' to method '" + method.Name + "'. " +
                            "If this is intentional, annotate the method with the JSPackedArrayArguments attribute."
                        );
                }
            }
        }

        public static string[] GetPackedArrayArgumentNames (MethodInfo method, out TypeReference packedArrayAttributeType) {
            packedArrayAttributeType = null;

            AttributeGroup packedArrayAttribute;
            if (
                (method != null) &&
                ((packedArrayAttribute = method.Metadata.GetAttribute("JSIL.Meta.JSPackedArrayArgumentsAttribute")) != null)
            ) {
                var result = new List<string>();

                foreach (var entry in packedArrayAttribute.Entries) {
                    packedArrayAttributeType = entry.Type;

                    var argumentNames = entry.Arguments[0].Value as IList<CustomAttributeArgument>;
                    if (argumentNames == null)
                        throw new ArgumentException("Arguments to JSPackedArrayArguments must be strings");

                    foreach (var attributeArgument in argumentNames) {
                        var argumentName = attributeArgument.Value as string;
                        if (argumentName == null)
                            throw new ArgumentException("Arguments to JSPackedArrayArguments must be strings");

                        result.Add(argumentName);
                    }
                }

                return result.ToArray();
            }

            return null;
        }

        public static JSExpression FilterInvocationResult (
            MethodReference methodReference, MethodInfo method, 
            JSExpression result, 
            ITypeInfoSource typeInfo, TypeSystem typeSystem
        ) {
            if (method == null)
                return result;

            var resultType = result.GetActualType(typeSystem);
            var resultIsPackedArray = PackedArrayUtil.IsPackedArrayType(resultType);
            var returnValueAttribute = method.Metadata.GetAttribute("JSIL.Meta.JSPackedArrayReturnValueAttribute");

            if (returnValueAttribute != null) {
                if (TypeUtil.IsOpenType(resultType)) {
                    // FIXME: We need to restrict substitution to when the result type is a generic parameter owned by the invocation...
                    resultType = JSExpression.SubstituteTypeArgs(typeInfo, resultType, methodReference);
                }

                if (!resultIsPackedArray)
                    return JSChangeTypeExpression.New(result, PackedArrayUtil.MakePackedArrayType(resultType, returnValueAttribute.Entries.First().Type), typeSystem);
            }

            return result;
        }

        public static void CheckReturnValue (MethodInfo method, JSExpression returnValue, TypeSystem typeSystem) {
            if (method == null)
                return;

            var resultType = returnValue.GetActualType(typeSystem);
            var resultIsPackedArray = PackedArrayUtil.IsPackedArrayType(resultType);

            var returnValueAttribute = method.Metadata.GetAttribute("JSIL.Meta.JSPackedArrayReturnValueAttribute");
            if (returnValueAttribute != null) {
                if (!resultIsPackedArray)
                    throw new ArgumentException(
                        "Return value of method '" + method.Name + "' must be a packed array."
                    );
            } else {
                if (resultIsPackedArray)
                    throw new ArgumentException(
                        "Return value of method '" + method.Name + "' is a packed array. " + 
                        "For this to be valid you must attach a JSPackedArrayReturnValueAttribute to the method."
                    );
            }
        }

        public static JSExpression GetItem (TypeReference targetType, TypeInfo targetTypeInfo, JSExpression target, JSExpression index, MethodTypeFactory methodTypes, bool proxy = false) {
            var targetGit = (GenericInstanceType)targetType;
            var getMethodName = "get_Item";
            var getMethod = (JSIL.Internal.MethodInfo)targetTypeInfo.Members.First(
                (kvp) => kvp.Key.Name == getMethodName
            ).Value;

            if (proxy)
                return new JSNewPackedArrayElementProxy(
                    target, index, targetGit.GenericArguments[0]
                );

            // We have to construct a custom reference to the method in order for ILSpy's
            //  SubstituteTypeArgs method not to explode later on
            var getMethodReference = CecilUtil.RebindMethod(getMethod.Member, targetGit, targetGit.GenericArguments[0]);

            var result = JSInvocationExpression.InvokeMethod(
                new JSType(targetType),
                new JSMethod(getMethodReference, getMethod, methodTypes),
                target,
                new JSExpression[] { index },
                constantIfArgumentsAre: proxy
            );

            return result;
        }

        public static bool IsElementProxy (JSExpression expression) {
            return expression.SelfAndChildrenRecursive.OfType<JSNewPackedArrayElementProxy>().Any() ||
                expression.SelfAndChildrenRecursive.OfType<JSRetargetPackedArrayElementProxy>().Any();
        }
    }
}
