using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using JSIL.Internal;
using JSIL.Proxies;
using Mono.Cecil;

using TypeInfo = JSIL.Internal.TypeInfo;

namespace JSIL {
    public class TypeInfoProvider : ITypeInfoSource, IDisposable {
        protected class ProxiesByNameRecord {
            public readonly ConcurrentCache<HashedString, ArraySegment<string>> Cache =
                new ConcurrentCache<HashedString, ArraySegment<string>>(new HashedStringComparer());
            public volatile int Count;
        }

        protected class MakeTypeInfoArgs {
            public readonly Dictionary<TypeIdentifier, TypeDefinition> MoreTypes = new Dictionary<TypeIdentifier, TypeDefinition>(TypeIdentifier.Comparer);
            public readonly Dictionary<TypeIdentifier, TypeInfo> SecondPass = new Dictionary<TypeIdentifier, TypeInfo>(TypeIdentifier.Comparer);
            public readonly OrderedDictionary<TypeIdentifier, TypeDefinition> TypesToInitialize = new OrderedDictionary<TypeIdentifier, TypeDefinition>(TypeIdentifier.Comparer);

            public TypeDefinition Definition;
        }

        protected readonly HashSet<AssemblyDefinition> Assemblies;
        protected readonly HashSet<string> ProxyAssemblyNames;
        protected readonly ConcurrentCache<TypeIdentifier, TypeInfo> TypeInformation;
        protected readonly ConcurrentCache<string, ModuleInfo> ModuleInformation;
        protected readonly Dictionary<TypeIdentifier, ProxyInfo> TypeProxies;
        protected readonly Dictionary<string, HashSet<ProxyInfo>> DirectProxiesByTypeName;
        protected readonly ConcurrentCache<HashedString, ProxiesByNameRecord> ProxiesByName;
        protected readonly ConcurrentCache<Tuple<string, string>, bool> TypeAssignabilityCache;

        protected static readonly ConcurrentCache<string, ModuleInfo>.CreatorFunction<ModuleDefinition> MakeModuleInfo;
        protected static readonly ConcurrentCache<HashedString, ProxiesByNameRecord>.CreatorFunction<MemberReference> MakeProxiesByName;
        protected static readonly ConcurrentCache<HashedString, ArraySegment<string>>.CreatorFunction<MemberReference> MakeProxiesByFullName;
        protected static readonly Predicate<ArraySegment<string>> ShouldAddProxies; 
        protected readonly ConcurrentCache<TypeIdentifier, TypeInfo>.CreatorFunction<MakeTypeInfoArgs> MakeTypeInfo;

        static TypeInfoProvider () {
            MakeModuleInfo = (key, module) => new ModuleInfo(module);
            MakeProxiesByName = _MakeProxiesByName;
            MakeProxiesByFullName = _MakeProxiesByFullName;
            ShouldAddProxies = _ShouldAddProxies;
        }

        public TypeInfoProvider () {
            var levelOfParallelism = Math.Max(1, Environment.ProcessorCount / 2);

            Assemblies = new HashSet<AssemblyDefinition>();
            ProxyAssemblyNames = new HashSet<string>(StringComparer.Ordinal);
            TypeProxies = new Dictionary<TypeIdentifier, ProxyInfo>(TypeIdentifier.Comparer);
            DirectProxiesByTypeName = new Dictionary<string, HashSet<ProxyInfo>>(StringComparer.Ordinal);
            ProxiesByName = new ConcurrentCache<HashedString, ProxiesByNameRecord>(
                levelOfParallelism, 256, new HashedStringComparer()
            );
            TypeAssignabilityCache = new ConcurrentCache<Tuple<string, string>, bool>(levelOfParallelism, 4096);

            TypeInformation = new ConcurrentCache<TypeIdentifier, TypeInfo>(levelOfParallelism, 4096, TypeIdentifier.Comparer);
            ModuleInformation = new ConcurrentCache<string, ModuleInfo>(levelOfParallelism, 256, StringComparer.Ordinal);

            MakeTypeInfo = _MakeTypeInfo;
        }

