using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JSIL.Meta;
using Mono.Cecil;

namespace JSIL.Internal {
    public interface ITypeInfoSource {
        ModuleInfo Get (ModuleDefinition module);
        TypeInfo Get (TypeReference type);
        IMemberInfo Get (MemberReference member);

        ProxyInfo[] GetProxies (TypeReference type);
    }

    public static class TypeInfoSourceExtensions {
        public static FieldInfo GetField (this ITypeInfoSource source, FieldReference field) {
            return (FieldInfo)source.Get(field);
        }

        public static MethodInfo GetMethod (this ITypeInfoSource source, MethodReference method) {
            return (MethodInfo)source.Get(method);
        }

        public static PropertyInfo GetProperty (this ITypeInfoSource source, PropertyReference property) {
            return (PropertyInfo)source.Get(property);
        }
    }

    internal struct MemberIdentifier {
        public string Name;
        public TypeReference ReturnType;
        public int ParameterCount;
        public IEnumerable<TypeReference> ParameterTypes;

        public MemberIdentifier (MemberReference mr) {
            Name = mr.FullName;
            ReturnType = null;
            ParameterCount = 0;
            ParameterTypes = null;
        }

        public MemberIdentifier (MethodReference mr) {
            Name = mr.Name;
            ReturnType = mr.ReturnType;
            ParameterCount = mr.Parameters.Count;
            ParameterTypes = (from p in mr.Parameters select p.ParameterType);
        }

        static bool TypesAreEqual (TypeReference lhs, TypeReference rhs) {
            if (lhs == null || rhs == null)
                return (lhs == rhs);

            if (lhs.IsGenericParameter || rhs.IsGenericParameter)
                return true;

            return ILBlockTranslator.TypesAreEqual(lhs, rhs);
        }

        public bool Equals (MemberIdentifier rhs) {
            if (!String.Equals(Name, rhs.Name))
                return false;

            if (!TypesAreEqual(ReturnType, rhs.ReturnType))
                return false;

            if ((ParameterTypes == null) || (rhs.ParameterTypes == null)) {
                if (ParameterTypes != rhs.ParameterTypes)
                    return false;
            } else {
                if (ParameterCount != rhs.ParameterCount)
                    return false;

                using (var eLeft = ParameterTypes.GetEnumerator())
                using (var eRight = rhs.ParameterTypes.GetEnumerator()) {
                    bool left, right;
                    while ((left = eLeft.MoveNext()) & (right = eRight.MoveNext())) {
                        if (!TypesAreEqual(eLeft.Current, eRight.Current))
                            return false;
                    }

                    if (left != right)
                        return false;
                }
            }

            return true;
        }

        public override bool Equals (object obj) {
            if (obj is MemberIdentifier)
                return Equals((MemberIdentifier)obj);

            return base.Equals(obj);
        }

        public override int GetHashCode () {
            return Name.GetHashCode() ^ ParameterCount.GetHashCode();
        }

        public override string ToString () {
            return String.Format(
                "{0} {1} ( {2} )", ReturnType, Name,
                String.Join(", ", (from p in ParameterTypes select p.ToString()).ToArray())
            );
        }
    }

    internal class MemberReferenceComparer : IEqualityComparer<MemberReference> {
        protected MemberIdentifier GetKey (MemberReference mr) {
            var method = mr as MethodReference;
            if (method != null)
                return new MemberIdentifier(method);
            else
                return new MemberIdentifier(mr);
        }

        public bool Equals (MemberReference lhs, MemberReference rhs) {
            var keyLeft = GetKey(lhs);
            var keyRight = GetKey(rhs);
            return keyLeft.Equals(keyRight);
        }

        public int GetHashCode (MemberReference obj) {
            return GetKey(obj).GetHashCode();
        }
    }

    public class ModuleInfo {
        public readonly bool IsIgnored;
        public readonly MetadataCollection Metadata;

        public ModuleInfo (ModuleDefinition module) {
            Metadata = new MetadataCollection(module);

            IsIgnored = TypeInfo.IsIgnoredName(module.FullyQualifiedName) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore");
        }
    }

    public class ProxyInfo {
        public readonly TypeDefinition Definition;
        public readonly TypeReference[] ProxiedTypes;

        public readonly MetadataCollection Metadata;

        public readonly JSProxyAttributePolicy AttributePolicy;
        public readonly JSProxyMemberPolicy MemberPolicy;

        public readonly Dictionary<string, FieldDefinition> Fields = new Dictionary<string, FieldDefinition>();
        public readonly Dictionary<string, PropertyDefinition> Properties = new Dictionary<string, PropertyDefinition>();
        public readonly Dictionary<string, EventDefinition> Events = new Dictionary<string, EventDefinition>();
        public readonly Dictionary<string, MethodDefinition> Methods = new Dictionary<string, MethodDefinition>();

