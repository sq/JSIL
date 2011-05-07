using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace JSIL.Internal {
    public interface ITypeInfoSource {
        ModuleInfo Get (ModuleDefinition module);
        TypeInfo Get (TypeReference type);

        bool IsIgnored (ModuleDefinition module);
        bool IsIgnored (TypeReference type);
        bool IsIgnored (MemberReference member);
    }

    internal class MemberReferenceComparer : IEqualityComparer<MemberReference> {
        public bool Equals (MemberReference x, MemberReference y) {
            return String.Equals(x.FullName, y.FullName);
        }

        public int GetHashCode (MemberReference obj) {
            return obj.FullName.GetHashCode();
        }
    }

    public static class TypeInformation {
        public static bool IsIgnoredName (string fullName) {
            if (fullName.EndsWith("__BackingField"))
                return false;
            else if (fullName.Contains("<Module>"))
                return true;
            else if (fullName.Contains("__SiteContainer"))
                return true;
            else if (fullName.StartsWith("CS$<"))
                return true;
            else if (fullName.Contains("<PrivateImplementationDetails>"))
                return true;
            else if (fullName.Contains("Runtime.CompilerServices.CallSite"))
                return true;
            else if (fullName.Contains("__CachedAnonymousMethodDelegate"))
                return true;

            return false;
        }
    }

    public class ModuleInfo {
        public readonly bool IsIgnored;
        public readonly MetadataCollection Metadata;

        public ModuleInfo (ModuleDefinition module) {
            Metadata = new MetadataCollection(module);

            IsIgnored = TypeInformation.IsIgnoredName(module.FullyQualifiedName) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore");
        }
    }

    public class TypeInfo {
        public readonly TypeDefinition Definition;
        public readonly TypeReference Reference;

        // Class information
        public readonly bool IsIgnored;
        public readonly MethodDefinition StaticConstructor;
        public readonly List<MethodDefinition> Constructors = new List<MethodDefinition>();
        public readonly MetadataCollection Metadata;

        // Method overloading information
        public readonly List<MethodGroupInfo> MethodGroups = new List<MethodGroupInfo>();
        public readonly Dictionary<MemberReference, MethodGroupItem> MethodToMethodGroupItem = new Dictionary<MemberReference, MethodGroupItem>(
            new MemberReferenceComparer()
        );

        // Property information
        public readonly Dictionary<MemberReference, PropertyDefinition> MethodToProperty = new Dictionary<MemberReference, PropertyDefinition>(
            new MemberReferenceComparer()
        );

        // Event information
        public readonly Dictionary<MemberReference, EventDefinition> MethodToEvent = new Dictionary<MemberReference, EventDefinition>(
            new MemberReferenceComparer()
        );

        // Enum information
        public readonly bool IsFlagsEnum;
        public readonly Dictionary<long, EnumMemberInfo> ValueToEnumMember = new Dictionary<long, EnumMemberInfo>();
        public readonly Dictionary<string, EnumMemberInfo> EnumMembers = new Dictionary<string, EnumMemberInfo>();

        // General member information
        public readonly HashSet<MemberReference> IgnoredMembers = new HashSet<MemberReference>(
            new MemberReferenceComparer()
        );
        public readonly Dictionary<MemberReference, MetadataCollection> MemberMetadata = new Dictionary<MemberReference, MetadataCollection>(
            new MemberReferenceComparer()
        );

        public TypeInfo (ModuleInfo module, TypeDefinition type, TypeReference reference) {
            Definition = type;
            Reference = reference;

            Metadata = new MetadataCollection(type);

            foreach (var field in type.Fields)
                AddMember(field);

            foreach (var method in type.Methods)
                AddMember(method);

            foreach (var property in type.Properties) {
                if (property.GetMethod != null)
                    MethodToProperty[property.GetMethod] = property;

                if (property.SetMethod != null)
                    MethodToProperty[property.SetMethod] = property;

                AddMember(property);
            }

            foreach (var evt in type.Events) {
                if (evt.AddMethod != null)
                    MethodToEvent[evt.AddMethod] = evt;

                if (evt.RemoveMethod != null)
                    MethodToEvent[evt.RemoveMethod] = evt;

                AddMember(evt);
            }

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

                    MethodGroups.Add(info);

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

                IsFlagsEnum = Metadata.HasAttribute("System.FlagsAttribute");
            }

            IsIgnored = module.IsIgnored ||
                TypeInformation.IsIgnoredName(type.FullName) ||
                TypeInformation.IsIgnoredName(reference.FullName) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore");
        }

        protected void AddMember (MethodDefinition method) {
            AddMember<MethodDefinition>(method);

            var m = Regex.Match(method.Name, @"\<(?'scope'[^>]*)\>(?'mangling'[^_]*)__(?'index'[0-9]*)");
            if (m.Success)
                IgnoredMembers.Add(method);
        }

        protected void AddMember<T> (T definition)
            where T : MemberReference, ICustomAttributeProvider {

            var metadata = new MetadataCollection(definition);
            MemberMetadata[definition] = metadata;

            if (metadata.HasAttribute("JSIL.Meta.JSIgnore") ||
                TypeInformation.IsIgnoredName(definition.FullName))
                IgnoredMembers.Add(definition);
        }
    }

    public class MetadataCollection {
        public readonly Dictionary<string, HashSet<CustomAttribute>> CustomAttributes = new Dictionary<string, HashSet<CustomAttribute>>();

        public MetadataCollection (ICustomAttributeProvider target) {
            HashSet<CustomAttribute> attrs;

            foreach (var ca in target.CustomAttributes) {
                var key = ca.AttributeType.FullName;

                if (!CustomAttributes.TryGetValue(key, out attrs))
                    CustomAttributes[key] = attrs = new HashSet<CustomAttribute>();

                attrs.Add(ca);
            }
        }

        public bool HasAttribute (TypeReference attributeType) {
            return HasAttribute(attributeType.FullName);
        }

        public bool HasAttribute (Type attributeType) {
            return HasAttribute(attributeType.FullName);
        }

        public bool HasAttribute (string fullName) {
            var attrs = GetAttributes(fullName);

            return ((attrs != null) && (attrs.Count > 0));
        }

        public HashSet<CustomAttribute> GetAttributes (TypeReference attributeType) {
            return GetAttributes(attributeType.FullName);
        }

        public HashSet<CustomAttribute> GetAttributes (Type attributeType) {
            return GetAttributes(attributeType.FullName);
        }

        public HashSet<CustomAttribute> GetAttributes (string fullName) {
            HashSet<CustomAttribute> attrs;

            if (CustomAttributes.TryGetValue(fullName, out attrs))
                return attrs;

            return null;
        }

        public IList<CustomAttributeArgument> GetAttributeParameters (string fullName) {
            var attrs = GetAttributes(fullName);
            if ((attrs == null) || (attrs.Count == 0))
                return null;

            if (attrs.Count > 1)
                throw new NotImplementedException("There are multiple attributes of the type '" + fullName + "'.");

            return attrs.First().ConstructorArguments;
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
            MangledName = String.Format("{0}${1}", method.Name, index);
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