        protected TypeInfoProvider (TypeInfoProvider cloneSource) {
            Assemblies = new HashSet<AssemblyDefinition>(cloneSource.Assemblies);
            ProxyAssemblyNames = new HashSet<string>(cloneSource.ProxyAssemblyNames);
            TypeProxies = new Dictionary<TypeIdentifier, ProxyInfo>(cloneSource.TypeProxies, TypeIdentifier.Comparer);

            DirectProxiesByTypeName = new Dictionary<string, HashSet<ProxyInfo>>();
            foreach (var kvp in cloneSource.DirectProxiesByTypeName)
                DirectProxiesByTypeName.Add(kvp.Key, new HashSet<ProxyInfo>(kvp.Value));

            ProxiesByName = cloneSource.ProxiesByName.Clone();
            TypeAssignabilityCache = cloneSource.TypeAssignabilityCache.Clone();
            TypeInformation = cloneSource.TypeInformation.Clone();
            ModuleInformation = cloneSource.ModuleInformation.Clone();

            MakeTypeInfo = _MakeTypeInfo;
        }

        public TypeInfoProvider Clone () {
            return new TypeInfoProvider(this);
        }

        private static bool _ShouldAddProxies (ArraySegment<string> proxies) {
            if (proxies.Array == null)
                return false;
            else if (proxies.Count == 0)
                return false;
            else
                return true;
        }

        private static ProxiesByNameRecord _MakeProxiesByName (HashedString key, MemberReference mr) {
            return new ProxiesByNameRecord();
        }

        private static ArraySegment<string> _MakeProxiesByFullName (HashedString key, MemberReference mr) {
            var icap = mr.DeclaringType as Mono.Cecil.ICustomAttributeProvider;
            if (icap == null)
                return ImmutableArrayPool<string>.Empty;

            CustomAttribute proxyAttribute = null;
            for (int i = 0, c = icap.CustomAttributes.Count; i < c; i++) {
                var ca = icap.CustomAttributes[i];
                if ((ca.AttributeType.Name == "JSProxy") && (ca.AttributeType.Namespace == "JSIL.Proxy")) {
                    proxyAttribute = ca;
                    break;
                }
            }

            if (proxyAttribute == null)
                return ImmutableArrayPool<string>.Empty;

            ArraySegment<string> proxyTargets = ImmutableArrayPool<string>.Empty;
            var args = proxyAttribute.ConstructorArguments;

            foreach (var arg in args) {
                switch (arg.Type.FullName) {
                    case "System.Type": {
                            proxyTargets = ImmutableArrayPool<string>.Elements(
                                ((TypeReference)arg.Value).FullName
                            );

                            break;
                        }
                    case "System.Type[]": {
                            var values = (CustomAttributeArgument[])arg.Value;
                            proxyTargets = (from v in values select ((TypeReference)v.Value).FullName)
                                .ToImmutableArray(values.Length);

                            break;
                        }
                    case "System.String": {
                            proxyTargets = ImmutableArrayPool<string>.Elements(
                                (string)arg.Value
                            );

                            break;
                        }
                    case "System.String[]": {
                            var values = (CustomAttributeArgument[])arg.Value;
                            proxyTargets = (from v in values select (string)v.Value)
                                .ToImmutableArray(values.Length);

                            break;
                        }
                }
            }

            return proxyTargets;
        }

        private TypeInfo _MakeTypeInfo(TypeIdentifier identifier, MakeTypeInfoArgs args) {
            var constructed = ConstructTypeInformation(identifier, args.Definition, args.MoreTypes);
            args.SecondPass.Add(identifier, constructed);

            foreach (var typedef in args.MoreTypes.Values)
                EnqueueType(args.TypesToInitialize, typedef);

            args.MoreTypes.Clear();

            return constructed;
        }

        ConcurrentCache<Tuple<string, string>, bool> ITypeInfoSource.AssignabilityCache {
            get {
                return this.TypeAssignabilityCache;
            }
        }

        public int Count {
            get {
                return TypeInformation.Count + ModuleInformation.Count;
            }
        }

