using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public class TypeInfoProvider : ITypeInfoSource {
        public readonly HashSet<AssemblyDefinition> Assemblies = new HashSet<AssemblyDefinition>();
        public readonly Dictionary<TypeIdentifier, ProxyInfo> TypeProxies = new Dictionary<TypeIdentifier, ProxyInfo>();
        public readonly Dictionary<TypeIdentifier, TypeInfo> TypeInformation = new Dictionary<TypeIdentifier, TypeInfo>();
        public readonly Dictionary<string, ModuleInfo> ModuleInformation = new Dictionary<string, ModuleInfo>();

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
            foreach (var assembly in assemblies) {
                Assemblies.Remove(assembly);

                foreach (var module in assembly.Modules) {
                    ModuleInformation.Remove(module.FullyQualifiedName);

                    foreach (var type in module.Types) {
                        var identifier = new TypeIdentifier(type);

                        TypeProxies.Remove(identifier);
                        TypeInformation.Remove(identifier);

                        foreach (var nt in type.NestedTypes) {
                            var ni = new TypeIdentifier(nt);

                            TypeProxies.Remove(ni);
                            TypeInformation.Remove(ni);
                        }
                    }
                }
            }
        }

        public void AddProxyAssemblies (params AssemblyDefinition[] assemblies) {
            foreach (var asm in assemblies) {
                if (Assemblies.Contains(asm))
                    continue;
                else
                    Assemblies.Add(asm);

                foreach (var proxyType in ProxyTypesFromAssembly(asm))
                    TypeProxies.Add(new TypeIdentifier(proxyType), new ProxyInfo(proxyType));
            }
        }

        public ModuleInfo GetModuleInformation (ModuleDefinition module) {
            if (module == null)
                throw new ArgumentNullException("module");

            var fullName = module.FullyQualifiedName;

            ModuleInfo result;
            if (!ModuleInformation.TryGetValue(fullName, out result))
                ModuleInformation[fullName] = result = new ModuleInfo(module);

            return result;
        }

        public TypeInfo GetTypeInformation (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            // TODO: Enable this once it's fixed in ILSpy upstream
            /*
            if (type.DeclaringType != null)
                type = TypeAnalysis.SubstituteTypeArgs(type, type.DeclaringType);
             */

            var identifier = new TypeIdentifier(type);

            var fullName = type.FullName;

            var typesToInitialize = new Dictionary<TypeIdentifier, TypeDefinition>();
            var secondPass = new Dictionary<TypeIdentifier, TypeInfo>();

            TypeInfo result;
            if (!TypeInformation.TryGetValue(identifier, out result)) {
                var typedef = ILBlockTranslator.GetTypeDefinition(type);
                if (typedef == null)
                    return null;

                identifier = new TypeIdentifier(typedef);
                typesToInitialize.Add(identifier, typedef);
            }

            // We must construct type information in two passes, so that method group construction
            //  behaves correctly and ignores all the right methods.
            // The first pass walks all the way through the type graph (starting with the current type),
            //  ensuring we have type information for all the types in the graph. We do this iteratively
            //  to avoid overflowing the stack.
            // After we have type information for all the types in the graph, we then walk over all
            //  the types again, and construct their method groups, since we have the necessary
            //  information to determine which methods are ignored.
            while (typesToInitialize.Count > 0) {
                var kvp = typesToInitialize.First();
                typesToInitialize.Remove(kvp.Key);

                if (TypeInformation.ContainsKey(kvp.Key))
                    continue;
                else if (kvp.Value == null) {
                    TypeInformation[kvp.Key] = null;
                    continue;
                }

                var moreTypes = ConstructTypeInformation(kvp.Key, kvp.Value);

                TypeInfo temp;
                if (TypeInformation.TryGetValue(kvp.Key, out temp))
                    secondPass.Add(kvp.Key, temp);

                foreach (var more in moreTypes) {
                    if (typesToInitialize.ContainsKey(more.Key))
                        continue;
                    else if (TypeInformation.ContainsKey(more.Key))
                        continue;

                    typesToInitialize.Add(more.Key, more.Value);
                }
            }

            foreach (var ti in secondPass.Values) {
                ti.Initialize();
                ti.ConstructMethodGroups();
            }

            if (!TypeInformation.TryGetValue(identifier, out result)) {
                return null;
            }

            return result;
        }

        protected Dictionary<TypeIdentifier, TypeDefinition> ConstructTypeInformation (TypeIdentifier identifier, TypeDefinition type) {
            var moduleInfo = GetModuleInformation(type.Module);

            TypeInfo baseType = null;
            if (type.BaseType != null)
                baseType = GetTypeInformation(type.BaseType);

            foreach (var iface in type.Interfaces)
                GetTypeInformation(iface);

            var result = new TypeInfo(this, moduleInfo, type, baseType, identifier);
            TypeInformation[identifier] = result;

            var typesToInitialize = new Dictionary<TypeIdentifier, TypeDefinition>();
            Action<TypeReference> addType = (tr) => {
                if (tr == null)
                    return;

                var _identifier = new TypeIdentifier(tr);
                if (_identifier.Equals(identifier))
                    return;
                else if (TypeInformation.ContainsKey(_identifier))
                    return;
                else if (typesToInitialize.ContainsKey(_identifier))
                    return;

                var td = ILBlockTranslator.GetTypeDefinition(tr);
                if (td == null)
                    return;

                _identifier = new TypeIdentifier(td);
                if (typesToInitialize.ContainsKey(_identifier))
                    return;

                typesToInitialize.Add(_identifier, td);
            };

            foreach (var member in result.Members.Values) {
                addType(member.ReturnType);

                var method = member as Internal.MethodInfo;
                if (method != null) {
                    foreach (var p in method.Member.Parameters)
                        addType(p.ParameterType);
                }
            }

            return typesToInitialize;
        }

        ProxyInfo[] ITypeInfoSource.GetProxies (TypeReference type) {
            var result = new List<ProxyInfo>();

            foreach (var p in TypeProxies.Values) {
                if (p.IsMatch(type, null))
                    result.Add(p);
            }

            return result.Distinct().ToArray();
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

            // TODO: Enable this once it's fixed upstream
            /*
            if (type.DeclaringType != null)
                type = TypeAnalysis.SubstituteTypeArgs(type, type.DeclaringType);
             */

            var identifier = new TypeIdentifier(type);

            TypeInfo result;
            if (!TypeInformation.TryGetValue(identifier, out result))
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
}
