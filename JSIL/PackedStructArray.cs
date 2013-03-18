using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    }
}