        public ProxyInfo (TypeDefinition proxyType) {
            Definition = proxyType;
            Metadata = new MetadataCollection(proxyType);

            var args = Metadata.GetAttributeParameters("JSIL.Meta.JSProxy");

            var caa = args[0].Value as CustomAttributeArgument[];
            if (caa != null) {
                ProxiedTypes = new TypeReference[caa.Length];
                for (var i = 0; i < ProxiedTypes.Length; i++)
                    ProxiedTypes[i] = (TypeReference)caa[i].Value;
            } else {
                ProxiedTypes = new[] { (TypeReference)args[0].Value };
            }

            AttributePolicy = (JSProxyAttributePolicy)args[1].Value;
            MemberPolicy = (JSProxyMemberPolicy)args[2].Value;

            foreach (var field in proxyType.Fields) {
                if (!ILBlockTranslator.TypesAreEqual(field.DeclaringType, proxyType))
                    continue;

                Fields[field.Name] = field;
            }

            foreach (var property in proxyType.Properties) {
                if (!ILBlockTranslator.TypesAreEqual(property.DeclaringType, proxyType))
                    continue;

                Properties[property.Name] = property;
            }

            foreach (var evt in proxyType.Events) {
                if (!ILBlockTranslator.TypesAreEqual(evt.DeclaringType, proxyType))
                    continue;

                Events[evt.Name] = evt;
            }

            // TODO: Support overloaded methods
            foreach (var method in proxyType.Methods) {
                // TODO: No way to detect whether the constructor was compiler-generated.
                if ((method.Name == ".ctor") && (method.Parameters.Count == 0))
                    continue;

                if (!ILBlockTranslator.TypesAreEqual(method.DeclaringType, proxyType))
                    continue;

                Methods[method.Name] = method;
            }
        }

        public bool GetMember<T> (string key, out T result)
            where T : class {

            // TODO: Support overloaded methods
            MethodDefinition method;
            if (Methods.TryGetValue(key, out method) && ((result = method as T) != null))
                return true;

            FieldDefinition field;
            if (Fields.TryGetValue(key, out field) && ((result = field as T) != null))
                return true;

            PropertyDefinition property;
            if (Properties.TryGetValue(key, out property) && ((result = property as T) != null))
                return true;

            EventDefinition evt;
            if (Events.TryGetValue(key, out evt) && ((result = evt as T) != null))
                return true;

            result = null;
            return false;
        }
    }

    internal static class ProxyExtensions {
        public static FieldDefinition ResolveProxy (this ProxyInfo[] proxies, FieldDefinition field) {
            var key = field.Name;
            FieldDefinition temp;

            foreach (var proxy in proxies)
                if (proxy.Fields.TryGetValue(key, out temp))
                    field = temp;

            return field;
        }

        public static PropertyDefinition ResolveProxy (this ProxyInfo[] proxies, PropertyDefinition property) {
            var key = property.Name;
            PropertyDefinition temp;

            foreach (var proxy in proxies)
                if (
                    proxy.Properties.TryGetValue(key, out temp) && 
                    !(temp.GetMethod ?? temp.SetMethod).IsAbstract &&
                    !(temp.SetMethod ?? temp.GetMethod).IsAbstract
                )
                    property = temp;

            return property;
        }

        public static EventDefinition ResolveProxy (this ProxyInfo[] proxies, EventDefinition evt) {
            var key = evt.Name;
            EventDefinition temp;

            foreach (var proxy in proxies)
                if (
                    proxy.Events.TryGetValue(key, out temp) && 
                    !(temp.AddMethod ?? temp.RemoveMethod).IsAbstract &&
                    !(temp.RemoveMethod ?? temp.AddMethod).IsAbstract
                )
                    evt = temp;

            return evt;
        }

        public static MethodDefinition ResolveProxy (this ProxyInfo[] proxies, MethodDefinition method) {
            var key = method.Name;
            MethodDefinition temp;

            foreach (var proxy in proxies)
                if (proxy.Methods.TryGetValue(key, out temp) && (!temp.IsAbstract))
                    method = temp;

            return method;
        }
    }

    public class TypeInfo {
        public readonly TypeDefinition Definition;

        // Class information
        public readonly bool IsIgnored;
        // This needs to be mutable so we can introduce a constructed cctor later
        public MethodDefinition StaticConstructor;
        public readonly HashSet<MethodDefinition> Constructors = new HashSet<MethodDefinition>();
        public readonly MetadataCollection Metadata;
        public readonly ProxyInfo[] Proxies;

        public readonly HashSet<MethodGroupInfo> MethodGroups = new HashSet<MethodGroupInfo>();

