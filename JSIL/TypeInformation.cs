using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace JSIL.Internal {
    public interface ITypeInfoSource {
        TypeInfo Get (TypeReference type);
    }

    public class TypeInfo {
        // Method overloading information
        public readonly Dictionary<string, MethodGroupInfo> MethodGroups = new Dictionary<string, MethodGroupInfo>();
        public readonly Dictionary<MethodReference, MethodGroupInfo> MethodToMethodGroup = new Dictionary<MethodReference, MethodGroupInfo>();

        // Enum information
        public readonly bool IsFlagsEnum;
        public readonly Dictionary<long, EnumMemberInfo> ValueToEnumMember = new Dictionary<long, EnumMemberInfo>();
        public readonly Dictionary<string, EnumMemberInfo> EnumMembers = new Dictionary<string, EnumMemberInfo>();

        public TypeInfo (TypeDefinition type) {
            if (type.IsEnum) {
                long enumValue = 0;
                foreach (var field in type.Fields) {
                    // Skip 'value__'
                    if (field.IsRuntimeSpecialName)
                        continue;

                    if (field.HasConstant)
                        enumValue = Convert.ToInt64(field.Constant);

                    var info = new EnumMemberInfo(type, field.Name, enumValue);
                    ValueToEnumMember[enumValue] = info;
                    EnumMembers[field.Name] = info;

                    enumValue += 1;
                }
            }
        }
    }

    public class MethodGroupInfo {
        public readonly TypeReference DeclaringType;
        public readonly bool IsStatic;
        public readonly string Name;
    }

    public class EnumMemberInfo {
        public readonly TypeReference DeclaringType;
        public readonly string FullName;
        public readonly string Name;
        public readonly long Value;

        public EnumMemberInfo (TypeDefinition type, string name, long value) {
            DeclaringType = type;
            FullName = type.FullName + "." + name;
            Name = name;
            Value = value;
        }
    }
}
