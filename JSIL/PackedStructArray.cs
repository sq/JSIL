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

        public static TypeReference MakePackedArrayType (TypeReference arrayType, TypeReference attributeType) {
            var at = arrayType as ArrayType;
            if (at == null)
                throw new InvalidOperationException("Cannot apply JSPackedArray to a non-array type");
            
            if (!TypeUtil.IsStruct(at.ElementType))
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
    }
}