        public readonly bool IsFlagsEnum;
        public readonly Dictionary<long, EnumMemberInfo> ValueToEnumMember = new Dictionary<long, EnumMemberInfo>();
        public readonly Dictionary<string, EnumMemberInfo> EnumMembers = new Dictionary<string, EnumMemberInfo>();

        public readonly Dictionary<MemberReference, IMemberInfo> Members = new Dictionary<MemberReference, IMemberInfo>(
            new MemberReferenceComparer()
        );

        public TypeInfo (ITypeInfoSource source, ModuleInfo module, TypeDefinition type) {
            Definition = type;

            Proxies = source.GetProxies(type);

            Metadata = new MetadataCollection(type);

            foreach (var proxy in Proxies)
                Metadata.Update(proxy.Metadata, proxy.AttributePolicy == JSProxyAttributePolicy.Replace);

            foreach (var field in type.Fields)
                AddMember(field);

            foreach (var property in type.Properties) {
                var pi = AddMember(property);

                if (property.GetMethod != null)
                    AddMember(property.GetMethod, pi);

                if (property.SetMethod != null)
                    AddMember(property.SetMethod, pi);
            }

            foreach (var evt in type.Events) {
                var ei = AddMember(evt);

                if (evt.AddMethod != null)
                    AddMember(evt.AddMethod, ei);

                if (evt.RemoveMethod != null)
                    AddMember(evt.RemoveMethod, ei);
            }

            foreach (var method in type.Methods) {
                if (method.Name == ".ctor")
                    Constructors.Add(method);

                if (!Members.ContainsKey(method))
                    AddMember(method);
            }

            var methodGroups = from m in type.Methods 
                          where !Members[m].IsIgnored
                          group m by new { 
                              m.Name, m.IsStatic
                          } into mg select mg;

            foreach (var mg in methodGroups) {
                var count = mg.Count();
                if (count > 1) {
                    int i = 0;

                    foreach (var item in mg) {
                        (Members[item] as MethodInfo).OverloadIndex = i;
                        i += 1;
                    }

                    MethodGroups.Add(new MethodGroupInfo(
                        this, (from m in mg select (Members[m] as MethodInfo)).ToArray(), mg.First().Name
                    ));
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
                IsIgnoredName(type.FullName) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore");
        }

        public static bool IsIgnoredName (string fullName) {
            var m = Regex.Match(fullName, @"\<(?'scope'[^>]*)\>(?'mangling'[^_]*)__(?'index'[0-9]*)");
            if (m.Success)
                return false; 

            if (fullName.EndsWith("__BackingField"))
                return false;
            else if (fullName.Contains("__DisplayClass"))
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

        protected void AddMember (MethodDefinition method, PropertyInfo property) {
            Members.Add(method, new MethodInfo(method, Proxies, property));
        }

        protected void AddMember (MethodDefinition method, EventInfo evt) {
            Members.Add(method, new MethodInfo(method, Proxies, evt));
        }

        protected void AddMember (MethodDefinition method) {
            Members.Add(method, new MethodInfo(method, Proxies));
        }

        protected void AddMember (FieldDefinition field) {
            Members.Add(field, new FieldInfo(field, Proxies));
        }

        protected PropertyInfo AddMember (PropertyDefinition property) {
            var result = new PropertyInfo(property, Proxies);
            Members.Add(property, result);
            return result;
        }

        protected EventInfo AddMember (EventDefinition evt) {
            var result = new EventInfo(evt, Proxies);
            Members.Add(evt, result);
            return result;
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

        public void Update (MetadataCollection rhs, bool replace) {
            if (replace)
                CustomAttributes.Clear();

            foreach (var kvp in rhs.CustomAttributes) {
                HashSet<CustomAttribute> setLhs;
                if (!CustomAttributes.TryGetValue(kvp.Key, out setLhs))
                    CustomAttributes[kvp.Key] = setLhs = new HashSet<CustomAttribute>();

                foreach (var ca in kvp.Value)
                    setLhs.Add(ca);
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

    public interface IMemberInfo {
        PropertyInfo DeclaringProperty {
            get;
        }
        EventInfo DeclaringEvent {
            get;
        }
        MetadataCollection Metadata {
            get;
        }
        string Name {
            get;
        }
        bool IsIgnored {
            get;
        }
    }

    public class MemberInfo<T> : IMemberInfo
        where T : MemberReference, ICustomAttributeProvider 
    {
        public readonly T Member;
        public readonly MetadataCollection Metadata;
        public readonly bool IsIgnored;
        public readonly string ForcedName;

        public MemberInfo (T member, ProxyInfo[] proxies, bool isIgnored = false) {
            IsIgnored = isIgnored || TypeInfo.IsIgnoredName(member.FullName);

            Member = member;
            Metadata = new MetadataCollection(member);

            if (Metadata.HasAttribute("JSIL.Meta.JSIgnore"))
                IsIgnored = true;

            foreach (var proxy in proxies) {
                T temp;
                if (proxy.GetMember<T>(member.Name, out temp)) {
                    var meta = new MetadataCollection(temp);
                    Metadata.Update(meta, proxy.AttributePolicy == JSProxyAttributePolicy.Replace);
                }
            }
        }

        protected virtual string GetName () {
            return Member.Name;
        }

        MetadataCollection IMemberInfo.Metadata {
            get { return Metadata; }
        }

        bool IMemberInfo.IsIgnored {
            get { return IsIgnored; }
        }

        public string Name {
            get {
                var parms = Metadata.GetAttributeParameters("JSIL.Meta.JSChangeName");
                if (parms != null)
                    return (string)parms[0].Value;

                return GetName();
            }
        }

        public TypeReference DeclaringType {
            get { return Member.DeclaringType; }
        }

        public virtual PropertyInfo DeclaringProperty {
            get { return null; }
        }
        public virtual EventInfo DeclaringEvent {
            get { return null; }
        }
    }

    public class FieldInfo : MemberInfo<FieldDefinition> {
        public FieldInfo (FieldDefinition field, ProxyInfo[] proxies) : base(
            proxies.ResolveProxy(field), proxies, field.FieldType.IsPointer
        ) {
        }

        public TypeReference Type {
            get {
                return Member.FieldType;
            }
        }
    }

    public class PropertyInfo : MemberInfo<PropertyDefinition> {
        public PropertyInfo (PropertyDefinition property, ProxyInfo[] proxies) : base(
            proxies.ResolveProxy(property), proxies, property.PropertyType.IsPointer
        ) {
        }

        protected override string GetName () {
            string result;
            var declType = Member.DeclaringType.Resolve();

            if ((declType != null) && declType.IsInterface)
                result = String.Format("{0}.{1}", declType.Name, Member.Name);
            else
                result = Member.Name;

            return result;
        }

        public TypeReference Type {
            get {
                return Member.PropertyType;
            }
        }
    }

    public class EventInfo : MemberInfo<EventDefinition> {
        public EventInfo (EventDefinition evt, ProxyInfo[] proxies) : base(
            proxies.ResolveProxy(evt), proxies, false
        ) {
        }
    }

    public class MethodInfo : MemberInfo<MethodDefinition> {
        public readonly PropertyInfo Property = null;
        public readonly EventInfo Event = null;

        public int? OverloadIndex;

        public MethodInfo (MethodDefinition method, ProxyInfo[] proxies) : base (
            proxies.ResolveProxy(method), proxies,
            (method.ReturnType.IsPointer) || (method.Parameters.Any((p) => p.ParameterType.IsPointer))
        ) {
        }

        public MethodInfo (MethodDefinition method, ProxyInfo[] proxies, PropertyInfo property) : base (
            proxies.ResolveProxy(method), proxies,
            method.ReturnType.IsPointer || 
                method.Parameters.Any((p) => p.ParameterType.IsPointer) || 
                property.IsIgnored
        ) {
            Property = property;
        }

        public MethodInfo (MethodDefinition method, ProxyInfo[] proxies, EventInfo evt) : base(
            proxies.ResolveProxy(method), proxies,
            method.ReturnType.IsPointer || 
                method.Parameters.Any((p) => p.ParameterType.IsPointer) ||
                evt.IsIgnored
        ) {
            Event = evt;
        }

        protected override string GetName () {
            string result;
            var declType = Member.DeclaringType.Resolve();
            var over = Member.Overrides.FirstOrDefault();

            if ((declType != null) && declType.IsInterface)
                result = String.Format("{0}.{1}", declType.Name, Member.Name);
            else if (over != null)
                result = String.Format("{0}.{1}", over.DeclaringType.Name, over.Name);
            else
                result = Member.Name;

            if ((OverloadIndex.HasValue) && !Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch")) {
                result = String.Format("{0}${1}", result, OverloadIndex.Value);
            }

            return result;
        }

        public override PropertyInfo DeclaringProperty {
            get { return Property; }
        }

        public override EventInfo DeclaringEvent {
            get { return Event; }
        }

        public TypeReference ReturnType {
            get { return Member.ReturnType; }
        }
    }

    public class MethodGroupInfo {
        public readonly TypeInfo DeclaringType;
        public readonly MethodInfo[] Methods;
        public readonly bool IsStatic;
        public readonly string Name;

        public MethodGroupInfo (TypeInfo declaringType, MethodInfo[] methods, string name) {
            DeclaringType = declaringType;
            Methods = methods;
            IsStatic = Methods.First().Member.IsStatic;
            Name = name;
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
