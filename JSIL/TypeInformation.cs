using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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

    public class TypeIdentifier {
        public readonly string Assembly;
        public readonly string Namespace;
        public readonly string DeclaringTypeName;
        public readonly string Name;

        public TypeIdentifier (TypeDefinition type)
            : this ((TypeReference)type) {
            Assembly = type.Module.Assembly.FullName;
        }

        public TypeIdentifier (TypeReference type) {
            Assembly = null;
            Namespace = type.Namespace;
            if (type.DeclaringType != null)
                DeclaringTypeName = type.DeclaringType.FullName;
            else
                DeclaringTypeName = null;
            Name = type.Name;
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
            if (DeclaringTypeName != null)
                return String.Format("{0} {1}.{2}/{3}", Assembly, Namespace, DeclaringTypeName, Name);
            else
                return String.Format("{0} {1}.{2}", Assembly, Namespace, Name);
        }
    }

    public class QualifiedMemberIdentifier {
        public readonly TypeIdentifier Type;
        public readonly MemberIdentifier Member;

        public QualifiedMemberIdentifier (TypeIdentifier type, MemberIdentifier member) {
            Type = type;
            Member = member;
        }

        public override int GetHashCode () {
            return Type.GetHashCode() ^ Member.GetHashCode();
        }

        public bool Equals (QualifiedMemberIdentifier rhs) {
            if (!Type.Equals(rhs.Type))
                return false;

            return Member.Equals(rhs.Member);
        }

        public override bool Equals (object obj) {
            var rhs = obj as QualifiedMemberIdentifier;
            if (rhs != null)
                return Equals(rhs);
            else
                return base.Equals(obj);
        }

        public override string ToString () {
            return String.Format("{0} {1}", Type, Member);
        }
    }

    public class MemberIdentifier {
        public enum MemberType {
            Field,
            Property,
            Event,
            Method,
        }

        public static readonly Dictionary<string, string[]> Proxies = new Dictionary<string, string[]>();

        public readonly MemberType Type;
        public readonly string Name;
        public readonly TypeReference ReturnType;
        public readonly int ParameterCount;
        public readonly IEnumerable<TypeReference> ParameterTypes;

        public static readonly IEnumerable<TypeReference> AnyParameterTypes = new TypeReference[0] {};

        public static void ResetProxies () {
            Proxies.Clear();
        }

        public static MemberIdentifier New (MemberReference mr) {
            var method = mr as MethodReference;
            var property = mr as PropertyReference;
            var evt = mr as EventReference;
            var field = mr as FieldReference;

            if (method != null)
                return new MemberIdentifier(method);
            else if (property != null)
                return new MemberIdentifier(property);
            else if (evt != null)
                return new MemberIdentifier(evt);
            else if (field != null)
                return new MemberIdentifier(field);
            else
                throw new NotImplementedException();
        }

        public MemberIdentifier (MethodReference mr) {
            Type = MemberType.Method;
            Name = mr.Name;
            ReturnType = mr.ReturnType;
            ParameterCount = mr.Parameters.Count;
            ParameterTypes = GetParameterTypes(mr.Parameters);
            LocateProxy(mr);
        }

        public MemberIdentifier (PropertyReference pr) {
            Type = MemberType.Property;
            Name = pr.Name;
            ReturnType = pr.PropertyType;
            ParameterCount = 0;
            ParameterTypes = null;
            LocateProxy(pr);

            var pd = pr.Resolve();
            if (pd != null) {
                if (pd.GetMethod != null) {
                    ParameterCount = pd.GetMethod.Parameters.Count;
                    ParameterTypes = (from p in pd.GetMethod.Parameters select p.ParameterType);
                } else if (pd.SetMethod != null) {
                    ParameterCount = pd.SetMethod.Parameters.Count - 1;
                    ParameterTypes = (from p in pd.SetMethod.Parameters select p.ParameterType).Take(ParameterCount);
                }
            }
        }

        public MemberIdentifier (FieldReference fr) {
            Type = MemberType.Field;
            Name = fr.Name;
            ReturnType = fr.FieldType;
            ParameterCount = 0;
            ParameterTypes = null;
            LocateProxy(fr);
        }

        public MemberIdentifier (EventReference er) {
            Type = MemberType.Event;
            Name = er.Name;
            ReturnType = er.EventType;
            ParameterCount = 0;
            ParameterTypes = null;
            LocateProxy(er);
        }

        protected static void LocateProxy (MemberReference mr) {
            var fullName = mr.DeclaringType.FullName;
            if (Proxies.ContainsKey(fullName))
                return;

            var icap = mr.DeclaringType as ICustomAttributeProvider;
            if (icap == null)
                return;

            var metadata = new MetadataCollection(icap);
            if (!metadata.HasAttribute("JSIL.Proxy.JSProxy"))
                return;

            string[] proxyTargets = null;
            var args = metadata.GetAttributeParameters("JSIL.Proxy.JSProxy");

            foreach (var arg in args) {
                switch (arg.Type.FullName) {
                    case "System.Type":
                        proxyTargets = new string[] { ((TypeReference)arg.Value).FullName };

                        break;
                    case "System.Type[]": {
                        var values = (CustomAttributeArgument[])arg.Value;
                        proxyTargets = new string[values.Length];
                        for (var i = 0; i < proxyTargets.Length; i++)
                            proxyTargets[i] = ((TypeReference)values[i].Value).FullName;

                        break;
                    }
                    case "System.String": {
                        proxyTargets = new string[] { (string)arg.Value };

                        break;
                    }
                    case "System.String[]": {
                        var values = (CustomAttributeArgument[])arg.Value;
                        proxyTargets = (from v in values select (string)v.Value).ToArray();

                        break;
                    }
                }
            }

            if (proxyTargets != null)
                Proxies[fullName] = proxyTargets;
        }

        static IEnumerable<TypeReference> GetParameterTypes (IList<ParameterDefinition> parameters) {
            if (
                (parameters.Count == 1) && 
                (from ca in parameters[0].CustomAttributes 
                 where ca.AttributeType.FullName == "System.ParamArrayAttribute" 
                 select ca).Count() == 1
            ) {
                var t = JSExpression.DeReferenceType(parameters[0].ParameterType);
                var at = t as ArrayType;
                if ((at != null) && IsAnyType(at.ElementType))
                    return AnyParameterTypes;
            }

            return (from p in parameters select p.ParameterType);
        }

        static bool IsAnyType (TypeReference t) {
            if (t == null)
                return false;

            return (t.Name == "AnyType" && t.Namespace == "JSIL.Proxy") ||
                (JSExpression.DeReferenceType(t).IsGenericParameter);
        }

        bool TypesAreEqual (TypeReference lhs, TypeReference rhs) {
            if (lhs == null || rhs == null)
                return (lhs == rhs);

            var lhsReference = lhs as ByReferenceType;
            var rhsReference = rhs as ByReferenceType;

            if ((lhsReference != null) || (rhsReference != null)) {
                if ((lhsReference == null) || (rhsReference == null))
                    return false;

                return TypesAreEqual(lhsReference.ElementType, rhsReference.ElementType);
            }

            var lhsArray = lhs as ArrayType;
            var rhsArray = rhs as ArrayType;

            if ((lhsArray != null) || (rhsArray != null)) {
                if ((lhsArray == null) || (rhsArray == null))
                    return false;

                return TypesAreEqual(lhsArray.ElementType, rhsArray.ElementType);
            }

            var lhsGit = lhs as GenericInstanceType;
            var rhsGit = rhs as GenericInstanceType;

            if ((lhsGit != null) && (rhsGit != null)) {
                if (lhsGit.GenericArguments.Count != rhsGit.GenericArguments.Count)
                    return false;

                if (!TypesAreEqual(lhsGit.ElementType, rhsGit.ElementType))
                    return false;

                using (var eLeft = lhsGit.GenericArguments.GetEnumerator())
                using (var eRight = rhsGit.GenericArguments.GetEnumerator())
                while (eLeft.MoveNext() && eRight.MoveNext()) {
                    if (!TypesAreEqual(eLeft.Current, eRight.Current))
                        return false;
                }

                return true;
            }

            string[] proxyTargets;
            if (
                Proxies.TryGetValue(lhs.FullName, out proxyTargets) &&
                (proxyTargets != null) &&
                proxyTargets.Contains(rhs.FullName)
            ) {
                return true;
            } else if (
                Proxies.TryGetValue(rhs.FullName, out proxyTargets) &&
                (proxyTargets != null) &&
                proxyTargets.Contains(lhs.FullName)
            ) {
                return true;
            }

            if (IsAnyType(lhs) || IsAnyType(rhs))
                return true;

            return ILBlockTranslator.TypesAreEqual(lhs, rhs);
        }

        public bool Equals (MemberIdentifier rhs) {
            if (Type != rhs.Type)
                return false;

            if (!String.Equals(Name, rhs.Name))
                return false;

            if (!TypesAreEqual(ReturnType, rhs.ReturnType))
                return false;

            if ((ParameterTypes == AnyParameterTypes) || (rhs.ParameterTypes == AnyParameterTypes)) {
            } else if ((ParameterTypes == null) || (rhs.ParameterTypes == null)) {
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
            var rhs = obj as MemberIdentifier;
            if (rhs != null)
                return Equals(rhs);

            return base.Equals(obj);
        }

        public override int GetHashCode () {
            return Type.GetHashCode() ^ Name.GetHashCode();
        }

        public override string ToString () {
            if (ParameterTypes != null)
                return String.Format(
                    "{0} {1} ( {2} )", ReturnType, Name,
                    String.Join(", ", (from p in ParameterTypes select p.ToString()).ToArray())
                );
            else
                return String.Format(
                    "{0} {1}", ReturnType, Name
                );
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
        public readonly string[] ProxiedTypeNames;

        public readonly TypeReference[] Interfaces;
        public readonly MetadataCollection Metadata;

        public readonly JSProxyAttributePolicy AttributePolicy;
        public readonly JSProxyMemberPolicy MemberPolicy;
        public readonly JSProxyInterfacePolicy InterfacePolicy;

        public readonly Dictionary<MemberIdentifier, FieldDefinition> Fields = new Dictionary<MemberIdentifier, FieldDefinition>();
        public readonly Dictionary<MemberIdentifier, PropertyDefinition> Properties = new Dictionary<MemberIdentifier, PropertyDefinition>();
        public readonly Dictionary<MemberIdentifier, EventDefinition> Events = new Dictionary<MemberIdentifier, EventDefinition>();
        public readonly Dictionary<MemberIdentifier, MethodDefinition> Methods = new Dictionary<MemberIdentifier, MethodDefinition>();

        public readonly bool IsInheritable;

        public ProxyInfo (TypeDefinition proxyType) {
            Definition = proxyType;
            Metadata = new MetadataCollection(proxyType);
            Interfaces = proxyType.Interfaces.ToArray();
            IsInheritable = true;
            ProxiedTypes = new TypeReference[0];
            ProxiedTypeNames = new string[0];

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
                        ProxiedTypes = new[] { (TypeReference)arg.Value };
                        break;
                    case "System.Type[]": {
                        var values = (CustomAttributeArgument[])arg.Value;
                        ProxiedTypes = new TypeReference[values.Length];
                        for (var i = 0; i < ProxiedTypes.Length; i++)
                            ProxiedTypes[i] = (TypeReference)values[i].Value;
                        break;
                    }
                    case "System.Boolean":
                        IsInheritable = (bool)arg.Value;
                        break;
                    case "System.String":
                        ProxiedTypeNames = new[] { (string)arg.Value };
                        break;
                    case "System.String[]": {
                        var values = (CustomAttributeArgument[])arg.Value;
                        ProxiedTypeNames = (from v in values select (string)v.Value).ToArray();
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
            }

            foreach (var field in proxyType.Fields) {
                if (!ILBlockTranslator.TypesAreEqual(field.DeclaringType, proxyType))
                    continue;

                Fields.Add(new MemberIdentifier(field), field);
            }

            foreach (var property in proxyType.Properties) {
                if (!ILBlockTranslator.TypesAreEqual(property.DeclaringType, proxyType))
                    continue;

                Properties.Add(new MemberIdentifier(property), property);
            }

            foreach (var evt in proxyType.Events) {
                if (!ILBlockTranslator.TypesAreEqual(evt.DeclaringType, proxyType))
                    continue;

                Events.Add(new MemberIdentifier(evt), evt);
            }

            foreach (var method in proxyType.Methods) {
                if (!ILBlockTranslator.TypesAreEqual(method.DeclaringType, proxyType))
                    continue;

                Methods.Add(new MemberIdentifier(method), method);
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

        public bool IsMatch (TypeReference type, bool? forcedInheritable) {
            bool inheritable = forcedInheritable.GetValueOrDefault(IsInheritable);

            foreach (var pt in ProxiedTypes) {
                bool isMatch;
                if (inheritable)
                    isMatch = ILBlockTranslator.TypesAreAssignable(pt, type);
                else
                    isMatch = ILBlockTranslator.TypesAreEqual(pt, type);

                if (isMatch)
                    return true;
            }

            foreach (var ptn in ProxiedTypeNames) {
                bool isMatch;
                if (inheritable)
                    isMatch = new[] { type.FullName }.Concat(ILBlockTranslator.AllBaseTypesOf(
                        ILBlockTranslator.GetTypeDefinition(type)).Select((t) => t.FullName))
                        .Contains(ptn);
                else
                    isMatch = type.FullName == ptn;

                if (isMatch)
                    return true;
            }

            return false;
        }
    }

    public class TypeInfo {
        public readonly TypeDefinition Definition;
        public readonly ITypeInfoSource Source;
        public readonly TypeInfo BaseClass;

        public readonly TypeInfo[] Interfaces;

        // This needs to be mutable so we can introduce a constructed cctor later
        public MethodDefinition StaticConstructor;
        public readonly HashSet<MethodDefinition> Constructors = new HashSet<MethodDefinition>();
        public readonly MetadataCollection Metadata;
        public readonly ProxyInfo[] Proxies;

        public readonly HashSet<MethodGroupInfo> MethodGroups = new HashSet<MethodGroupInfo>();

        public readonly bool IsFlagsEnum;
        public readonly Dictionary<long, EnumMemberInfo> ValueToEnumMember = new Dictionary<long, EnumMemberInfo>();
        public readonly Dictionary<string, EnumMemberInfo> EnumMembers = new Dictionary<string, EnumMemberInfo>();
        public readonly Dictionary<MemberIdentifier, IMemberInfo> Members = new Dictionary<MemberIdentifier, IMemberInfo>();
        public readonly bool IsProxy;
        public readonly bool IsDelegate;

        protected bool _IsIgnored = false;
        protected bool _MethodGroupsInitialized = false;

        public TypeInfo (ITypeInfoSource source, ModuleInfo module, TypeDefinition type, TypeInfo baseClass) {
            BaseClass = baseClass;
            Source = source;
            Definition = type;
            bool isStatic = type.IsSealed && type.IsAbstract;

            Proxies = source.GetProxies(type);
            Metadata = new MetadataCollection(type);

            // Do this check before copying attributes from proxy types, since that will copy their JSProxy attribute
            IsProxy = Metadata.HasAttribute("JSIL.Proxy.JSProxy");

            IsDelegate = (type.BaseType != null) && (
                (type.BaseType.FullName == "System.Delegate") ||
                (type.BaseType.FullName == "System.MulticastDelegate")
            );

            var interfaces = new HashSet<TypeInfo>(
                from i in type.Interfaces select source.Get(i)
            );

            foreach (var proxy in Proxies) {
                Metadata.Update(proxy.Metadata, proxy.AttributePolicy == JSProxyAttributePolicy.ReplaceAll);

                if (proxy.InterfacePolicy == JSProxyInterfacePolicy.ReplaceNone) {
                } else {
                    if (proxy.InterfacePolicy == JSProxyInterfacePolicy.ReplaceAll)
                        interfaces.Clear();

                    foreach (var i in proxy.Interfaces)
                        interfaces.Add(source.Get(i));
                }
            }

            Interfaces = interfaces.ToArray();

            _IsIgnored = module.IsIgnored ||
                IsIgnoredName(type.Name) ||
                Metadata.HasAttribute("JSIL.Meta.JSIgnore") ||
                Metadata.HasAttribute("System.Runtime.CompilerServices.UnsafeValueTypeAttribute") ||
                Metadata.HasAttribute("System.Runtime.CompilerServices.NativeCppClassAttribute");

            // Microsoft assemblies often contain global, private classes with these names that collide in the generated JS
            if (
                (type.DeclaringType == null) &&
                String.IsNullOrWhiteSpace(type.Namespace) &&
                (
                    (type.Name == "ThisAssembly") ||
                    (type.Name == "FXAssembly") ||
                    (type.Name == "AssemblyRef")
                ) &&
                type.IsSealed &&
                type.IsAbstract &&
                !type.IsPublic
            ) {
                _IsIgnored = true;
            }

            if (baseClass != null)
                _IsIgnored |= baseClass.IsIgnored;

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

            foreach (var proxy in Proxies) {
                var seenMethods = new HashSet<MethodDefinition>();

                foreach (var property in proxy.Properties.Values) {
                    var p = (PropertyInfo)AddProxyMember(proxy, property);

                    if (property.GetMethod != null) {
                        AddProxyMember(proxy, property.GetMethod, p);
                        seenMethods.Add(property.GetMethod);
                    }

                    if (property.SetMethod != null) {
                        AddProxyMember(proxy, property.SetMethod, p);
                        seenMethods.Add(property.SetMethod);
                    }
                }

                foreach (var evt in proxy.Events.Values) {
                    var e = (EventInfo)AddProxyMember(proxy, evt);

                    if (evt.AddMethod != null) {
                        AddProxyMember(proxy, evt.AddMethod, e);
                        seenMethods.Add(evt.AddMethod);
                    }

                    if (evt.RemoveMethod != null) {
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

                    // TODO: No way to detect whether the constructor was compiler-generated.
                    if ((method.Name == ".ctor") && (method.Parameters.Count == 0))
                        continue;

                    AddProxyMember(proxy, method);
                }
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
                                where !m.IsIgnored && !m.Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch")
                                group m by new {
                                    m.Name,
                                    m.IsStatic
                                } into mg
                                select mg).ToArray();

            foreach (var mg in methodGroups) {
                var count = mg.Count();
                if (count > 1) {
                    int i = 0;

                    var groupName = mg.First().Name;

                    foreach (var item in mg.OrderBy((m) => m.Member.MetadataToken.ToUInt32())) {
                        item.OverloadIndex = i;
                        i += 1;
                    }

                    MethodGroups.Add(new MethodGroupInfo(
                        this, mg.ToArray(), groupName
                    ));
                } else {
                    if (mg.Key.Name == ".cctor")
                        StaticConstructor = mg.First().Member;
                }
            }
        }

        public bool IsIgnored {
            get {
                if (Definition.DeclaringType != null) {
                    var dt = Source.GetExisting(Definition.DeclaringType);
                    if ((dt != null) && dt.IsIgnored)
                        return true;
                }

                return _IsIgnored;
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
            var identifier = MemberIdentifier.New(member);

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

                    Members.Remove(identifier);
                } else {
                    throw new ArgumentException();
                }
            }

            result = null;
            return false;
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result))
                return result;

            return AddMember(method);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method, PropertyInfo owningProperty) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result, owningProperty.Member))
                return result;

            return AddMember(method, owningProperty);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, MethodDefinition method, EventInfo owningEvent) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, method, out result, owningEvent.Member))
                return result;

            return AddMember(method, owningEvent);
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

            return AddMember(property);
        }

        protected IMemberInfo AddProxyMember (ProxyInfo proxy, EventDefinition evt) {
            IMemberInfo result;
            if (BeforeAddProxyMember(proxy, evt, out result))
                return result;

            return AddMember(evt);
        }

        static readonly Regex MangledNameRegex = new Regex(@"\<([^>]*)\>([^_]*)__(.*)", RegexOptions.Compiled);

        public static string GetOriginalName (string memberName) {
            var m = MangledNameRegex.Match(memberName);
            if (!m.Success)
                return null;

            var originalName = m.Groups[1].Value;
            if (String.IsNullOrWhiteSpace(originalName))
                originalName = String.Format("${0}", m.Groups[3].Value.Trim());
            if (String.IsNullOrWhiteSpace(originalName))
                return null;

            if (memberName.Contains("__BackingField"))
                return String.Format("{0}$value", originalName);
            else
                return originalName;
        }

        public static bool IsIgnoredName (string shortName) {
            if (shortName.EndsWith("__BackingField"))
                return false;
            else if (shortName.Contains("__DisplayClass"))
                return false;
            else if (shortName.Contains("<PrivateImplementationDetails>"))
                return true;
            else if (shortName.Contains("Runtime.CompilerServices.CallSite"))
                return true;
            else if (shortName.Contains("__CachedAnonymousMethodDelegate"))
                return true;
            else if (shortName.Contains("<Module>"))
                return true;
            else if (shortName.Contains("__SiteContainer"))
                return true;
            else if (shortName.StartsWith("CS$<"))
                return true;
            else {
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
            }

            return false;
        }

        protected MethodInfo AddMember (MethodDefinition method, PropertyInfo property) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, property);
            Members.Add(identifier, result);
            return (MethodInfo)result;
        }

        protected MethodInfo AddMember (MethodDefinition method, EventInfo evt) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies, evt);
            Members.Add(identifier, result);
            return (MethodInfo)result;
        }

        protected MethodInfo AddMember (MethodDefinition method) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(method);
            if (Members.TryGetValue(identifier, out result))
                return (MethodInfo)result;

            result = new MethodInfo(this, identifier, method, Proxies);
            Members.Add(identifier, result);
            return (MethodInfo)result;
        }

        protected FieldInfo AddMember (FieldDefinition field) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(field);
            if (Members.TryGetValue(identifier, out result))
                return (FieldInfo)result;

            result = new FieldInfo(this, identifier, field, Proxies);
            Members.Add(identifier, result);
            return (FieldInfo)result;
        }

        protected PropertyInfo AddMember (PropertyDefinition property) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(property);
            if (Members.TryGetValue(identifier, out result))
                return (PropertyInfo)result;

            result = new PropertyInfo(this, identifier, property, Proxies);
            Members.Add(identifier, result);
            return (PropertyInfo)result;
        }

        protected EventInfo AddMember (EventDefinition evt) {
            IMemberInfo result;
            var identifier = new MemberIdentifier(evt);
            if (Members.TryGetValue(identifier, out result))
                return (EventInfo)result;

            result = new EventInfo(this, identifier, evt, Proxies);
            Members.Add(identifier, result);
            return (EventInfo)result;
        }

        internal bool IsTypeIgnored (ParameterDefinition pd) {
            return IsTypeIgnored(pd.ParameterType);
        }

        internal bool IsTypeIgnored (TypeReference t) {
            if (ILBlockTranslator.IsIgnoredType(t))
                return true;

            if (t.IsPrimitive)
                return false;
            else if (t.FullName == "System.Void")
                return false;

            var typeInfo = Source.GetExisting(t);
            if ((typeInfo != null) && typeInfo.IsIgnored)
                return true;

            return false;
        }
    }

    public class MetadataCollection {
        public readonly Dictionary<string, CustomAttribute> CustomAttributes = new Dictionary<string, CustomAttribute>();

        public MetadataCollection (ICustomAttributeProvider target) {
            foreach (var ca in target.CustomAttributes) {
                var key = ca.AttributeType.FullName;

                CustomAttributes[key] = ca;
            }
        }

        public void Update (MetadataCollection rhs, bool replaceAll) {
            if (replaceAll)
                CustomAttributes.Clear();

            foreach (var kvp in rhs.CustomAttributes)
                CustomAttributes[kvp.Key] = kvp.Value;
        }

        public bool HasAttribute (TypeReference attributeType) {
            return HasAttribute(attributeType.FullName);
        }

        public bool HasAttribute (Type attributeType) {
            return HasAttribute(attributeType.FullName);
        }

        public bool HasAttribute (string fullName) {
            var attr = GetAttribute(fullName);

            return attr != null;
        }

        public CustomAttribute GetAttribute (TypeReference attributeType) {
            return GetAttribute(attributeType.FullName);
        }

        public CustomAttribute GetAttribute (Type attributeType) {
            return GetAttribute(attributeType.FullName);
        }

        public CustomAttribute GetAttribute (string fullName) {
            CustomAttribute attr;

            if (CustomAttributes.TryGetValue(fullName, out attr))
                return attr;

            return null;
        }

        public IList<CustomAttributeArgument> GetAttributeParameters (string fullName) {
            var attr = GetAttribute(fullName);
            if (attr == null)
                return null;

            return attr.ConstructorArguments;
        }
    }

    public interface IMemberInfo {
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
        protected readonly string _ForcedName;
        protected readonly JSReadPolicy _ReadPolicy;
        protected readonly JSWritePolicy _WritePolicy;
        protected readonly JSInvokePolicy _InvokePolicy;

        public MemberInfo (TypeInfo parent, MemberIdentifier identifier, T member, ProxyInfo[] proxies, bool isIgnored = false, bool isExternal = false) {
            Identifier = identifier;

            _ReadPolicy = JSReadPolicy.Unmodified;
            _WritePolicy = JSWritePolicy.Unmodified;
            _InvokePolicy = JSInvokePolicy.Unmodified;

            _IsIgnored = isIgnored || TypeInfo.IsIgnoredName(member.Name);
            IsExternal = isExternal;
            DeclaringType = parent;

            Member = member;
            Metadata = new MetadataCollection(member);

            var ca = member.DeclaringType as ICustomAttributeProvider;
            if ((ca != null) && (ca.CustomAttributes.Any((p) => p.AttributeType.FullName == "JSIL.Proxy.JSProxy")))
                IsFromProxy = true;
            else
                IsFromProxy = false;

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
            return ForcedName ?? Member.Name;
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
            get { return _IsIgnored | DeclaringType.IsIgnored | DeclaringType.IsTypeIgnored(ReturnType); }
        }

        public string ForcedName {
            get {
                if (_ForcedName != null)
                    return _ForcedName;

                var parms = Metadata.GetAttributeParameters("JSIL.Meta.JSChangeName");
                if (parms != null)
                    return (string)parms[0].Value;

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

        public FieldInfo (TypeInfo parent, MemberIdentifier identifier, FieldDefinition field, ProxyInfo[] proxies) : base(
            parent, identifier, field, proxies, ILBlockTranslator.IsIgnoredType(field.FieldType)
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
        protected readonly string ShortName;

        public PropertyInfo (TypeInfo parent, MemberIdentifier identifier, PropertyDefinition property, ProxyInfo[] proxies) : base(
            parent, identifier, property, proxies, ILBlockTranslator.IsIgnoredType(property.PropertyType)
        ) {
            ShortName = GetShortName(property);
        }

        protected override string GetName () {
            string result;
            var declType = Member.DeclaringType.Resolve();
            var over = (Member.GetMethod ?? Member.SetMethod).Overrides.FirstOrDefault();

            if ((declType != null) && declType.IsInterface)
                result = ForcedName ?? String.Format("{0}.{1}", declType.Name, ShortName);
            else if (over != null)
                result = ForcedName ?? String.Format("{0}.{1}", over.DeclaringType.Name, ShortName);
            else
                result = ForcedName ?? ShortName;

            return result;
        }

        public override TypeReference ReturnType {
            get { return Member.PropertyType; }
        }

        public override bool IsStatic {
            get { return (Member.GetMethod ?? Member.SetMethod).IsStatic; }
        }
    }

    public class EventInfo : MemberInfo<EventDefinition> {
        public EventInfo (TypeInfo parent, MemberIdentifier identifier, EventDefinition evt, ProxyInfo[] proxies) : base(
            parent, identifier, evt, proxies, false
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
        public readonly PropertyInfo Property = null;
        public readonly EventInfo Event = null;

        public FunctionStaticData StaticData = null;

        public int? OverloadIndex;
        protected readonly string ShortName;

        public MethodInfo (TypeInfo parent, MemberIdentifier identifier, MethodDefinition method, ProxyInfo[] proxies) : base (
            parent, identifier, method, proxies,
            ILBlockTranslator.IsIgnoredType(method.ReturnType) || 
                method.Parameters.Any((p) => ILBlockTranslator.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl
        ) {
            ShortName = GetShortName(method);
            Parameters = method.Parameters.ToArray();
        }

        public MethodInfo (TypeInfo parent, MemberIdentifier identifier, MethodDefinition method, ProxyInfo[] proxies, PropertyInfo property) : base (
            parent, identifier, method, proxies,
            ILBlockTranslator.IsIgnoredType(method.ReturnType) || 
                method.Parameters.Any((p) => ILBlockTranslator.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl
        ) {
            Property = property;
            ShortName = GetShortName(method);
            Parameters = method.Parameters.ToArray();
        }

        public MethodInfo (TypeInfo parent, MemberIdentifier identifier, MethodDefinition method, ProxyInfo[] proxies, EventInfo evt) : base(
            parent, identifier, method, proxies,
            ILBlockTranslator.IsIgnoredType(method.ReturnType) ||
                method.Parameters.Any((p) => ILBlockTranslator.IsIgnoredType(p.ParameterType)),
            method.IsNative || method.IsUnmanaged || method.IsUnmanagedExport || method.IsInternalCall || method.IsPInvokeImpl
        ) {
            Event = evt;
            ShortName = GetShortName(method);
            Parameters = method.Parameters.ToArray();
        }

        protected override string GetName () {
            return GetName(null);
        }

        public string GetName (bool? nameMangling = null) {
            string result;
            var declType = Member.DeclaringType.Resolve();
            var over = Member.Overrides.FirstOrDefault();

            if ((declType != null) && declType.IsInterface)
                result = ForcedName ?? String.Format("{0}.{1}", declType.Name, ShortName);
            else if (over != null)
                result = ForcedName ?? String.Format("{0}.{1}", over.DeclaringType.Name, ShortName);
            else
                result = ForcedName ?? ShortName;

            if (OverloadIndex.HasValue) {
                if (nameMangling.GetValueOrDefault(!Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch")))
                    result = String.Format("{0}${1}", result, OverloadIndex.Value);
            }

            return result;
        }

        public override bool IsIgnored {
            get {
                if ((Event != null) && Event.IsIgnored)
                    return true;
                if ((Property != null) && Property.IsIgnored)
                    return true;

                return base.IsIgnored || Member.Parameters.Any(DeclaringType.IsTypeIgnored);
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
}
