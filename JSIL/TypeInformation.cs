using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JSIL.Ast;
using JSIL.Meta;
using JSIL.Proxy;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL.Internal {
    public interface ITypeInfoSource {
        ModuleInfo Get (ModuleDefinition module);
        TypeInfo Get (TypeReference type);
        TypeInfo GetExisting (TypeReference type);
        TypeInfo GetExisting (TypeDefinition type);
        TypeInfo GetExisting (TypeIdentifier type);
        IMemberInfo Get (MemberReference member);

        ProxyInfo[] GetProxies (TypeDefinition type);

        void CacheProxyNames (MemberReference member);
        bool TryGetProxyNames (string typeFullName, out string[] result);

        ConcurrentCache<Tuple<string, string>, bool> AssignabilityCache {
            get;
        }
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

    public struct TypeIdentifier {
        public readonly string Assembly;
        public readonly string Namespace;
        public readonly string DeclaringTypeName;
        public readonly string Name;

        public TypeIdentifier (TypeDefinition type) {
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.Module != null) {
                var asm = type.Module.Assembly;
                if (asm != null)
                    Assembly = asm.FullName;
                else
                    Assembly = null;
            } else {
                Assembly = null;
            }

            Namespace = type.Namespace;
            Name = type.Name;

            var declaringType = type.DeclaringType;
            if (declaringType != null)
                DeclaringTypeName = declaringType.FullName;
            else
                DeclaringTypeName = null;
        }

        public bool Equals (TypeIdentifier rhs) {
            if (!String.Equals(Name, rhs.Name))
                return false;

            if (!String.Equals(Namespace, rhs.Namespace))
                return false;

            if (!String.Equals(DeclaringTypeName, rhs.DeclaringTypeName))
                return false;

            if ((Assembly == null) || (rhs.Assembly == null))
                return true;
            else if (!String.Equals(Assembly, rhs.Assembly))
                return false;
            else
                return true;
        }

        public override bool Equals (object obj) {
            if (obj is TypeIdentifier)
                return Equals((TypeIdentifier)obj);

            return base.Equals(obj);
        }

        public override int GetHashCode () {
            var result = Namespace.GetHashCode() ^ Name.GetHashCode();
            if (DeclaringTypeName != null)
                result ^= DeclaringTypeName.GetHashCode();

            return result;
        }

        public override string ToString () {
            return String.Format(
                "{0}{1}{2}{3}{4}{5}{6}", Assembly, String.IsNullOrWhiteSpace(Assembly) ? "" : " ",
                Namespace, String.IsNullOrWhiteSpace(Namespace) ? "" : ".",
                String.IsNullOrWhiteSpace(DeclaringTypeName) ? "" : "/",
                String.IsNullOrWhiteSpace(DeclaringTypeName) ? "" : DeclaringTypeName,
                Name
            );
        }
    }

    public struct GenericTypeIdentifier {
        public readonly TypeIdentifier Type;
        public readonly TypeIdentifier[] Arguments;
        public readonly int ArrayRank;

        public GenericTypeIdentifier (TypeDefinition type, TypeDefinition[] arguments, int arrayRank) {
            Type = new TypeIdentifier(type);
            Arguments = (from a in arguments select new TypeIdentifier(a)).ToArray();
            ArrayRank = arrayRank;
        }

        public bool Equals (GenericTypeIdentifier rhs) {
            if (!Type.Equals(rhs.Type))
                return false;

            if (Arguments.Length != rhs.Arguments.Length)
                return false;

            if (ArrayRank != rhs.ArrayRank)
                return false;

            for (var i = 0; i < Arguments.Length; i++) {
                if (!Arguments[i].Equals(rhs.Arguments[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals (object obj) {
            if (obj is GenericTypeIdentifier)
                return Equals((GenericTypeIdentifier)obj);
            else
                return false;
        }

        public override int GetHashCode () {
            return Type.GetHashCode() ^ Arguments.Length ^ ArrayRank;
        }

        private static string GetRankSuffix (int rank) {
            if (rank <= 0)
                return "";
            else {
                var result = "[";
                for (var i = 1; i < rank; i++)
                    result += ",";
                result += "]";
                return result;
            }
        }

        public override string ToString () {
            return String.Format(
                "{0}<{1}>",
                (Type + GetRankSuffix(ArrayRank)),
                String.Join<TypeIdentifier>(", ", Arguments)
            );
        }
    }

    public class ModuleInfo {
        public readonly bool IsIgnored;
        public readonly MetadataCollection Metadata;

        public ModuleInfo (ModuleDefinition module) {
            Metadata = new MetadataCollection(module);

            IsIgnored = TypeInfo.IsIgnoredName(module.FullyQualifiedName, false) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore");
        }
    }

    public class ProxyInfo {
        public readonly string Name;

        public readonly TypeDefinition Definition;
        public readonly HashSet<TypeReference> ProxiedTypes = new HashSet<TypeReference>();
        public readonly HashSet<string> ProxiedTypeNames = new HashSet<string>();

        public readonly TypeReference[] Interfaces;
        public readonly MetadataCollection Metadata;

        public readonly JSProxyAttributePolicy AttributePolicy;
        public readonly JSProxyMemberPolicy MemberPolicy;
        public readonly JSProxyInterfacePolicy InterfacePolicy;

        public readonly Dictionary<MemberIdentifier, FieldDefinition> Fields;
        public readonly Dictionary<MemberIdentifier, PropertyDefinition> Properties;
        public readonly Dictionary<MemberIdentifier, EventDefinition> Events;
        public readonly Dictionary<MemberIdentifier, MethodDefinition> Methods;

        public readonly MethodDefinition ExtraStaticConstructor;

        public readonly bool IsInheritable;

        protected readonly ITypeInfoSource TypeInfo;

        public ProxyInfo (ITypeInfoSource typeInfo, TypeDefinition proxyType) {
            TypeInfo = typeInfo;
            var comparer = new MemberIdentifier.Comparer(typeInfo);

            Fields = new Dictionary<MemberIdentifier, FieldDefinition>(comparer);
            Properties = new Dictionary<MemberIdentifier, PropertyDefinition>(comparer);
            Events = new Dictionary<MemberIdentifier, EventDefinition>(comparer);
            Methods = new Dictionary<MemberIdentifier, MethodDefinition>(comparer);

            ExtraStaticConstructor = null;

            Definition = proxyType;
            Name = Definition.Name;
            Metadata = new MetadataCollection(proxyType);
            Interfaces = proxyType.Interfaces.ToArray();
            IsInheritable = true;

            var args = Metadata.GetAttributeParameters("JSIL.Proxy.JSProxy");
            if (args == null)
                throw new ArgumentNullException("JSProxy without arguments");

            // Attribute parameter ordering is random. Awesome!
            foreach (var arg in args) {
                switch (arg.Type.FullName) {
                    case "JSIL.Proxy.JSProxyAttributePolicy":
                        AttributePolicy = (JSProxyAttributePolicy)arg.Value;
                        break;
                    case "JSIL.Proxy.JSProxyMemberPolicy":
                        MemberPolicy = (JSProxyMemberPolicy)arg.Value;
                        break;
                    case "JSIL.Proxy.JSProxyInterfacePolicy":
                        InterfacePolicy = (JSProxyInterfacePolicy)arg.Value;
                        break;
                    case "System.Type":
                        ProxiedTypes.Add((TypeReference)arg.Value);
                        break;
                    case "System.Type[]": {
                        var values = (CustomAttributeArgument[])arg.Value;
                        for (var i = 0; i < values.Length; i++)
                            ProxiedTypes.Add((TypeReference)values[i].Value);
                        break;
                    }
                    case "System.Boolean":
                        IsInheritable = (bool)arg.Value;
                        break;
                    case "System.String":
                        ProxiedTypeNames.Add((string)arg.Value);
                        break;
                    case "System.String[]": {
                        var values = (CustomAttributeArgument[])arg.Value;
                        foreach (var v in values)
                            ProxiedTypeNames.Add((string)v.Value);
                        break;
                    }
                    default:
                        throw new NotImplementedException(String.Format(
                            "Invalid argument to JSProxy attribute: {0}",
                            arg.Type.FullName
                        ));
                }
            }

            foreach (var field in proxyType.Fields) {
                if (!TypeUtil.TypesAreEqual(field.DeclaringType, proxyType))
                    continue;

                Fields.Add(new MemberIdentifier(typeInfo, field), field);
            }

            foreach (var property in proxyType.Properties) {
                if (!TypeUtil.TypesAreEqual(property.DeclaringType, proxyType))
                    continue;

                Properties.Add(new MemberIdentifier(typeInfo, property), property);
            }

            foreach (var evt in proxyType.Events) {
                if (!TypeUtil.TypesAreEqual(evt.DeclaringType, proxyType))
                    continue;

                Events.Add(new MemberIdentifier(typeInfo, evt), evt);
            }

            foreach (var method in proxyType.Methods) {
                if (!TypeUtil.TypesAreEqual(method.DeclaringType, proxyType))
                    continue;

                if ((method.Name == ".cctor") && method.CustomAttributes.Any((ca) => ca.AttributeType.FullName == "JSIL.Meta.JSExtraStaticConstructor")) {
                    ExtraStaticConstructor = method;
                } else {
                    Methods.Add(new MemberIdentifier(typeInfo, method), method);
                }
            }
        }

        public override string ToString () {
            return Definition.FullName;
        }

        public bool GetMember<T> (MemberIdentifier member, out T result)
            where T : class {

            MethodDefinition method;
            if (Methods.TryGetValue(member, out method) && ((result = method as T) != null))
                return true;

            FieldDefinition field;
            if (Fields.TryGetValue(member, out field) && ((result = field as T) != null))
                return true;

            PropertyDefinition property;
            if (Properties.TryGetValue(member, out property) && ((result = property as T) != null))
                return true;

            EventDefinition evt;
            if (Events.TryGetValue(member, out evt) && ((result = evt as T) != null))
                return true;

            result = null;
            return false;
        }

        public bool IsMatch (TypeDefinition type, bool? forcedInheritable) {
            bool inheritable = forcedInheritable.GetValueOrDefault(IsInheritable);

            foreach (var pt in ProxiedTypes) {
                bool isMatch;
                if (inheritable)
                    isMatch = TypeUtil.TypesAreAssignable(TypeInfo, pt, type);
                else
                    isMatch = TypeUtil.TypesAreEqual(pt, type);

                if (isMatch)
                    return true;
            }

            if (ProxiedTypeNames.Count > 0) {
                if (ProxiedTypeNames.Contains(type.FullName))
                    return true;

                if (inheritable)
                    foreach (var baseType in TypeUtil.AllBaseTypesOf(TypeUtil.GetTypeDefinition(type))) {
                        if (ProxiedTypeNames.Contains(baseType.FullName))
                            return true;
                    }
            }

            return false;
        }
    }

    public class TypeInfo {
        public readonly TypeIdentifier Identifier;
        public readonly TypeDefinition Definition;
        public readonly ITypeInfoSource Source;

        public readonly TypeInfo DeclaringType;
        public readonly TypeInfo BaseClass;

        public readonly System.Tuple<TypeInfo, TypeReference>[] Interfaces;
        private System.Tuple<TypeInfo, TypeInfo, TypeReference>[] _AllInterfacesRecursive = null;

        // This needs to be mutable so we can introduce a constructed cctor later
        public MethodDefinition StaticConstructor;

        public readonly List<MethodInfo> ExtraStaticConstructors = new List<MethodInfo>();

        public readonly HashSet<MethodDefinition> Constructors = new HashSet<MethodDefinition>();
        public readonly MetadataCollection Metadata;
        public readonly ProxyInfo[] Proxies;

        public readonly MethodSignatureCollection MethodSignatures;
        public readonly HashSet<MethodGroupInfo> MethodGroups = new HashSet<MethodGroupInfo>();

        public readonly bool IsFlagsEnum;
        public readonly EnumMemberInfo FirstEnumMember = null;
        public readonly ConcurrentDictionary<long, EnumMemberInfo> ValueToEnumMember;
        public readonly ConcurrentDictionary<string, EnumMemberInfo> EnumMembers;
        public readonly ConcurrentDictionary<MemberIdentifier, IMemberInfo> Members;
        public readonly List<FieldInfo> AddedFieldsFromProxies = new List<FieldInfo>();
        public readonly bool IsProxy;
        public readonly bool IsDelegate;
        public readonly bool IsInterface;
        public readonly bool IsImmutable;
        public readonly string Replacement;

        // Matches JSIL runtime name escaping rules
        public readonly string LocalName;

        protected int _DerivedTypeCount = 0;
        protected string _FullName = null;
        protected bool _FullyInitialized = false;
        protected bool _IsIgnored = false;
        protected bool _IsExternal = false;
        protected bool _MethodGroupsInitialized = false;

        protected List<NamedMethodSignature> DeferredMethodSignatureSetUpdates = new List<NamedMethodSignature>();

        public TypeInfo (ITypeInfoSource source, ModuleInfo module, TypeDefinition type, TypeInfo declaringType, TypeInfo baseClass, TypeIdentifier identifier) {
            Identifier = identifier;
            DeclaringType = declaringType;
            BaseClass = baseClass;
            Source = source;
            Definition = type;
            bool isStatic = type.IsSealed && type.IsAbstract;

            LocalName = TypeUtil.GetLocalName(type);

            if (baseClass != null)
                Interlocked.Increment(ref baseClass._DerivedTypeCount);

            Proxies = source.GetProxies(type);
            Metadata = new MetadataCollection(type);
            MethodSignatures = new MethodSignatureCollection();

            // Do this check before copying attributes from proxy types, since that will copy their JSProxy attribute
            IsProxy = Metadata.HasAttribute("JSIL.Proxy.JSProxy");

            IsDelegate = (type.BaseType != null) && (
                (type.BaseType.FullName == "System.Delegate") ||
                (type.BaseType.FullName == "System.MulticastDelegate")
            );

            IsInterface = type.IsInterface;

            var interfaces = new HashSet<Tuple<TypeInfo, TypeReference>>();
            {
                StringBuilder errorString = null;

                foreach (var i in type.Interfaces) {
                    var resolved = i.Resolve();
                    if (resolved == null) {
                        Console.Error.WriteLine("Warning: Could not resolve interface reference '{0}' for type '{1}'!", i.FullName, type.FullName);
                        continue;
                    }

                    var ii = Tuple.Create(source.GetExisting(i), i);
                    if (ii.Item1 == null) {
                        if (errorString == null) {
                            errorString = new StringBuilder();
                            errorString.AppendFormat(
                                "Missing type information for the following interface(s) of type '{0}':{1}",
                                type.FullName, Environment.NewLine
                            );
                        }

                        errorString.AppendLine(i.FullName);
                    } else {
                        interfaces.Add(ii);
                    }
                }

                if (errorString != null)
                    throw new InvalidDataException(errorString.ToString());
            }

            foreach (var proxy in Proxies) {
                Metadata.Update(proxy.Metadata, proxy.AttributePolicy == JSProxyAttributePolicy.ReplaceAll);

                if (proxy.InterfacePolicy == JSProxyInterfacePolicy.ReplaceNone) {
                } else {
                    if (proxy.InterfacePolicy == JSProxyInterfacePolicy.ReplaceAll)
                        interfaces.Clear();

                    foreach (var i in proxy.Interfaces) {
                        var ii = source.Get(i);
                        interfaces.Add(Tuple.Create(ii, i));
                    }
                }
            }

            if (Metadata.HasAttribute("JSIL.Proxy.JSProxy") && !IsProxy)
                Metadata.Remove("JSIL.Proxy.JSProxy");

            Interfaces = interfaces.ToArray();

            _IsIgnored = module.IsIgnored ||
                IsIgnoredName(type.Namespace, false) || 
                IsIgnoredName(type.Name, false) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore") ||
                Metadata.HasAttribute("System.Runtime.CompilerServices.UnsafeValueTypeAttribute") ||
                Metadata.HasAttribute("System.Runtime.CompilerServices.NativeCppClassAttribute");

            _IsExternal = Metadata.HasAttribute("JSIL.Meta.JSExternal");

            if (Metadata.HasAttribute("JSIL.Meta.JSReplacement")) {
                Replacement = (string)Metadata.GetAttributeParameters("JSIL.Meta.JSReplacement")[0].Value;
            } else {
                Replacement = null;
            }

            if (baseClass != null)
                _IsIgnored |= baseClass.IsIgnored;

            {
                var capacity = type.Fields.Count + type.Properties.Count + type.Events.Count + type.Methods.Count;
                var comparer = new MemberIdentifier.Comparer(source);
                Members = new ConcurrentDictionary<MemberIdentifier, IMemberInfo>(1, capacity, comparer);
            }

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

                AddMember(method);
            }

            if (type.IsEnum) {
                long enumValue = 0;

                var capacity = type.Fields.Count;
                ValueToEnumMember = new ConcurrentDictionary<long, EnumMemberInfo>(1, capacity);
                EnumMembers = new ConcurrentDictionary<string, EnumMemberInfo>(1, capacity);

                foreach (var field in type.Fields) {
                    // Skip 'value__'
                    if (field.IsRuntimeSpecialName)
                        continue;

                    if (field.HasConstant)
                        enumValue = Convert.ToInt64(field.Constant);

                    var info = new EnumMemberInfo(type, field.Name, enumValue);

                    if (FirstEnumMember == null)
                        FirstEnumMember = info;

                    ValueToEnumMember[enumValue] = info;
                    EnumMembers[field.Name] = info;

                    enumValue += 1;
                }

                IsFlagsEnum = Metadata.HasAttribute("System.FlagsAttribute");
            }

            foreach (var proxy in Proxies) {
                var seenMethods = new HashSet<MethodDefinition>();

                foreach (var property in proxy.Properties.Values) {
                    var p = (PropertyInfo)AddProxyMember(proxy, property);

                    if (property.GetMethod != null) {
                        if (!property.CustomAttributes.Any(ShouldNeverReplace))
                            AddProxyMember(proxy, property.GetMethod, p);

                        seenMethods.Add(property.GetMethod);
                    }

                    if (property.SetMethod != null) {
                        if (!property.CustomAttributes.Any(ShouldNeverReplace))
                            AddProxyMember(proxy, property.SetMethod, p);

                        seenMethods.Add(property.SetMethod);
                    }
                }

                foreach (var evt in proxy.Events.Values) {
                    var e = (EventInfo)AddProxyMember(proxy, evt);

                    if (evt.AddMethod != null) {
                        if (!evt.CustomAttributes.Any(ShouldNeverReplace))
                            AddProxyMember(proxy, evt.AddMethod, e);

                        seenMethods.Add(evt.AddMethod);
                    }

                    if (evt.RemoveMethod != null) {
                        if (!evt.CustomAttributes.Any(ShouldNeverReplace))
                            AddProxyMember(proxy, evt.RemoveMethod, e);

                        seenMethods.Add(evt.RemoveMethod);
                    }
                }

                foreach (var field in proxy.Fields.Values) {
                    if (isStatic && !field.IsStatic)
                        continue;

                    AddProxyMember(proxy, field);
                }

                foreach (var method in proxy.Methods.Values) {
                    if (seenMethods.Contains(method))
                        continue;

                    if (isStatic && !method.IsStatic)
                        continue;

                    // The constructor may be compiler-generated, so only replace if it has the attribute.
                    if ((method.Name == ".ctor") && (method.Parameters.Count == 0)) {
                        if (!method.CustomAttributes.Any((ca) => ca.AttributeType.FullName == "JSIL.Meta.JSReplaceConstructor"))
                            continue;
                    }

                    AddProxyMember(proxy, method);
                }

                if (proxy.ExtraStaticConstructor != null) {
                    var name = String.Format(".cctor{0}", ExtraStaticConstructors.Count + 2);
                    var escIdentifier = new MemberIdentifier(source, proxy.ExtraStaticConstructor, name);
                    var escInfo = new MethodInfo(this, escIdentifier, proxy.ExtraStaticConstructor, Proxies, proxy);
                    escInfo.ForcedNewName = name;
                    ExtraStaticConstructors.Add(escInfo);
                }

                if (proxy.MemberPolicy == JSProxyMemberPolicy.ReplaceAll) {
                    var previousMembers = Members.ToArray();
                    Members.Clear();
                    foreach (var member in previousMembers) {
                        if (member.Value.IsFromProxy)
                            Members.TryAdd(member.Key, member.Value);
                    }
                }
            }

            if (
                !IsInterface &&
                !IsDelegate &&
                !Definition.IsEnum && 
                !Definition.IsAbstract && 
                !Definition.IsPrimitive
            ) {
                IsImmutable = Metadata.HasAttribute("JSIL.Meta.JSImmutable") ||
                    Members.Values.OfType<FieldInfo>().All((f) => f.IsStatic || f.IsImmutable);
            }

            DoDeferredMethodSignatureSetUpdate();
        }

        private void DoDeferredMethodSignatureSetUpdate () {
            var selfAndBaseTypesRecursive = this.SelfAndBaseTypesRecursive.ToArray();

            foreach (var t in selfAndBaseTypesRecursive) {
                var ms = t.MethodSignatures;

                foreach (var nms in DeferredMethodSignatureSetUpdates) {
                    var set = ms.GetOrCreateFor(nms.Name);
                    set.Add(nms);
                }
            }

            DeferredMethodSignatureSetUpdates.Clear();
            DeferredMethodSignatureSetUpdates = null;
        }

        public bool IsFullyInitialized {
            get {
                return _FullyInitialized;
            }
        }

        public string ChangedName {
            get {
                var parms = Metadata.GetAttributeParameters("JSIL.Meta.JSChangeName");
                if (parms != null)
                    return (string)parms[0].Value;

                return null;
            }
        }

        public IEnumerable<TypeInfo> SelfAndBaseTypesRecursive {
            get {
                yield return this;

                var baseType = BaseClass;
                while (baseType != null) {
                    yield return baseType;

                    baseType = baseType.BaseClass;
                }
            }
        }

        public int DerivedTypeCount {
            get {
                return _DerivedTypeCount;
            }
        }

        public override string ToString () {
            return Definition.FullName;
        }

        public System.Tuple<TypeInfo, TypeInfo, TypeReference>[] AllInterfacesRecursive {
            get {
                if (_AllInterfacesRecursive == null) {
                    var list = new List<System.Tuple<TypeInfo, TypeInfo, TypeReference>>();
                    var types = SelfAndBaseTypesRecursive.Reverse().ToArray();

                    foreach (var type in types)
                        foreach (var @interface in type.Interfaces)
                            list.Add(Tuple.Create(type, @interface.Item1, @interface.Item2));

                    _AllInterfacesRecursive = list.ToArray();
                }

                return _AllInterfacesRecursive;
            }
        }

        internal void ConstructMethodGroups () {
            if (_MethodGroupsInitialized)
                return;

            _MethodGroupsInitialized = true;

            var methodGroups = (from kvp in Members where kvp.Key.Type == MemberIdentifier.MemberType.Method
                                let m = (MethodInfo)kvp.Value
                                group m by new {
                                    m.Member.Name,
                                    m.Member.GenericParameters.Count,
                                    m.IsStatic
                                } into mg
                                where mg.Count() > 1
                                select mg).ToArray();

            foreach (var mg in methodGroups) {
                var filtered = (from m in mg where !m.IsIgnored && 
                                    !m.Metadata.HasAttribute("JSIL.Meta.JSReplacement") && 
                                    !m.Metadata.HasAttribute("JSIL.Meta.JSChangeName")
                                select m).ToArray();
                if (filtered.Length <= 1)
                    continue;

                var groupName = filtered.First().Name;
                var mgi = new MethodGroupInfo(
                    this, filtered.ToArray(), groupName
                );

                foreach (var m in mg)
                    m.MethodGroup = mgi;

                MethodGroups.Add(mgi);
            }
        }

        public bool IsIgnored {
            get {
                if (_FullyInitialized)
                    return _IsIgnored;

                if (Definition.DeclaringType != null) {
                    var dt = Source.GetExisting(Definition.DeclaringType);
                    if ((dt != null) && dt.IsIgnored)
                        return true;
                }

                return _IsIgnored;
            }
        }

        public bool IsExternal {
            get {
                if (_FullyInitialized)
                    return _IsExternal;

                if (Definition.DeclaringType != null) {
                    var dt = Source.GetExisting(Definition.DeclaringType);
                    if ((dt != null) && dt.IsExternal)
                        return true;
                }

                return _IsExternal;
            }
        }

        protected static bool ShouldNeverReplace (CustomAttribute ca) {
            return ca.AttributeType.FullName == "JSIL.Proxy.JSNeverReplace";
        }

        protected static bool ShouldNeverInherit (CustomAttribute ca) {
            return ca.AttributeType.FullName == "JSIL.Proxy.JSNeverInherit";
        }

        protected bool BeforeAddProxyMember<T> (ProxyInfo proxy, T member, out IMemberInfo result, ICustomAttributeProvider owningMember = null)
            where T : MemberReference, ICustomAttributeProvider
        {
            var identifier = MemberIdentifier.New(this.Source, member);

            if (member.CustomAttributes.Any(ShouldNeverInherit)) {
                if (!proxy.IsMatch(this.Definition, false)) {
                    result = null;
                    return false;
                }
            }

            while (Members.TryGetValue(identifier, out result)) {
                if (
                    (proxy.MemberPolicy == JSProxyMemberPolicy.ReplaceNone) ||
                    member.CustomAttributes.Any(ShouldNeverReplace) ||
                    ((owningMember != null) && (owningMember.CustomAttributes.Any(ShouldNeverReplace)))
                ) {
                    return true;
                } else if (
                           proxy.MemberPolicy == JSProxyMemberPolicy.ReplaceDeclared ||
                           proxy.MemberPolicy == JSProxyMemberPolicy.ReplaceAll) {
                    if (result.IsFromProxy)
                        Debug.WriteLine(String.Format("Warning: Proxy member '{0}' replacing proxy member '{1}'.", member, result));

                    Members.TryRemove(identifier, out result);
                } else {
                    throw new ArgumentException(String.Format(
                        "Member '{0}' not found", member.Name
                    ), "member");
                }
            }

            result = null;
            return false;
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result))
                return result;

            return AddMember(method, proxy);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method, PropertyInfo owningProperty) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result, owningProperty.Member))
                return result;

            return AddMember(method, owningProperty, proxy);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method, EventInfo owningEvent) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result, owningEvent.Member))
                return result;

            return AddMember(method, owningEvent, proxy);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, FieldDefinition field) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, field, out result))
                return result;

            var fi = AddMember(field, proxy);

            AddedFieldsFromProxies.Add(fi);
            return fi;
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, PropertyDefinition property) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, property, out result))
                return result;

            return AddMember(property, proxy);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, EventDefinition evt) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, evt, out result))
                return result;

            return AddMember(evt, proxy);
        }

        /*
            Test strings:
            <<$$>>__0
            <>2__current
            <baseType>5__1
            <o>__2
            <B>k__BackingField
            <$>things
         */

        static readonly Regex MangledNameRegex = new Regex(
            "(" +
                @"\<(\<?)(?'class'[^>]*)(\>?)\>(?'id'[a-zA-Z0-9]*)(_+)(?'name'[a-zA-Z0-9_]*)|" +
                @"\<(?'class'[^>]*)\>(?'id'[^_]*)(_+)(?'name'[a-zA-Z0-9_]*)|" +
                @"\<(\<?)(?'class'[^>]*)(\>?)\>(?'name'[a-zA-Z0-9_]*)" +
            ")", 
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        static readonly Regex IgnoredKeywordRegex = new Regex(
            @"__BackingField|CS\$\<|__DisplayClass|\<PrivateImplementationDetails\>|" +
            @"Runtime\.CompilerServices\.CallSite|\<Module\>|__SiteContainer|" +
            @"__DynamicSite|__CachedAnonymousMethodDelegate", 
            RegexOptions.Compiled
        );

        private static string GetGroup (Match m, string groupName, string defaultValue = null) {
            if (m.Groups[groupName].Success)
                return m.Groups[groupName].Value;
            else
                return defaultValue;
        }

        public static string GetOriginalName (string memberName, out bool isBackingField) {
            isBackingField = false;

            var m = MangledNameRegex.Match(memberName);
            if (!m.Success)
                return null;

            var @class = GetGroup(m, "class", "");
            var name = GetGroup(m, "name", "");

            isBackingField = name.Trim() == "BackingField";

            if (isBackingField) {
                return String.Format("{0}$value", @class);
            } else {
                int temp;
                string result;

                if (int.TryParse(name, out temp))
                    result = @class;
                else if (String.IsNullOrWhiteSpace(@class))
                    result = "$" + name;
                else
                    result = @class + "$" + name;

                // <<$$>>__1
                if ((result == "$") || (result == "$$"))
                    result += name;

                if (String.IsNullOrWhiteSpace(result))
                    return null;
                else 
                    return result;
            }
        }

        public string Name {
            get {
                return ChangedName ?? Definition.Name;
            }
        }

        public string FullName {
            get {
                if (_FullName != null)
                    return _FullName;

                if (DeclaringType != null)
                    return _FullName = DeclaringType.FullName + "/" + Name;

                if (string.IsNullOrEmpty(Definition.Namespace))
                    return _FullName = Name;

                return _FullName = Definition.Namespace + "." + Name;
            }
        }

        public static bool IsIgnoredName (string shortName, bool isField) {
            bool defaultResult = false;

            foreach (Match m2 in IgnoredKeywordRegex.Matches(shortName)) {
                if (m2.Success) {
                    var length = m2.Length;
                    var index = m2.Index;
                    if (
                        (length >= 2) &&
                        (shortName[index] == '_') && 
                        (shortName[index + 1] == '_')
                    ) {
                        switch (m2.Value) {
                            case "__BackingField":
                            case "__DisplayClass":
                                return false;

                            case "__DynamicSite":
                            case "__SiteContainer":
                                return true;

                            case "__CachedAnonymousMethodDelegate":
                                if (isField)
                                    return true;
                                break;
                        }
                    } else if (
                        (length >= 4) &&
                        (shortName[index] == 'C') && 
                        (shortName[index + 1] == 'S') &&
                        (shortName[index + 2] == '$') &&
                        (shortName[index + 3] == '<')
                    ) {
                        if (!isField)
                            return true;
                    } else {
                        defaultResult = true;
                    }
                }
            }

            var m = MangledNameRegex.Match(shortName);
            if (m.Success) {
                switch (shortName[m.Groups[2].Index]) {
                    case 'b':
                        // Lambda
                        return true;
                    case 'c':
                        // Class
                        return false;
                    case 'd':
                        // Enumerator
                        return false;
                }
            }

            return defaultResult;
        }

        protected MethodInfo AddMember (MethodDefinition method, PropertyInfo property, ProxyInfo sourceProxy = null) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, property, sourceProxy);
            if (property.Member.GetMethod == method)
                property.Getter = (MethodInfo)result;
            else if (property.Member.SetMethod == method)
                property.Setter = (MethodInfo)result;

            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            DeferredMethodSignatureSetUpdates.Add(((MethodInfo)result).NamedSignature);

            return (MethodInfo)result;
        }

        protected MethodInfo AddMember (MethodDefinition method, EventInfo evt, ProxyInfo sourceProxy = null) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, evt, sourceProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            DeferredMethodSignatureSetUpdates.Add(((MethodInfo)result).NamedSignature);

            return (MethodInfo)result;
        }

        protected MethodInfo AddMember (MethodDefinition method, ProxyInfo sourceProxy = null) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, sourceProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            if (method.Name == ".cctor")
                StaticConstructor = method;

            DeferredMethodSignatureSetUpdates.Add(((MethodInfo)result).NamedSignature);

            return (MethodInfo)result;
        }

        protected FieldInfo AddMember (FieldDefinition field, ProxyInfo sourceProxy = null) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, field);
            if (Members.TryGetValue(identifier, out result))
                return (FieldInfo)result;

            result = new FieldInfo(this, identifier, field, Proxies, sourceProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            return (FieldInfo)result;
        }

        protected PropertyInfo AddMember (PropertyDefinition property, ProxyInfo sourceProxy = null) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, property);
            if (Members.TryGetValue(identifier, out result)) {
                if (sourceProxy == null)
                    return (PropertyInfo)result;
                else
                    Members.TryRemove(identifier, out result);
            }

            result = new PropertyInfo(this, identifier, property, Proxies, sourceProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            return (PropertyInfo)result;
        }

        protected EventInfo AddMember (EventDefinition evt, ProxyInfo sourceProxy = null) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, evt);
            if (Members.TryGetValue(identifier, out result))
                return (EventInfo)result;

            result = new EventInfo(this, identifier, evt, Proxies, sourceProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            return (EventInfo)result;
        }

        internal bool IsTypeIgnored (ParameterDefinition pd, out TypeInfo typeInfo) {
            var pt = pd.ParameterType;
            pt = TypeUtil.DereferenceType(pt, false);

            return IsTypeIgnored(pt, out typeInfo);
        }

        internal bool IsTypeIgnored (TypeReference t, out TypeInfo typeInfo) {
            typeInfo = null;
            if (TypeUtil.IsIgnoredType(t))
                return true;

            if (t.IsPrimitive)
                return false;
            else if ((t.Name == "Void") && (t.Namespace == "System"))
                return false;
            else if (t is ArrayType)
                return false;
            else if (t is GenericParameter)
                return false;

            typeInfo = Source.GetExisting(t);
            if ((typeInfo != null) && typeInfo.IsIgnored)
                return true;

            return false;
        }

        internal void Initialize () {
            if (_FullyInitialized)
                return;

            _IsIgnored = IsIgnored;
            _FullyInitialized = true;
        }
    }

    public class AttributeGroup {
        public struct Entry {
            public readonly TypeReference Type;
            public readonly IList<CustomAttributeArgument> Arguments;

            public Entry (CustomAttribute ca) {
                Type = ca.AttributeType;
                Arguments = ca.ConstructorArguments;
            }
        }

        public bool Inherited;
        public string Name;
        public readonly List<Entry> Entries = new List<Entry>();
    }

    public class MetadataCollection : IEnumerable<KeyValuePair<string, AttributeGroup>> {
        protected Dictionary<string, AttributeGroup> Attributes = null;

        public MetadataCollection (ICustomAttributeProvider target) {
            var cas = target.CustomAttributes;

            if (cas.Count == 0)
                return;

            foreach (var ca in cas) {
                AttributeGroup existing;
                if (TryGetValue(ca.AttributeType.FullName, out existing))
                    existing.Entries.Add(new AttributeGroup.Entry(ca));
                else
                    Add(ca.AttributeType.FullName, new AttributeGroup {
                        Entries = { new AttributeGroup.Entry(ca) },
                        Inherited = false
                    });
            }
        }

        public void Add (string name, AttributeGroup entry) {
            if (Attributes == null)
                Attributes = new Dictionary<string, AttributeGroup>();

            Attributes.Add(name, entry);
        }

        public bool TryGetValue (string name, out AttributeGroup entry) {
            if (Attributes == null) {
                entry = null;
                return false;
            }

            return Attributes.TryGetValue(name, out entry);
        }

        public bool Remove (string key) {
            if (Attributes != null)
                return Attributes.Remove(key);

            return false;
        }

        public void Clear () {
            if (Attributes != null)
                Attributes.Clear();
        }

        public void Update (MetadataCollection rhs, bool replaceAll) {
            if (replaceAll)
                Clear();

            AttributeGroup existing;
            if (rhs.Attributes != null) {
                foreach (var kvp in rhs.Attributes) {
                    if (TryGetValue(kvp.Key, out existing)) {
                        if (existing.Inherited)
                            Remove(kvp.Key);
                        else
                            continue;
                    }

                    var inherited = new AttributeGroup {
                        Inherited = true,
                    };
                    inherited.Entries.AddRange(kvp.Value.Entries);
                    Add(kvp.Key, inherited);
                }
            }
        }

        public bool HasOwnAttribute (string fullName) {
            var ae = GetAttribute(fullName);
            return (ae != null) && (!ae.Inherited);
        }

        public bool HasAttribute (string fullName) {
            if (Attributes != null)
                return Attributes.ContainsKey(fullName);

            return false;
        }

        public AttributeGroup GetAttribute (string fullName) {
            AttributeGroup entry;

            if (TryGetValue(fullName, out entry))
                return entry;

            return null;
        }

        public IList<CustomAttributeArgument> GetAttributeParameters (string fullName) {
            var attr = GetAttribute(fullName);
            if (attr == null)
                return null;

            return attr.Entries[0].Arguments;
        }

        public IEnumerator<KeyValuePair<string, AttributeGroup>> GetEnumerator () {
            if (Attributes == null)
                return Enumerable.Empty<KeyValuePair<string, AttributeGroup>>().GetEnumerator();

            return Attributes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            if (Attributes == null)
                return Enumerable.Empty<KeyValuePair<string, AttributeGroup>>().GetEnumerator();

            return Attributes.GetEnumerator();
        }
    }

    public interface IMemberInfo {
        MemberIdentifier Identifier { get; }
        TypeInfo DeclaringType { get; }
        TypeReference ReturnType { get; }
        PropertyInfo DeclaringProperty { get; }
        EventInfo DeclaringEvent { get; }
        MetadataCollection Metadata { get; }
        string Name { get; }
        bool IsStatic { get; }
        bool IsFromProxy { get; }
        bool IsIgnored { get; }
        JSReadPolicy ReadPolicy { get; }
        JSWritePolicy WritePolicy { get; }
        JSInvokePolicy InvokePolicy { get; }
        IEnumerable<OverrideInfo> Overrides { get; }
    }

    public abstract class MemberInfo<T> : IMemberInfo
        where T : MemberReference, ICustomAttributeProvider 
    {
        public readonly MemberIdentifier Identifier;
        public readonly TypeInfo DeclaringType;
        public readonly ProxyInfo SourceProxy;
        public readonly T Member;
        public readonly MetadataCollection Metadata;
        public readonly bool IsExternal;
        public readonly bool IsFromProxy;
        protected readonly bool _IsIgnored;
        protected readonly JSReadPolicy _ReadPolicy;
        protected readonly JSWritePolicy _WritePolicy;
        protected readonly JSInvokePolicy _InvokePolicy;
        protected bool? _IsReturnIgnored;
        protected bool _WasReservedIdentifier;
        protected string _ShortName;

        public MemberInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            T member, ProxyInfo[] proxies, 
            bool isIgnored, bool isExternal, ProxyInfo sourceProxy
        ) {
            Identifier = identifier;

            _ReadPolicy = JSReadPolicy.Unmodified;
            _WritePolicy = JSWritePolicy.Unmodified;
            _InvokePolicy = JSInvokePolicy.Unmodified;

            _IsIgnored = isIgnored || TypeInfo.IsIgnoredName(member.Name, member is FieldReference);
            IsExternal = isExternal;
            IsFromProxy = sourceProxy != null;
            SourceProxy = sourceProxy;
            DeclaringType = parent;

            Member = member;
            Metadata = new MetadataCollection(member);

            if (proxies != null)
            foreach (var proxy in proxies) {
                ICustomAttributeProvider proxyMember;

                if (proxy.GetMember<ICustomAttributeProvider>(identifier, out proxyMember)) {
                    var meta = new MetadataCollection(proxyMember);
                    Metadata.Update(meta, proxy.AttributePolicy == JSProxyAttributePolicy.ReplaceAll);
                }
            }

            if (Metadata.HasAttribute("JSIL.Meta.JSIgnore"))
                _IsIgnored = true;

            if (Metadata.HasAttribute("JSIL.Meta.JSExternal") || Metadata.HasAttribute("JSIL.Meta.JSReplacement"))
                IsExternal = true;

            var parms = Metadata.GetAttributeParameters("JSIL.Meta.JSPolicy");
            if (parms != null) {
                foreach (var param in parms) {
                    switch (param.Type.FullName) {
                        case "JSIL.Meta.JSReadPolicy":
                            _ReadPolicy = (JSReadPolicy)param.Value;
                        break;
                        case "JSIL.Meta.JSWritePolicy":
                            _WritePolicy = (JSWritePolicy)param.Value;
                        break;
                        case "JSIL.Meta.JSInvokePolicy":
                            _InvokePolicy = (JSInvokePolicy)param.Value;
                        break;
                    }
                }
            }

            _WasReservedIdentifier = Util.ReservedIdentifiers.Contains(Name);
        }

        public string ShortName {
            get {
                if (_ShortName == null)
                    _ShortName = GetShortName(this.Member);

                return _ShortName;
            }
        }

        // Sometimes the type system prefixes the name of a member with some or all of the declaring type's name.
        //  The rules seem to be random, so just strip it off.
        protected static string GetShortName (MemberReference member) {
            var result = member.Name;
            int lastIndex = result.LastIndexOfAny(new char[] { '.', '/', '+', ':' });
            if (lastIndex >= 1)
                result = result.Substring(lastIndex + 1);
            return result;
        }

        protected virtual string GetName () {
            return ChangedName ?? Member.Name;
        }

        public virtual IEnumerable<OverrideInfo> Overrides {
            get {
                yield break;
            }
        }

        public ITypeInfoSource Source {
            get { return DeclaringType.Source; }
        }

        MemberIdentifier IMemberInfo.Identifier {
            get { return Identifier; }
        }

        bool IMemberInfo.IsFromProxy {
            get { return IsFromProxy; }
        }

        TypeInfo IMemberInfo.DeclaringType {
            get { return DeclaringType; }
        }

        MetadataCollection IMemberInfo.Metadata {
            get { return Metadata; }
        }

        public virtual bool IsIgnored {
            get {
                if (_IsIgnored)
                    return true;
                else if (DeclaringType.IsIgnored)
                    return true;

                if (_IsReturnIgnored.HasValue)
                    return _IsReturnIgnored.Value;
                else {
                    TypeInfo typeInfo;
                    bool isReturnIgnored = DeclaringType.IsTypeIgnored(ReturnType, out typeInfo);
                    if ((typeInfo != null) && typeInfo.IsFullyInitialized)
                        _IsReturnIgnored = isReturnIgnored;
                    return isReturnIgnored;
                }
            }
        }

        public virtual string ChangedName {
            get {
                var parms = Metadata.GetAttributeParameters("JSIL.Meta.JSChangeName");
                if (parms != null)
                    return (string)parms[0].Value;

                if (_WasReservedIdentifier)
                    return "$" + Member.Name;

                return null;
            }
        }

        public string Name {
            get {
                return GetName();
            }
        }

        public abstract bool IsStatic {
            get;
        }

        public abstract TypeReference ReturnType {
            get;
        }

        public virtual PropertyInfo DeclaringProperty {
            get { return null; }
        }

        public virtual EventInfo DeclaringEvent {
            get { return null; }
        }

        public JSReadPolicy ReadPolicy {
            get { return _ReadPolicy; }
        }

        public JSWritePolicy WritePolicy {
            get { return _WritePolicy; }
        }

        public JSInvokePolicy InvokePolicy {
            get { return _InvokePolicy; }
        }

        public override string ToString () {
            string result;
            if (IsFromProxy)
                result = String.Format("{0}::{1} (from {2})", DeclaringType.FullName, Member.Name, SourceProxy.Name);
            else
                result = String.Format("{0}::{1}", DeclaringType.FullName, Member.Name);

            return result;
        }
    }

    public class FieldInfo : MemberInfo<FieldDefinition> {
        public readonly bool IsBackingField;
        public readonly TypeReference FieldType;
        protected readonly string OriginalName;

        public FieldInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            FieldDefinition field, ProxyInfo[] proxies,
            ProxyInfo sourceProxy
        ) : base(
            parent, identifier, field, proxies, 
            TypeUtil.IsIgnoredType(field.FieldType), false, sourceProxy
        ) {
            OriginalName = TypeInfo.GetOriginalName(Name, out IsBackingField);

            if (IsBackingField)
                OriginalName = String.Format("{0}${1}", Util.EscapeIdentifier(parent.Name), OriginalName);

            var psa = Metadata.GetAttribute("JSIL.Meta.JSPackedArray");

            if ((psa != null) && (psa.Entries.Count > 0)) {
                FieldType = PackedArrayUtil.MakePackedArrayType(Member.FieldType, psa.Entries[0].Type);
            } else {
                FieldType = Member.FieldType;
            }
        }

        protected override string GetName () {
            if (OriginalName != null)
                return OriginalName;
            else
                return base.GetName();
        }

        public override TypeReference ReturnType {
            get { return FieldType; }
        }

        public bool IsImmutable {
            get {
                return Member.IsInitOnly || Metadata.HasAttribute("JSIL.Meta.JSImmutable") || DeclaringType.Metadata.HasAttribute("JSIL.Meta.JSImmutable");
            }
        }

        public override bool IsStatic {
            get { return Member.IsStatic; }
        }
    }

    public class PropertyInfo : MemberInfo<PropertyDefinition> {
        public MethodInfo Getter, Setter;
        public readonly bool IsAutoProperty;
        public readonly string BackingFieldName;

        public PropertyInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            PropertyDefinition property, ProxyInfo[] proxies, 
            ProxyInfo sourceProxy
        ) : base(
            parent, identifier, property, proxies, 
            TypeUtil.IsIgnoredType(property.PropertyType), false, sourceProxy
        ) {
            IsAutoProperty = (Member.GetMethod ?? Member.SetMethod).CustomAttributes.Any(
                (ca) => ca.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"
            );

            if (IsAutoProperty)
                BackingFieldName = String.Format("{0}${1}$value", Util.EscapeIdentifier(parent.Name), Util.EscapeIdentifier(Name));
            else
                BackingFieldName = null;
        }

        protected override string GetName () {
            string result;
            result = ChangedName ?? Member.Name;

            return result;
        }

        public override TypeReference ReturnType {
            get { return Member.PropertyType; }
        }

        public bool IsVirtual {
            get { return (Member.GetMethod ?? Member.SetMethod).IsVirtual; }
        }

        public bool IsPublic {
            get { return (Member.GetMethod ?? Member.SetMethod).IsPublic; }
        }

        public override bool IsStatic {
            get { return (Member.GetMethod ?? Member.SetMethod).IsStatic; }
        }
    }

    public class EventInfo : MemberInfo<EventDefinition> {
        public EventInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            EventDefinition evt, ProxyInfo[] proxies,
            ProxyInfo sourceProxy
        ) : base(
            parent, identifier, evt, proxies, false, false, sourceProxy
        ) {
        }

        public bool IsVirtual {
            get { return (Member.AddMethod ?? Member.RemoveMethod).IsVirtual; }
        }

        public bool IsPublic {
            get { return (Member.AddMethod ?? Member.RemoveMethod).IsPublic; }
        }

        public override bool IsStatic {
            get { return (Member.AddMethod ?? Member.RemoveMethod).IsStatic; }
        }

        public override TypeReference ReturnType {
            get { return Member.EventType; }
        }
    }

    public class MethodInfo : MemberInfo<MethodDefinition> {
        public readonly ParameterDefinition[] Parameters;
        public readonly string[] GenericParameterNames;
        public readonly PropertyInfo Property = null;
        public readonly EventInfo Event = null;
        public readonly bool IsGeneric;
        public readonly bool IsConstructor;
        public readonly bool IsVirtual;
        public readonly bool IsSealed;

        protected NamedMethodSignature _Signature = null;

        protected MethodGroupInfo _MethodGroup = null;
        protected bool? _IsOverloadedRecursive;
        protected bool? _IsRedefinedRecursive;
        protected bool? _ParametersIgnored;

        public MethodInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            MethodDefinition method, ProxyInfo[] proxies,
            ProxyInfo sourceProxy
        ) : base (
            parent, identifier, method, proxies,
            TypeUtil.IsIgnoredType(method.ReturnType) || 
                method.Parameters.Any((p) => TypeUtil.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl,
            sourceProxy
        ) {
            Parameters = method.Parameters.ToArray();
            GenericParameterNames = (from p in method.GenericParameters select p.Name).ToArray();
            IsGeneric = method.HasGenericParameters;
            IsConstructor = method.Name == ".ctor";
            IsVirtual = method.IsVirtual;
            IsSealed = method.IsFinal || method.DeclaringType.IsSealed;
        }

        public MethodInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            MethodDefinition method, ProxyInfo[] proxies, 
            PropertyInfo property, ProxyInfo sourceProxy
        ) : base (
            parent, identifier, method, proxies,
            TypeUtil.IsIgnoredType(method.ReturnType) || 
                method.Parameters.Any((p) => TypeUtil.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || 
                method.IsInternalCall || method.IsPInvokeImpl || property.IsExternal,
            sourceProxy
        ) {
            Property = property;
            Parameters = method.Parameters.ToArray();
            GenericParameterNames = (from p in method.GenericParameters select p.Name).ToArray();
            IsGeneric = method.HasGenericParameters;
            IsConstructor = method.Name == ".ctor";
            IsVirtual = method.IsVirtual;
            IsSealed = method.IsFinal || method.DeclaringType.IsSealed;

            if (property != null)
                Metadata.Update(property.Metadata, false);
        }

        public MethodInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            MethodDefinition method, ProxyInfo[] proxies, 
            EventInfo evt, ProxyInfo sourceProxy
        ) : base(
            parent, identifier, method, proxies,
            TypeUtil.IsIgnoredType(method.ReturnType) ||
                method.Parameters.Any((p) => TypeUtil.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl,
            sourceProxy
        ) {
            Event = evt;
            Parameters = method.Parameters.ToArray();
            GenericParameterNames = (from p in method.GenericParameters select p.Name).ToArray();
            IsGeneric = method.HasGenericParameters;
            IsConstructor = method.Name == ".ctor";
            IsVirtual = method.IsVirtual;
            IsSealed = method.IsFinal || method.DeclaringType.IsSealed;

            if (evt != null)
                Metadata.Update(evt.Metadata, false);
        }

        protected void MakeSignature () {
            _Signature = new NamedMethodSignature(
                Name, new MethodSignature(
                    Source, 
                    ReturnType, (from p in Parameters select p.ParameterType).ToArray(),
                    GenericParameterNames
                )
            );
        }

        protected override string GetName () {
            return GetName(false);
        }

        public override IEnumerable<OverrideInfo> Overrides {
            get {
                var typeInfo = DeclaringType.Source;
                var implementationType = DeclaringType.Definition;

                foreach (var @override in Member.Overrides)
                    yield return new OverrideInfo(typeInfo, implementationType, @override);
            }
        }

        public NamedMethodSignature NamedSignature {
            get {
                if (_Signature == null)
                    MakeSignature();

                return _Signature;
            }
        }

        public MethodSignature Signature {
            get {
                if (_Signature == null)
                    MakeSignature();

                return _Signature.Signature;
            }
        }

        public string ForcedNewName {
            private get;
            set;
        }

        public override string ChangedName {
            get {
                if (ForcedNewName != null)
                    return ForcedNewName;

                if (Property != null) {
                    var propertyChangedName = Property.ChangedName;
                    if (propertyChangedName != null)
                        return ShortName.Substring(0, ShortName.IndexOf('_') + 1) + propertyChangedName;
                }

                return base.ChangedName;
            }
        }

        public bool IsOverloaded {
            get {
                return _MethodGroup != null;
            }
        }

        public bool IsRedefinedRecursive {
            get {
                if (!_IsRedefinedRecursive.HasValue) {
                    _IsRedefinedRecursive = 
                        DeclaringType.MethodSignatures.GetDefinitionCountOf(this) > 1;
                }

                return _IsRedefinedRecursive.Value;
            }
        }

        public bool IsOverloadedRecursive {
            get {
                if (IsOverloaded)
                    return true;

                if (!_IsOverloadedRecursive.HasValue) {
                    _IsOverloadedRecursive = 
                        DeclaringType.MethodSignatures.GetOverloadCountOf(Name) > 1;
                }

                return _IsOverloadedRecursive.Value;
            }
        }

        public MethodGroupInfo MethodGroup {
            get {
                return _MethodGroup;
            }
            internal set {
                _IsOverloadedRecursive = null;
                _MethodGroup = value;
            }
        }

        public string GetName (bool stripGenericSuffix) {
            string result;

            var cn = ChangedName;
            if (cn != null)
                return cn;

            result = Member.Name;
            
            if (IsGeneric && !stripGenericSuffix)
                result = String.Format("{0}`{1}", result, Member.GenericParameters.Count);

            return result;
        }

        public override bool IsIgnored {
            get {
                if ((Event != null) && Event.IsIgnored)
                    return true;
                if ((Property != null) && Property.IsIgnored)
                    return true;

                if (base.IsIgnored)
                    return true;

                bool parametersIgnored;
                if (_ParametersIgnored.HasValue)
                    return _ParametersIgnored.Value;
                else {
                    bool canStore = true;
                    TypeInfo typeInfo;
                    foreach (var p in Member.Parameters) {
                        bool isIgnored = DeclaringType.IsTypeIgnored(p, out typeInfo);
                        if ((typeInfo == null) || !typeInfo.IsFullyInitialized)
                            canStore = false;

                        if (isIgnored) {
                            if (canStore)
                                _ParametersIgnored = true;
                            return true;
                        }
                    }

                    if (canStore)
                        _ParametersIgnored = false;
                }

                return false;
            }
        }

        public override bool IsStatic {
            get { return Member.IsStatic; }
        }

        public override PropertyInfo DeclaringProperty {
            get { return Property; }
        }

        public override EventInfo DeclaringEvent {
            get { return Event; }
        }

        public override TypeReference ReturnType {
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
        public readonly string Name;
        public readonly long Value;

        public EnumMemberInfo (TypeDefinition type, string name, long value) {
            DeclaringType = type;
            Name = name;
            Value = value;
        }
    }

    public static class PolicyExtensions {
        public static bool ApplyReadPolicy (this IMemberInfo member, JSExpression thisExpression, out JSExpression result) {
            result = null;
            if (member == null)
                return false;

            switch (member.ReadPolicy) {
                case JSReadPolicy.ReturnDefaultValue:
                    result = new JSDefaultValueLiteral(member.ReturnType);
                    return true;
                case JSReadPolicy.LogWarning:
                case JSReadPolicy.ThrowError:
                    result = new JSIgnoredMemberReference(member.ReadPolicy == JSReadPolicy.ThrowError, member, thisExpression);
                    return true;
            }

            if (member.IsIgnored) {
                result = new JSIgnoredMemberReference(true, member, thisExpression);
                return true;
            }

            return false;
        }

        public static bool ApplyWritePolicy (this IMemberInfo member, JSExpression thisExpression, JSExpression newValue, out JSExpression result) {
            result = null;
            if (member == null)
                return false;

            switch (member.WritePolicy) {
                case JSWritePolicy.DiscardValue:
                    result = new JSNullExpression();
                    return true;
                case JSWritePolicy.LogWarning:
                case JSWritePolicy.ThrowError:
                    result = new JSIgnoredMemberReference(member.WritePolicy == JSWritePolicy.ThrowError, member, thisExpression, newValue);
                    return true;
            }

            if (member.IsIgnored) {
                result = new JSIgnoredMemberReference(true, member, thisExpression, newValue);
                return true;
            }

            return false;
        }

        public static bool ApplyInvokePolicy (this IMemberInfo member, JSExpression thisExpression, JSExpression[] parameters, out JSExpression result) {
            result = null;
            if (member == null)
                return false;

            switch (member.InvokePolicy) {
                case JSInvokePolicy.ReturnDefaultValue:
                    result = new JSDefaultValueLiteral(member.ReturnType);
                    return true;
                case JSInvokePolicy.LogWarning:
                case JSInvokePolicy.ThrowError:
                    result = new JSIgnoredMemberReference(member.InvokePolicy == JSInvokePolicy.ThrowError, member, new[] { thisExpression }.Concat(parameters).ToArray());
                    return true;
            }

            if (member.IsIgnored) {
                result = new JSIgnoredMemberReference(true, member, new[] { thisExpression }.Concat(parameters).ToArray());
                return true;
            }

            return false;
        }
    }

    public class MethodTypeFactory : IDisposable {
        protected struct Key {
            private static int WroteMonoWarning = 0;

            public readonly bool IsValid;
            public readonly TypeReference ReturnType;
            public readonly TypeReference[] ParameterTypes;
            public readonly int HashCode;

            public Key (TypeReference returnType, IEnumerable<TypeReference> parameterTypes) {
                if (parameterTypes == null)
                    throw new NullReferenceException("parameterTypes");

                ReturnType = returnType;
                ParameterTypes = parameterTypes.ToArray();

                HashCode = ReturnType.FullName.GetHashCode() ^ ParameterTypes.Length;

                int i = 0;
                foreach (var p in ParameterTypes) {
                    HashCode ^= (p.FullName.GetHashCode() << i);
                    i += 1;
                }

                IsValid = true;
            }

            public int ParameterCount {
                get {
                    if (ParameterTypes == null)
                        return 0;

                    return ParameterTypes.Length;
                }
            }

            public override int GetHashCode () {
                return HashCode;
            }

            public bool Equals (Key rhs) {
                if (!IsValid || !rhs.IsValid) {
                    if (Interlocked.CompareExchange(ref WroteMonoWarning, 1, 0) == 0)
                        Console.Error.WriteLine("WARNING: Invalid Key passed to Key.Equals. You are probably running a version of Mono with a broken ConcurrentDictionary.");

                    return false;
                }

                if (!TypeUtil.TypesAreEqual(
                    ReturnType, rhs.ReturnType
                ))
                    return false;

                if (ParameterTypes.Length != rhs.ParameterTypes.Length)
                    return false;

                for (int i = 0, l = ParameterTypes.Length; i < l; i++) {
                    if (!TypeUtil.TypesAreEqual(ParameterTypes[i], rhs.ParameterTypes[i]))
                        return false;
                }

                return true;
            }

            public override bool Equals (object obj) {
                if (obj is Key)
                    return Equals((Key)obj);
                else
                    return base.Equals(obj);
            }

            public override string ToString () {
                if (ParameterTypes != null)
                    return String.Format("{0} ({1})", ReturnType, String.Join(", ", (object[])ParameterTypes));
                else
                    return String.Format("{0}", ReturnType);
            }
        }

        protected class KeyComparer : IEqualityComparer<Key> {
            bool IEqualityComparer<Key>.Equals (Key x, Key y) {
                return x.Equals(y);
            }

            int IEqualityComparer<Key>.GetHashCode (Key obj) {
                return obj.HashCode;
            }
        }

        protected struct MakeReferenceArgs {
            public TypeReference ReturnType;
            public TypeReference[] ParameterTypes;
            public TypeSystem TypeSystem;
        }

        protected readonly ConcurrentCache<Key, TypeReference> Cache = new ConcurrentCache<Key, TypeReference>(new KeyComparer());
        protected static readonly ConcurrentCache<Key, TypeReference>.CreatorFunction<MakeReferenceArgs> MakeReference;

        static MethodTypeFactory () {
            MakeReference = (signature, args) => {
                TypeReference genericDelegateType;

                var systemModule = args.TypeSystem.Boolean.Resolve().Module;
                bool hasReturnType;

                if (TypeUtil.TypesAreEqual(args.TypeSystem.Void, args.ReturnType)) {
                    hasReturnType = false;
                    var name = String.Format("System.Action`{0}", signature.ParameterCount);
                    genericDelegateType = systemModule.GetType(
                        signature.ParameterCount == 0 ? "System.Action" : name
                    );
                } else {
                    hasReturnType = true;
                    genericDelegateType = systemModule.GetType(String.Format(
                        "System.Func`{0}", signature.ParameterCount + 1
                    ));
                }

                if (genericDelegateType != null) {
                    var git = new GenericInstanceType(genericDelegateType);
                    foreach (var pt in args.ParameterTypes)
                        git.GenericArguments.Add(pt);

                    if (hasReturnType)
                        git.GenericArguments.Add(args.ReturnType);

                    return git;
                } else {
                    var baseType = systemModule.GetType("System.MulticastDelegate");

                    var td = new TypeDefinition(
                        "JSIL.Meta", "MethodSignature", TypeAttributes.Class | TypeAttributes.NotPublic, baseType
                    );
                    td.DeclaringType = baseType;

                    var invoke = new MethodDefinition(
                        "Invoke", MethodAttributes.Public, args.ReturnType
                    );
                    foreach (var pt in args.ParameterTypes)
                        invoke.Parameters.Add(new ParameterDefinition(pt));

                    td.Methods.Add(invoke);

                    return td;
                }
            };
        }

        public TypeReference Get (MethodReference method, TypeSystem typeSystem) {
            return Get(
                method.ReturnType,
                (from p in method.Parameters select p.ParameterType),
                typeSystem
            );
        }

        public TypeReference Get (TypeReference returnType, IEnumerable<TypeReference> parameterTypes, TypeSystem typeSystem) {
            TypeReference result;

            var args = new MakeReferenceArgs {
                ReturnType = returnType,
                ParameterTypes = parameterTypes.ToArray(),
                TypeSystem = typeSystem
            };
            var signature = new Key(returnType, args.ParameterTypes);

            return Cache.GetOrCreate(
                signature, args, MakeReference
            );
        }

        public void Dispose () {
            Cache.Dispose();
        }
    }

    public class OverrideInfo {
        public readonly TypeReference InterfaceType, ImplementationType;
        public readonly MemberIdentifier MemberIdentifier;

        public OverrideInfo (ITypeInfoSource typeInfo, TypeReference implementationType, MethodReference @override) {
            InterfaceType = @override.DeclaringType;
            ImplementationType = implementationType;
            MemberIdentifier = new MemberIdentifier(typeInfo, @override);
        }
    }
}