        public void DumpSignatureCollectionStats () {
            int minSize = int.MaxValue, maxSize = int.MinValue;
            int sum = 0, count = 0;

            foreach (var kvp in TypeInformation) {
                var signatures = kvp.Value.MethodSignatures;

                minSize = Math.Min(minSize, signatures.Counts.Count);
                maxSize = Math.Max(maxSize, signatures.Counts.Count);
                sum += signatures.Counts.Count;
                count += 1;
            }

            Console.WriteLine("// method signature collection stats:");
            Console.WriteLine(
                "// total: {0:D6} min: {1:D4} max: {2:D4} average: {3:D4}",
                sum, minSize, maxSize,
                (int)Math.Floor((double)sum / count)
            );
        }

        public void ClearCaches () {
            TypeAssignabilityCache.Clear();
        }

        public void Dispose () {
            Assemblies.Clear();
            ProxyAssemblyNames.Clear();
            TypeInformation.Dispose();
            ModuleInformation.Dispose();
            TypeProxies.Clear();
            DirectProxiesByTypeName.Clear();
            ProxiesByName.Dispose();
            TypeAssignabilityCache.Clear();
        }

        bool ITypeInfoSource.TryGetProxyNames (TypeReference tr, out ArraySegment<string> result) {
            result = ImmutableArrayPool<string>.Empty;

            ProxiesByNameRecord proxiesByFullName;
            var name = new HashedString(tr.Name);
            if (!ProxiesByName.TryGet(name, out proxiesByFullName))
                return false;

            if (proxiesByFullName.Count == 0)
                return false;

            var fullName = new HashedString(tr.FullName);
            return proxiesByFullName.Cache.TryGet(fullName, out result);
        }

        void ITypeInfoSource.CacheProxyNames (MemberReference mr) {
            var name = new HashedString(mr.DeclaringType.Name);
            var proxiesByFullName = ProxiesByName.GetOrCreate(
                name, mr, MakeProxiesByName
            );

            var fullName = new HashedString(mr.DeclaringType.FullName);
            if (proxiesByFullName.Cache.TryCreate(
                fullName, mr,
                MakeProxiesByFullName, ShouldAddProxies
            ))
                Interlocked.Increment(ref proxiesByFullName.Count);
        }

        protected IEnumerable<TypeDefinition> ProxyTypesFromAssembly (AssemblyDefinition assembly) {
            foreach (var module in assembly.Modules) {
                foreach (var type in module.Types) {
                    foreach (var ca in type.CustomAttributes) {
                        if (ca.AttributeType.FullName == "JSIL.Proxy.JSProxy") {
                            yield return type;
                            break;
                        }
                    }
                }
            }
        }

        private void Remove (TypeDefinition type) {
            var identifier = new TypeIdentifier(type);
            var typeName = type.FullName;
            var hashedTypeName = new HashedString(typeName);

            TypeProxies.Remove(identifier);
            TypeInformation.TryRemove(identifier);
            DirectProxiesByTypeName.Remove(typeName);
            ProxiesByName.TryRemove(hashedTypeName);

            foreach (var nt in type.NestedTypes)
                Remove(nt);
        }

        public void Remove (params AssemblyDefinition[] assemblies) {
            lock (Assemblies)
            foreach (var assembly in assemblies) {
                Assemblies.Remove(assembly);
                ProxyAssemblyNames.Remove(assembly.FullName);

                foreach (var module in assembly.Modules) {
                    ModuleInformation.TryRemove(module.FullyQualifiedName);

                    foreach (var type in module.Types) {
                        Remove(type);
                    }
                }
            }
        }

