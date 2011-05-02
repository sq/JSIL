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
        // Class information
        public readonly MethodDefinition StaticConstructor;
        public readonly List<MethodDefinition> Constructors = new List<MethodDefinition>();

        // Method overloading information
        public readonly Dictionary<string, MethodGroupInfo> MethodGroups = new Dictionary<string, MethodGroupInfo>();
        public readonly Dictionary<MethodDefinition, MethodGroupItem> MethodToMethodGroupItem = new Dictionary<MethodDefinition, MethodGroupItem>();

        // Enum information
        public readonly bool IsFlagsEnum;
        public readonly Dictionary<long, EnumMemberInfo> ValueToEnumMember = new Dictionary<long, EnumMemberInfo>();
        public readonly Dictionary<string, EnumMemberInfo> EnumMembers = new Dictionary<string, EnumMemberInfo>();

        public TypeInfo (TypeDefinition type) {
            var methodGroups = from m in type.Methods 
                          group m by new { 
                              Name = m.Name, IsStatic = m.IsStatic
                          } into mg select mg;

            foreach (var mg in methodGroups) {
                if (mg.Key.Name == ".ctor")
                    Constructors.AddRange(mg);

                if (mg.Count() > 1) {
                    var info = new MethodGroupInfo(type, mg.Key.Name, mg.Key.IsStatic);
                    info.Items.AddRange(mg.Select(
                        (m, i) => new MethodGroupItem(info, m, i)
                    ));

                    MethodGroups.Add(mg.Key.Name, info);

                    foreach (var m in info.Items)
                        MethodToMethodGroupItem[m.Method] = m;
                } else {
                    if (mg.Key.Name == ".cctor")
                        StaticConstructor = mg.First();
                }
            }

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
        public readonly List<MethodGroupItem> Items = new List<MethodGroupItem>();

        public MethodGroupInfo (TypeReference declaringType, string name, bool isStatic) {
            DeclaringType = declaringType;
            Name = name;
            IsStatic = isStatic;
        }
    }

    public class MethodGroupItem {
        public readonly MethodGroupInfo MethodGroup;
        public readonly MethodDefinition Method;
        public readonly int Index;
        public readonly string MangledName;

        public MethodGroupItem (MethodGroupInfo methodGroup, MethodDefinition method, int index) {
            MethodGroup = methodGroup;
            Method = method;
            Index = index;
            MangledName = String.Format("{0}_{1}", method.Name, index);
        }
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
