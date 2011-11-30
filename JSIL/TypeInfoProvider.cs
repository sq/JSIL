using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public class TypeInfoProvider : ITypeInfoSource {
        protected readonly HashSet<AssemblyDefinition> Assemblies = new HashSet<AssemblyDefinition>();
        protected readonly ConcurrentCache<TypeIdentifier, TypeInfo> TypeInformation;
        protected readonly ConcurrentCache<string, ModuleInfo> ModuleInformation;
        protected readonly Dictionary<TypeIdentifier, ProxyInfo> TypeProxies = new Dictionary<TypeIdentifier, ProxyInfo>();
        protected readonly Dictionary<string, HashSet<ProxyInfo>> DirectProxiesByTypeName = new Dictionary<string, HashSet<ProxyInfo>>();

        public TypeInfoProvider () {
            TypeInformation = new ConcurrentCache<TypeIdentifier, TypeInfo>(Environment.ProcessorCount, 1024);
            ModuleInformation = new ConcurrentCache<string, ModuleInfo>(Environment.ProcessorCount, 128);
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

        public void Remove (params AssemblyDefinition[] assemblies) {
            lock (Assemblies)
            foreach (var assembly in assemblies) {
                Assemblies.Remove(assembly);

                foreach (var module in assembly.Modules) {
                    ModuleInformation.TryRemove(module.FullyQualifiedName);

                    foreach (var type in module.Types) {
                        var identifier = new TypeIdentifier(type);

                        TypeProxies.Remove(identifier);
                        TypeInformation.TryRemove(identifier);

                        foreach (var nt in type.NestedTypes) {
                            var ni = new TypeIdentifier(nt);

                            TypeProxies.Remove(ni);
                            TypeInformation.TryRemove(ni);
                        }
                    }
                }
            }
        }

        public void AddProxyAssemblies (params AssemblyDefinition[] assemblies) {
            HashSet<ProxyInfo> pl;

            lock (Assemblies)
            foreach (var asm in assemblies) {
                if (Assemblies.Contains(asm))
                    continue;
                else
                    Assemblies.Add(asm);

                foreach (var proxyType in ProxyTypesFromAssembly(asm)) {
                    var proxyInfo = new ProxyInfo(proxyType);
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
                fullName, 
                () => new ModuleInfo(module)
            );
        }

        private void EnqueueType (OrderedDictionary<TypeIdentifier, TypeDefinition> queue, TypeReference typeReference, LinkedListNode<TypeIdentifier> before = null) {
            var identifier = new TypeIdentifier(typeReference);

            if (queue.ContainsKey(identifier))
                return;

            if (!TypeInformation.ContainsKey(identifier)) {
                var typedef = ILBlockTranslator.GetTypeDefinition(typeReference);
                if (typedef == null)
                    return;

                LinkedListNode<TypeIdentifier> before2;
                if (before != null)
                    before2 = queue.EnqueueBefore(before, identifier, typedef);
                else
                    before2 = queue.Enqueue(identifier, typedef);

                if (typedef.BaseType != null)
                    EnqueueType(queue, typedef.BaseType, before2);

                foreach (var iface in typedef.Interfaces)
                    EnqueueType(queue, iface, before2);
            }
        }

        public TypeInfo GetTypeInformation (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            TypeInfo result;
            var identifier = new TypeIdentifier(type);
            if (TypeInformation.TryGet(identifier, out result))
                return result;

            var fullName = type.FullName;

            var moreTypes = new Dictionary<TypeIdentifier, TypeDefinition>();
            var typesToInitialize = new OrderedDictionary<TypeIdentifier, TypeDefinition>();
            var secondPass = new Dictionary<TypeIdentifier, TypeInfo>();

            EnqueueType(typesToInitialize, type);

            // We must construct type information in two passes, so that method group construction
            //  behaves correctly and ignores all the right methods.
            // The first pass walks all the way through the type graph (starting with the current type),
            //  ensuring we have type information for all the types in the graph. We do this iteratively
            //  to avoid overflowing the stack.
            // After we have type information for all the types in the graph, we then walk over all
            //  the types again, and construct their method groups, since we have the necessary
            //  information to determine which methods are ignored.
            while (typesToInitialize.Count > 0) {
                var kvp = typesToInitialize.First;
                typesToInitialize.Remove(kvp.Key);

                TypeInformation.TryCreate(
                    kvp.Key, () => {
                        var constructed = ConstructTypeInformation(kvp.Key, kvp.Value, moreTypes);
                        secondPass.Add(kvp.Key, constructed);

                        foreach (var more in moreTypes)
                            EnqueueType(typesToInitialize, more.Value);
                        moreTypes.Clear();

                        return constructed;
                    }
                );
            }

            foreach (var ti in secondPass.Values) {
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

            TypeInfo baseType = null;
            if (type.BaseType != null)
                baseType = GetTypeInformation(type.BaseType);

            var result = new TypeInfo(this, moduleInfo, type, baseType, identifier);

            Action<TypeReference> addType = (tr) => {
                if (tr == null)
                    return;

                var _identifier = new TypeIdentifier(tr);
                if (_identifier.Equals(identifier))
                    return;
                else if (moreTypes.ContainsKey(_identifier))
                    return;

                var td = ILBlockTranslator.GetTypeDefinition(tr);
                if (td == null)
                    return;

                _identifier = new TypeIdentifier(td);

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

        ProxyInfo[] ITypeInfoSource.GetProxies (TypeDefinition type) {
            var result = new HashSet<ProxyInfo>();
            bool isInherited = false;

            while (type != null) {
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

            return result.ToArray();
        }

        IMemberInfo ITypeInfoSource.Get (MemberReference member) {
            var typeInfo = GetTypeInformation(member.DeclaringType);
            if (typeInfo == null) {
                Console.Error.WriteLine("Warning: type not loaded: {0}", member.DeclaringType.FullName);
                return null;
            }

            var identifier = MemberIdentifier.New(member);

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

        TypeInfo ITypeInfoSource.GetExisting (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            var identifier = new TypeIdentifier(type);

            TypeInfo result;
            if (!TypeInformation.TryGet(identifier, out result))
                return null;

            return result;
        }

        public T GetMemberInformation<T> (MemberReference member)
            where T : class, Internal.IMemberInfo {
            var typeInformation = GetTypeInformation(member.DeclaringType);
            var identifier = MemberIdentifier.New(member);

            IMemberInfo result;
            if (!typeInformation.Members.TryGetValue(identifier, out result)) {
                // Console.Error.WriteLine("Warning: member not defined: {0}", member.FullName);
                return null;
            }

            return (T)result;
        }
    }

    public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
        protected readonly Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();
        protected readonly LinkedList<TKey> LinkedList = new LinkedList<TKey>();

        public int Count {
            get {
                return Dictionary.Count;
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
            if (Dictionary.Remove(key)) {
                LinkedList.Remove(key);
                return true;
            }

            return false;
        }

        public LinkedListNode<TKey> EnqueueBefore (LinkedListNode<TKey> before, TKey key, TValue value) {
            Dictionary.Add(key, value);
            return LinkedList.AddBefore(before, key);
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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () {
            foreach (var key in LinkedList)
                yield return new KeyValuePair<TKey, TValue>(key, Dictionary[key]);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            foreach (var key in LinkedList)
                yield return new KeyValuePair<TKey, TValue>(key, Dictionary[key]);
        }
    }
}