        public void AddProxyAssemblies (Action<AssemblyDefinition> onProxiesFound, params AssemblyDefinition[] assemblies) {
            HashSet<ProxyInfo> pl;

            lock (Assemblies)
            foreach (var asm in assemblies) {
                if (ProxyAssemblyNames.Contains(asm.FullName))
                    continue;
                else
                    ProxyAssemblyNames.Add(asm.FullName);

                if (Assemblies.Contains(asm))
                    continue;
                else
                    Assemblies.Add(asm);

                bool foundAProxy = false;

                foreach (var proxyType in ProxyTypesFromAssembly(asm)) {
                    if (!foundAProxy) {
                        foundAProxy = true;
                        onProxiesFound(asm);
                    }

                    var proxyInfo = new ProxyInfo(this, proxyType);
                    TypeProxies.Add(new TypeIdentifier(proxyType), proxyInfo);

                    foreach (var typeref in proxyInfo.ProxiedTypes) {
                        var name = typeref.FullName;

                        if (!DirectProxiesByTypeName.TryGetValue(name, out pl))
                            DirectProxiesByTypeName[name] = pl = new HashSet<ProxyInfo>();

                        pl.Add(proxyInfo);
                    }

                    foreach (var name in proxyInfo.ProxiedTypeNames) {
                        if (!DirectProxiesByTypeName.TryGetValue(name, out pl))
                            DirectProxiesByTypeName[name] = pl = new HashSet<ProxyInfo>();

                        pl.Add(proxyInfo);
                    }
                }
            }
        }

        public ModuleInfo GetModuleInformation (ModuleDefinition module) {
            if (module == null)
                throw new ArgumentNullException("module");

            var fullName = module.FullyQualifiedName;
            return ModuleInformation.GetOrCreate(
                fullName, module, MakeModuleInfo
            );
        }

        private void EnqueueType (OrderedDictionary<TypeIdentifier, TypeDefinition> queue, TypeReference typeReference, LinkedListNode<TypeIdentifier> before = null) {
            var typedef = TypeUtil.GetTypeDefinition(typeReference);
            if (typedef == null)
                return;

            var identifier = new TypeIdentifier(typedef);

            if (queue.ContainsKey(identifier))
                return;

            if (!TypeInformation.ContainsKey(identifier)) {
                LinkedListNode<TypeIdentifier> before2;
                if (before != null)
                    before2 = queue.EnqueueBefore(before, identifier, typedef);
                else
                    before2 = queue.Enqueue(identifier, typedef);

                if (typedef.BaseType != null)
                    EnqueueType(queue, typedef.BaseType, before2);

                if (typedef.DeclaringType != null)
                    EnqueueType(queue, typedef.DeclaringType, before2);

                foreach (var iface in typedef.Interfaces)
                    EnqueueType(queue, iface, before2);
            }
        }

        public TypeInfo GetTypeInformation (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            TypeInfo result;
            var typedef = TypeUtil.GetTypeDefinition(type);
            if (typedef == null)
                return null;

            var identifier = new TypeIdentifier(typedef);
            if (TypeInformation.TryGet(identifier, out result))
                return result;

            var args = new MakeTypeInfoArgs();

            EnqueueType(args.TypesToInitialize, type);

            // We must construct type information in two passes, so that method group construction
            //  behaves correctly and ignores all the right methods.
            // The first pass walks all the way through the type graph (starting with the current type),
            //  ensuring we have type information for all the types in the graph. We do this iteratively
            //  to avoid overflowing the stack.
            // After we have type information for all the types in the graph, we then walk over all
            //  the types again, and construct their method groups, since we have the necessary
            //  information to determine which methods are ignored.
            while (args.TypesToInitialize.Count > 0) {
                var kvp = args.TypesToInitialize.First;
                args.TypesToInitialize.Remove(kvp.Key);

                args.Definition = kvp.Value;
                TypeInformation.TryCreate(
                    kvp.Key, args, MakeTypeInfo
                );
            }

            foreach (var ti in args.SecondPass.Values) {
                ti.Initialize();
                ti.ConstructMethodGroups();
            }

            if (!TypeInformation.TryGet(identifier, out result))
                return null;
            else
                return result;
        }

