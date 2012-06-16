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

    public class TypeIdentifier {
        public readonly string Assembly;
        public readonly string Namespace;
        public readonly string DeclaringTypeName;
        public readonly string Name;

        public TypeIdentifier (TypeDefinition type)
            : this ((TypeReference)type) {

            var asm = type.Module.Assembly;
            if (asm != null)
                Assembly = asm.FullName;
            else
                Assembly = null;
        }

        public TypeIdentifier (TypeReference type) {
            Assembly = null;
            Namespace = type.Namespace;
            Name = type.Name;

            var declaringType = type.DeclaringType;
            if (declaringType != null)
                DeclaringTypeName = declaringType.FullName;
            else
                DeclaringTypeName = null;
        }

        public bool Equals (TypeIdentifier rhs) {
            if (this == rhs)
                return true;

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
            var rhs = obj as TypeIdentifier;
            if (rhs != null)
                return Equals(rhs);

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
        public readonly bool IsProxy;
        public readonly bool IsDelegate;
        public readonly string Replacement;

        protected int _DerivedTypeCount = 0;
        protected string _FullName = null;
        protected bool _FullyInitialized = false;
        protected bool _IsIgnored = false;
        protected bool _IsExternal = false;
        protected bool _MethodGroupsInitialized = false;

        public TypeInfo (ITypeInfoSource source, ModuleInfo module, TypeDefinition type, TypeInfo declaringType, TypeInfo baseClass, TypeIdentifier identifier) {
            Identifier = identifier;
            DeclaringType = declaringType;
            BaseClass = baseClass;
            Source = source;
            Definition = type;
            bool isStatic = type.IsSealed && type.IsAbstract;

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
                    var escInfo = new MethodInfo(this, escIdentifier, proxy.ExtraStaticConstructor, Proxies, true);
                    escInfo.ForcedNewName = name;
                    ExtraStaticConstructors.Add(escInfo);
                }
            }
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
                } else if (proxy.MemberPolicy == JSProxyMemberPolicy.ReplaceDeclared) {
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

            return AddMember(method, true);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method, PropertyInfo owningProperty) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result, owningProperty.Member))
                return result;

            return AddMember(method, owningProperty, true);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method, EventInfo owningEvent) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result, owningEvent.Member))
                return result;

            return AddMember(method, owningEvent, true);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, FieldDefinition field) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, field, out result))
                return result;

            return AddMember(field);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, PropertyDefinition property) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, property, out result))
                return result;

            return AddMember(property, true);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, EventDefinition evt) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, evt, out result))
                return result;

            return AddMember(evt, true);
        }

        static readonly Regex MangledNameRegex = new Regex(@"\<([^>]*)\>([^_]*)__(.*)", RegexOptions.Compiled);
        static readonly Regex IgnoredKeywordRegex = new Regex(
            @"__BackingField|CS\$\<|__DisplayClass|\<PrivateImplementationDetails\>|" +
            @"Runtime\.CompilerServices\.CallSite|\<Module\>|__SiteContainer|" +
            @"__DynamicSite|__CachedAnonymousMethodDelegate", RegexOptions.Compiled
        );

        public static string GetOriginalName (string memberName) {
            var m = MangledNameRegex.Match(memberName);
            if (!m.Success)
                return null;

            var originalName = m.Groups[1].Value;
            if (String.IsNullOrWhiteSpace(originalName))
                originalName = String.Format("${0}", m.Groups[3].Value.Trim());
            if (String.IsNullOrWhiteSpace(originalName))
                return null;

            if (memberName.EndsWith("__BackingField", StringComparison.Ordinal))
                return String.Format("{0}$value", originalName);
            else
                return originalName;
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
            foreach (Match m2 in IgnoredKeywordRegex.Matches(shortName)) {
                if (m2.Success) {
                    switch (m2.Value) {
                        case "__BackingField":
                        case "__DisplayClass":
                            return false;

                        case "<PrivateImplementationDetails>":
                        case "Runtime.CompilerServices.CallSite":
                        case "<Module>":
                        case "__SiteContainer":
                        case "__DynamicSite":
                            return true;


                        case "CS$<":
                            if (!isField)
                                return true;

                            break;

                        case "__CachedAnonymousMethodDelegate":
                            if (isField)
                                return true;

                            break;
                    }
                }
            }

            var m = MangledNameRegex.Match(shortName);
            if (m.Success) {
                switch (m.Groups[2].Value) {
                    case "b":
                        // Lambda
                        return true;
                    case "c":
                        // Class
                        return false;
                    case "d":
                        // Enumerator
                        return false;
                }
            }

            return false;
        }

        protected void UpdateSignatureSet (string methodName, MethodSignature signature) {
            foreach (var t in SelfAndBaseTypesRecursive) {
                int existingCount;

                var set = t.MethodSignatures.GetOrCreateFor(methodName);

                set.Add(signature);
            }
        }

        protected MethodInfo AddMember (MethodDefinition method, PropertyInfo property, bool isFromProxy = false) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, property, isFromProxy);
            if (property.Member.GetMethod == method)
                property.Getter = (MethodInfo)result;
            else if (property.Member.SetMethod == method)
                property.Setter = (MethodInfo)result;

            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            UpdateSignatureSet(result.Name, ((MethodInfo)result).Signature);

            return (MethodInfo)result;
        }

        protected MethodInfo AddMember (MethodDefinition method, EventInfo evt, bool isFromProxy = false) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, evt, isFromProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            UpdateSignatureSet(result.Name, ((MethodInfo)result).Signature);

            return (MethodInfo)result;
        }

        protected MethodInfo AddMember (MethodDefinition method, bool isFromProxy = false) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, isFromProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            if (method.Name == ".cctor")
                StaticConstructor = method;

            UpdateSignatureSet(result.Name, ((MethodInfo)result).Signature);

            return (MethodInfo)result;
        }

        protected FieldInfo AddMember (FieldDefinition field) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, field);
            if (Members.TryGetValue(identifier, out result))
                return (FieldInfo)result;

            result = new FieldInfo(this, identifier, field, Proxies);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            return (FieldInfo)result;
        }

        protected PropertyInfo AddMember (PropertyDefinition property, bool isFromProxy = false) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, property);
            if (Members.TryGetValue(identifier, out result)) {
                if (!isFromProxy)
                    return (PropertyInfo)result;
                else
                    Members.TryRemove(identifier, out result);
            }

            result = new PropertyInfo(this, identifier, property, Proxies, isFromProxy);
            if (!Members.TryAdd(identifier, result))
                throw new InvalidOperationException();

            return (PropertyInfo)result;
        }

        protected EventInfo AddMember (EventDefinition evt, bool isFromProxy = false) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(this.Source, evt);
            if (Members.TryGetValue(identifier, out result))
                return (EventInfo)result;

            result = new EventInfo(this, identifier, evt, Proxies, isFromProxy);
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
        public class Entry {
            public readonly CustomAttributeArgument[] Arguments;

            public Entry (CustomAttribute ca) {
                Arguments = ca.ConstructorArguments.ToArray();
            }
        }

        public bool Inherited;
        public string Name;
        public readonly List<Entry> Entries = new List<Entry>();
    }

    public class MetadataCollection : IEnumerable<KeyValuePair<string, AttributeGroup>> {
        protected Dictionary<string, AttributeGroup> Attributes = null;

        public MetadataCollection (ICustomAttributeProvider target) {
            if (target.CustomAttributes.Count == 0)
                return;

            foreach (var ca in target.CustomAttributes) {
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
            return Attributes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
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
    }

    public abstract class MemberInfo<T> : IMemberInfo
        where T : MemberReference, ICustomAttributeProvider 
    {
        public readonly MemberIdentifier Identifier;
        public readonly TypeInfo DeclaringType;
        public readonly T Member;
        public readonly MetadataCollection Metadata;
        public readonly bool IsExternal;
        internal readonly bool IsFromProxy;
        protected readonly bool _IsIgnored;
        protected readonly JSReadPolicy _ReadPolicy;
        protected readonly JSWritePolicy _WritePolicy;
        protected readonly JSInvokePolicy _InvokePolicy;
        protected bool? _IsReturnIgnored;
        protected bool _WasReservedIdentifier;

        public MemberInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            T member, ProxyInfo[] proxies, 
            bool isIgnored, bool isExternal, bool isFromProxy
        ) {
            Identifier = identifier;

            _ReadPolicy = JSReadPolicy.Unmodified;
            _WritePolicy = JSWritePolicy.Unmodified;
            _InvokePolicy = JSInvokePolicy.Unmodified;

            _IsIgnored = isIgnored || TypeInfo.IsIgnoredName(member.Name, member is FieldReference);
            IsExternal = isExternal;
            IsFromProxy = isFromProxy;
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
            return Member.FullName;
        }
    }

    public class FieldInfo : MemberInfo<FieldDefinition> {
        protected readonly string OriginalName;

        public FieldInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            FieldDefinition field, ProxyInfo[] proxies
        ) : base(
            parent, identifier, field, proxies, 
            TypeUtil.IsIgnoredType(field.FieldType), false, false
        ) {
            OriginalName = TypeInfo.GetOriginalName(Name);
        }

        protected override string GetName () {
            if (OriginalName != null)
                return OriginalName;
            else
                return base.GetName();
        }

        public override TypeReference ReturnType {
            get { return Member.FieldType; }
        }

        public override bool IsStatic {
            get { return Member.IsStatic; }
        }
    }

    public class PropertyInfo : MemberInfo<PropertyDefinition> {
        public MethodInfo Getter, Setter;
        protected readonly string ShortName;

        public PropertyInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            PropertyDefinition property, ProxyInfo[] proxies, bool isFromProxy
        ) : base(
            parent, identifier, property, proxies, 
            TypeUtil.IsIgnoredType(property.PropertyType), false, isFromProxy
        ) {
            ShortName = GetShortName(property);
        }

        protected override string GetName () {
            string result;
            var declType = Member.DeclaringType.Resolve();
            var over = (Member.GetMethod ?? Member.SetMethod).Overrides.FirstOrDefault();

            if ((declType != null) && declType.IsInterface)
                result = ChangedName ?? String.Format("{0}.{1}", declType.Name, ShortName);
            else if (over != null)
                result = ChangedName ?? String.Format("{0}.{1}", over.DeclaringType.Name, ShortName);
            else
                result = ChangedName ?? ShortName;

            return result;
        }

        public override TypeReference ReturnType {
            get { return Member.PropertyType; }
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
            EventDefinition evt, ProxyInfo[] proxies, bool isFromProxy
        ) : base(
            parent, identifier, evt, proxies, false, false, isFromProxy
        ) {
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

        protected MethodSignature _Signature = null;

        protected MethodGroupInfo _MethodGroup = null;
        protected bool? _IsOverloadedRecursive;
        protected bool? _IsRedefinedRecursive;
        protected bool? _ParametersIgnored;
        protected readonly string ShortName;

        public MethodInfo (
            TypeInfo parent, MemberIdentifier identifier, 
            MethodDefinition method, ProxyInfo[] proxies, bool isFromProxy
        ) : base (
            parent, identifier, method, proxies,
            TypeUtil.IsIgnoredType(method.ReturnType) || 
                method.Parameters.Any((p) => TypeUtil.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl,
            isFromProxy
        ) {
            ShortName = GetShortName(method);
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
            PropertyInfo property, bool isFromProxy
        ) : base (
            parent, identifier, method, proxies,
            TypeUtil.IsIgnoredType(method.ReturnType) || 
                method.Parameters.Any((p) => TypeUtil.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || 
                method.IsInternalCall || method.IsPInvokeImpl || property.IsExternal,
            isFromProxy
        ) {
            Property = property;
            ShortName = GetShortName(method);
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
            EventInfo evt, bool isFromProxy
        ) : base(
            parent, identifier, method, proxies,
            TypeUtil.IsIgnoredType(method.ReturnType) ||
                method.Parameters.Any((p) => TypeUtil.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl,
            isFromProxy
        ) {
            Event = evt;
            ShortName = GetShortName(method);
            Parameters = method.Parameters.ToArray();
            GenericParameterNames = (from p in method.GenericParameters select p.Name).ToArray();
            IsGeneric = method.HasGenericParameters;
            IsConstructor = method.Name == ".ctor";
            IsVirtual = method.IsVirtual;
            IsSealed = method.IsFinal || method.DeclaringType.IsSealed;
        }

        protected void MakeSignature () {
            _Signature = new MethodSignature(
                ReturnType, (from p in Parameters select p.ParameterType).ToArray(),
                GenericParameterNames
            );
        }

        protected override string GetName () {
            return GetName(false);
        }

        public MethodSignature Signature {
            get {
                if (_Signature == null)
                    MakeSignature();

                return _Signature;
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
            var declType = Member.DeclaringType.Resolve();
            var over = Member.Overrides.FirstOrDefault();

            var cn = ChangedName;
            if (cn != null)
                return cn;

            if ((declType != null) && declType.IsInterface) {
                result = String.Format("{0}.{1}", TypeUtil.GetLocalName(declType), ShortName);
            // FIXME: Enable this so MultipleGenericInterfaces2.cs passes.
            /*
            } else if (Member.Name.IndexOf(".") > 0) {
                // Qualified reference to an interface member
                result = Member.Name;
            */
            } else if (over != null)
                result = String.Format("{0}.{1}", TypeUtil.GetLocalName(over.DeclaringType.Resolve()), ShortName);
            else
                result = ShortName;
            
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
        protected struct MethodSignature {
            public readonly TypeReference ReturnType;
            public readonly IEnumerable<TypeReference> ParameterTypes;
            public readonly int ParameterCount;
            private readonly int HashCode;

            public MethodSignature (TypeReference returnType, IEnumerable<TypeReference> parameterTypes) {
                ReturnType = returnType;
                ParameterTypes = parameterTypes;
                ParameterCount = parameterTypes.Count();

                HashCode = ReturnType.FullName.GetHashCode() ^ ParameterCount;

                int i = 0;
                foreach (var p in ParameterTypes) {
                    HashCode ^= (p.FullName.GetHashCode() << i);
                    i += 1;
                }
            }

            public override int GetHashCode () {
                return HashCode;
            }

            public bool Equals (MethodSignature rhs) {
                if (!TypeUtil.TypesAreEqual(
                    ReturnType, rhs.ReturnType
                ))
                    return false;

                if (ParameterCount != rhs.ParameterCount)
                    return false;

                using (var e1 = ParameterTypes.GetEnumerator())
                using (var e2 = rhs.ParameterTypes.GetEnumerator())
                    while (e1.MoveNext() && e2.MoveNext()) {
                        if (!TypeUtil.TypesAreEqual(e1.Current, e2.Current))
                            return false;
                    }

                return true;
            }

            public override bool Equals (object obj) {
                if (obj is MethodSignature)
                    return Equals((MethodSignature)obj);
                else
                    return base.Equals(obj);
            }
        }

        protected readonly ConcurrentCache<MethodSignature, TypeReference> Cache = new ConcurrentCache<MethodSignature, TypeReference>();

        public TypeReference Get (MethodReference method, TypeSystem typeSystem) {
            return Get(
                method.ReturnType,
                (from p in method.Parameters select p.ParameterType),
                typeSystem
            );
        }

        public TypeReference Get (TypeReference returnType, IEnumerable<TypeReference> parameterTypes, TypeSystem typeSystem) {
            TypeReference result;
            var ptypes = parameterTypes.ToArray();
            var signature = new MethodSignature(returnType, ptypes);

            return Cache.GetOrCreate(
                signature, () => {
                    TypeReference genericDelegateType;

                    var systemModule = typeSystem.Boolean.Resolve().Module;
                    bool hasReturnType;

                    if (TypeUtil.TypesAreEqual(typeSystem.Void, returnType)) {
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
                        foreach (var pt in ptypes)
                            git.GenericArguments.Add(pt);

                        if (hasReturnType)
                            git.GenericArguments.Add(returnType);

                        return git;
                    } else {
                        var baseType = systemModule.GetType("System.MulticastDelegate");

                        var td = new TypeDefinition(
                            "JSIL.Meta", "MethodSignature", TypeAttributes.Class | TypeAttributes.NotPublic, baseType
                        );
                        td.DeclaringType = baseType;

                        var invoke = new MethodDefinition(
                            "Invoke", MethodAttributes.Public, returnType
                        );
                        foreach (var pt in ptypes)
                            invoke.Parameters.Add(new ParameterDefinition(pt));

                        td.Methods.Add(invoke);

                        return td;
                    }
                }
            );
        }

        public void Dispose () {
            Cache.Dispose();
        }
    }
}