        protected TypeInfo ConstructTypeInformation (TypeIdentifier identifier, TypeDefinition type, Dictionary<TypeIdentifier, TypeDefinition> moreTypes) {
            var moduleInfo = GetModuleInformation(type.Module);

            TypeInfo baseType = null, declaringType = null;
            if (type.BaseType != null) {
                baseType = GetExisting(type.BaseType);
                if (baseType == null)
                    throw new InvalidOperationException(String.Format(
                        "Missing type info for base type '{0}' of type '{1}'",
                        type.BaseType, type
                    ));
            }

            if (type.DeclaringType != null) {
                declaringType = GetExisting(type.DeclaringType);
                if (declaringType == null)
                    throw new InvalidOperationException(String.Format(
                        "Missing type info for declaring type '{0}' of type '{1}'",
                        type.DeclaringType, type
                    ));
            }

            var result = new TypeInfo(this, moduleInfo, type, declaringType, baseType, identifier);

            Action<TypeReference> addType = (tr) => {
                if (tr == null)
                    return;

                var td = TypeUtil.GetTypeDefinition(tr);
                if (td == null)
                    return;

                var _identifier = new TypeIdentifier(td);
                if (_identifier.Equals(identifier))
                    return;
                else if (moreTypes.ContainsKey(_identifier))
                    return;

                moreTypes[_identifier] = td;
            };

            foreach (var member in result.Members.Values) {
                addType(member.ReturnType);

                var method = member as Internal.MethodInfo;
                if (method != null) {
                    foreach (var p in method.Member.Parameters)
                        addType(p.ParameterType);
                }
            }

            return result;
        }

        ArraySegment<ProxyInfo> ITypeInfoSource.GetProxies (TypeDefinition type) {
            var result = new HashSet<ProxyInfo>();
            bool isInherited = false;
            bool isInterface = type.IsInterface;

            while (type != null) {
                // Never inherit proxy members from System.Object when processing an interface.
                if (isInterface && type.FullName == "System.Object")
                    break;

                HashSet<ProxyInfo> candidates;
                bool found;

                lock (Assemblies)
                    found = DirectProxiesByTypeName.TryGetValue(type.FullName, out candidates);

                if (found) {
                    foreach (var candidate in candidates) {
                        if (isInherited && !candidate.IsInheritable)
                            continue;

                        if (candidate.IsMatch(type, null))
                            result.Add(candidate);
                    }
                }

                if (type.BaseType != null)
                    type = type.BaseType.Resolve();
                else
                    break;

                isInherited = true;
            }

            return result.ToImmutableArray(result.Count);
        }

        IMemberInfo ITypeInfoSource.Get (MemberReference member) {
            var typeInfo = GetTypeInformation(member.DeclaringType);
            if (typeInfo == null) {
                Console.Error.WriteLine("Warning: type not loaded: {0}", member.DeclaringType.FullName);
                return null;
            }

            var identifier = MemberIdentifier.New(this, member);

            IMemberInfo result;
            if (!typeInfo.Members.TryGetValue(identifier, out result)) {
                // Console.Error.WriteLine("Warning: member not defined: {0}", member.FullName);
                return null;
            }

            return result;
        }

        ModuleInfo ITypeInfoSource.Get (ModuleDefinition module) {
            return GetModuleInformation(module);
        }

        TypeInfo ITypeInfoSource.Get (TypeReference type) {
            return GetTypeInformation(type);
        }

        public TypeInfo GetExisting (TypeIdentifier identifier) {
            TypeInfo result;
            if (!TypeInformation.TryGet(identifier, out result))
                return null;

            return result;
        }

        public TypeInfo GetExisting (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            var resolved = type.Resolve();
            if (resolved == null)
                return null;

            return GetExisting(resolved);
        }

        public TypeInfo GetExisting (TypeDefinition type) {
            if (type == null)
                throw new ArgumentNullException("type");

            var identifier = new TypeIdentifier(type);

            return GetExisting(identifier);
        }

        public T GetMemberInformation<T> (MemberReference member)
            where T : class, Internal.IMemberInfo {
            var typeInformation = GetTypeInformation(member.DeclaringType);
            var identifier = MemberIdentifier.New(this, member);

            IMemberInfo result;
            if (!typeInformation.Members.TryGetValue(identifier, out result)) {
                // Console.Error.WriteLine("Warning: member not defined: {0}", member.FullName);
                return null;
            }

            return (T)result;
        }
    }

    public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
        protected readonly Dictionary<TKey, TValue> Dictionary;
        protected readonly LinkedList<TKey> LinkedList = new LinkedList<TKey>();

        public OrderedDictionary () {
            Dictionary = new Dictionary<TKey, TValue>();
        }

        public OrderedDictionary (IEqualityComparer<TKey> comparer) {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public int Count {
            get {
                return Dictionary.Count;
            }
        }

        public TValue this [TKey key] { 
            get { 
                return Dictionary[key];
            }
        }

        public void Clear () {
            Dictionary.Clear();
            LinkedList.Clear();
        }

        public bool ContainsKey (TKey key) {
            return Dictionary.ContainsKey(key);
        }

        public bool Remove (TKey key) {
            // Is this right? Maybe enqueueing multiple times shouldn't work.
            while (LinkedList.Remove(key))
                ;

            return Dictionary.Remove(key);
        }

        public bool TryDequeueFirst (out TKey key, out TValue value) {
            if (LinkedList.Count == 0) {
                key = default(TKey);
                value = default(TValue);
                return false;
            }

            var node = LinkedList.First;
            key = node.Value;
            LinkedList.Remove(node);

            value = Dictionary[node.Value];
            Dictionary.Remove(node.Value);

            return true;
        }

        public LinkedListNode<TKey> EnqueueBefore (LinkedListNode<TKey> before, TKey key, TValue value) {
            Dictionary.Add(key, value);
            return LinkedList.AddBefore(before, key);
        }

        public LinkedListNode<TKey> EnqueueAfter (LinkedListNode<TKey> after, TKey key, TValue value) {
            Dictionary.Add(key, value);
            return LinkedList.AddAfter(after, key);
        }

        public LinkedListNode<TKey> Enqueue (TKey key, TValue value) {
            Dictionary.Add(key, value);
            return LinkedList.AddLast(key);
        }

        public KeyValuePair<TKey, TValue> First {
            get {
                var node = LinkedList.First;
                return new KeyValuePair<TKey, TValue>(node.Value, Dictionary[node.Value]);
            }
        }

        public KeyValuePair<TKey, TValue> Last {
            get {
                var node = LinkedList.Last;
                return new KeyValuePair<TKey, TValue>(node.Value, Dictionary[node.Value]);
            }
        }

        public IEnumerable<LinkedListNode<TKey>> Keys {
            get {
                var current = LinkedList.First;

                while (current != null) {
                    yield return current;

                    current = current.Next;
                }
            }
        }

        public KeyValuePair<TKey, TValue> AtIndex (int index) {
            var node = LinkedList.First;

            while (node != null) {
                if (index > 0) {
                    index -= 1;
                    node = node.Next;
                } else {
                    return new KeyValuePair<TKey, TValue>(node.Value, Dictionary[node.Value]);
                }
            }

            throw new ArgumentOutOfRangeException("index");
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () {
            foreach (var key in LinkedList)
                yield return new KeyValuePair<TKey, TValue>(key, Dictionary[key]);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            foreach (var key in LinkedList)
                yield return new KeyValuePair<TKey, TValue>(key, Dictionary[key]);
        }

        public bool Replace (TKey key, TValue newValue) {
            if (ContainsKey(key)) {
                Dictionary[key] = newValue;
                return true;
            }

            return false;
        }

        private LinkedListNode<TKey> MoveToHeadOrTail (TKey key, bool tail) {
            var node = LinkedList.Find(key);
            if (node == null)
                throw new KeyNotFoundException(key.ToString());

            LinkedList.Remove(node);

            if (tail)
                LinkedList.AddLast(node);
            else
                LinkedList.AddFirst(node);

            return node;
        }

        public LinkedListNode<TKey> MoveToHead (TKey key) {
            return MoveToHeadOrTail(key, false);
        }

        public LinkedListNode<TKey> MoveToTail (TKey key) {
            return MoveToHeadOrTail(key, true);
        }

        public LinkedListNode<TKey> FindNode (TKey key) {
            return LinkedList.Find(key);
        }
    }
}
