using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;
using ICSharpCode.Decompiler;
using Mono.Cecil.Pdb;

namespace JSIL {
    public class AssemblyResolver : BaseAssemblyResolver {
        public readonly Dictionary<string, AssemblyDefinition> Cache = new Dictionary<string, AssemblyDefinition>();

        public AssemblyResolver (IEnumerable<string> dirs) {
            foreach (var dir in dirs)
                AddSearchDirectory(dir);
        }

        public override AssemblyDefinition Resolve (AssemblyNameReference name) {
            if (name == null)
                throw new ArgumentNullException("name");

            AssemblyDefinition assembly;
            if (Cache.TryGetValue(name.FullName, out assembly))
                return assembly;

            assembly = base.Resolve(name);
            Cache[name.FullName] = assembly;

            return assembly;
        }

        protected void RegisterAssembly (AssemblyDefinition assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var name = assembly.Name.FullName;
            if (Cache.ContainsKey(name))
                return;

            Cache[name] = assembly;
        }
    }

    public class AssemblyTranslator : ITypeInfoSource {
        public const int LargeMethodThreshold = 1024;

        public readonly Dictionary<TypeIdentifier, ProxyInfo> TypeProxies = new Dictionary<TypeIdentifier, ProxyInfo>();
        public readonly Dictionary<TypeIdentifier, TypeInfo> TypeInformation = new Dictionary<TypeIdentifier, TypeInfo>();
        public readonly Dictionary<string, ModuleInfo> ModuleInformation = new Dictionary<string, ModuleInfo>();
        public readonly HashSet<string> GeneratedFiles = new HashSet<string>();
        public readonly List<Regex> IgnoredAssemblies = new List<Regex>();
        public readonly List<Regex> StubbedAssemblies = new List<Regex>();
        public readonly HashSet<string> DeclaredTypes = new HashSet<string>();

        public event Action<string> StartedLoadingAssembly;
        public event Action<string, bool> StartedDecompilingAssembly;

        public event Action<string> StartedDecompilingMethod;
        public event Action<string> FinishedDecompilingMethod;

        public event Action<string, Exception> CouldNotLoadSymbols;
        public event Action<string, Exception> CouldNotResolveAssembly;
        public event Action<string, Exception> CouldNotDecompileMethod;

        public string OutputDirectory = Environment.CurrentDirectory;

        public bool EliminateTemporaries = true;
        public bool SimplifyOperators = true;
        public bool IncludeDependencies = true;
        public bool UseSymbols = true;

        protected JavascriptAstEmitter AstEmitter;

        public AssemblyTranslator () {
            AddProxyAssembly(typeof(JSIL.Proxies.ObjectProxy).Assembly, false);
        }

        protected static ReaderParameters GetReaderParameters (bool useSymbols, string mainAssemblyPath = null) {
            var readerParameters = new ReaderParameters {
                ReadingMode = ReadingMode.Deferred,
                ReadSymbols = useSymbols
            };

            if (mainAssemblyPath != null) {
                readerParameters.AssemblyResolver = new AssemblyResolver(new string[] { 
                    Path.GetDirectoryName(mainAssemblyPath) 
                });
            }

            if (useSymbols)
                readerParameters.SymbolReaderProvider = new PdbReaderProvider();

            return readerParameters;
        }

        public void AddProxyAssemblies (AssemblyDefinition[] assemblies) {
            foreach (var asm in assemblies) {
                foreach (var module in asm.Modules) {
                    foreach (var type in module.Types) {
                        bool isProxyType = false;

                        foreach (var ca in type.CustomAttributes) {
                            if (ca.AttributeType.FullName == "JSIL.Proxy.JSProxy") {
                                isProxyType = true;
                                break;
                            }
                        }

                        if (isProxyType) {
                            var identifier = new TypeIdentifier(type);
                            TypeProxies.Add(identifier, new ProxyInfo(type));
                        }
                    }
                }
            }
        }

        public void AddProxyAssembly (string path, bool includeDependencies) {
            var assemblies = LoadAssembly(path, UseSymbols, includeDependencies);

            AddProxyAssemblies(assemblies);
        }

        public void AddProxyAssembly (Assembly assembly, bool includeDependencies) {
            var path = new Uri(assembly.CodeBase).AbsolutePath.Replace("/", "\\");
            AddProxyAssembly(path, includeDependencies);
        }

        public AssemblyDefinition[] LoadAssembly (string path) {
            return LoadAssembly(path, UseSymbols, IncludeDependencies);
        }

        protected AssemblyDefinition[] LoadAssembly (string path, bool useSymbols, bool includeDependencies) {
            var readerParameters = GetReaderParameters(useSymbols, path);

            if (StartedLoadingAssembly != null)
                StartedLoadingAssembly(path);

            var assembly = AssemblyDefinition.ReadAssembly(
                path, readerParameters
            );

            var result = new List<AssemblyDefinition>();
            result.Add(assembly);

            if (includeDependencies) {
                var modulesToVisit = new Queue<ModuleDefinition>(assembly.Modules);
                var visitedModules = new HashSet<string>();

                var assemblyNames = new HashSet<string>();
                while (modulesToVisit.Count > 0) {
                    var module = modulesToVisit.Dequeue();
                    if (visitedModules.Contains(module.FullyQualifiedName))
                        continue;

                    visitedModules.Add(module.FullyQualifiedName);

                    foreach (var reference in module.AssemblyReferences) {
                        bool ignored = false;
                        foreach (var ia in IgnoredAssemblies) {
                            if (ia.IsMatch(reference.FullName)) {
                                ignored = true;
                                break;
                            }
                        }

                        if (ignored)
                            continue;
                        if (assemblyNames.Contains(reference.FullName))
                            continue;

                        var childParameters = new ReaderParameters {
                            ReadingMode = ReadingMode.Deferred,
                            ReadSymbols = true,
                            SymbolReaderProvider = new PdbReaderProvider()
                        };

                        if (StartedLoadingAssembly != null)
                            StartedLoadingAssembly(reference.FullName);

                        AssemblyDefinition refAssembly = null;
                        assemblyNames.Add(reference.FullName);
                        try {
                            refAssembly = readerParameters.AssemblyResolver.Resolve(reference, readerParameters);
                        } catch (Exception ex) {
                            if (useSymbols) {
                                try {
                                    refAssembly = readerParameters.AssemblyResolver.Resolve(reference, GetReaderParameters(false, path));
                                    if (CouldNotLoadSymbols != null)
                                        CouldNotLoadSymbols(reference.FullName, ex);
                                } catch (Exception ex2) {
                                    if (CouldNotResolveAssembly != null)
                                        CouldNotResolveAssembly(reference.FullName, ex2);
                                }
                            } else {
                                if (CouldNotResolveAssembly != null)
                                    CouldNotResolveAssembly(reference.FullName, ex);
                            }
                        }

                        if (refAssembly != null) {
                            result.Add(refAssembly);
                            foreach (var refModule in refAssembly.Modules)
                                modulesToVisit.Enqueue(refModule);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public void Translate (string assemblyPath, Stream outputStream = null) {
            if (GeneratedFiles.Contains(assemblyPath))
                return;

            var assemblies = LoadAssembly(assemblyPath);

            AddProxyAssemblies(assemblies);

            GeneratedFiles.Add(assemblyPath);

            if (outputStream == null) {
                if (!Directory.Exists(OutputDirectory))
                    Directory.CreateDirectory(OutputDirectory);

                foreach (var assembly in assemblies) {
                    var outputPath = Path.Combine(OutputDirectory, assembly.Name + ".js");

                    if (File.Exists(outputPath))
                        File.Delete(outputPath);

                    using (outputStream = File.OpenWrite(outputPath))
                        Translate(assembly, outputStream);
                }
            } else {
                foreach (var assembly in assemblies)
                    Translate(assembly, outputStream);
            }
        }

        internal void Translate (AssemblyDefinition assembly, Stream outputStream) {
            var context = new DecompilerContext(assembly.MainModule);

            context.Settings.YieldReturn = false;
            context.Settings.AnonymousMethods = true;
            context.Settings.QueryExpressions = false;
            context.Settings.LockStatement = false;
            context.Settings.FullyQualifyAmbiguousTypeNames = true;
            context.Settings.ForEachStatement = false;

            bool stubbed = false;
            foreach (var sa in StubbedAssemblies) {
                if (sa.IsMatch(assembly.FullName)) {
                    stubbed = true;
                    break;
                }
            }

            if (StartedDecompilingAssembly != null)
                StartedDecompilingAssembly(assembly.MainModule.FullyQualifiedName, stubbed);

            var initializer = new List<Action>();
            var tw = new StreamWriter(outputStream, Encoding.ASCII);
            var formatter = new JavascriptFormatter(tw, this, assembly);

            if (stubbed) {
                formatter.Comment("Generating type stubs only");
                formatter.NewLine();
            }

            formatter.DeclareAssembly();

            // Important to clear this because types with the exact same full names can be defined in multiple assemblies
            DeclaredTypes.Clear();
            foreach (var module in assembly.Modules)
                TranslateModule(context, formatter, module, initializer, stubbed);

            foreach (var init in initializer) {
                formatter.Identifier("JSIL.QueueInitializer", null);
                formatter.LPar();
                formatter.OpenFunction(null, null);

                init();

                formatter.CloseBrace(false);
                formatter.RPar();
                formatter.Semicolon();
            }

            tw.Flush();
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

            foreach (var ti in secondPass.Values)
                ti.ConstructMethodGroups();

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

            var result = new TypeInfo(this, moduleInfo, type, baseType);
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
                Console.Error.WriteLine("Warning: member not defined: {0}", member.FullName);
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

        protected T GetMemberInformation<T> (MemberReference member)
            where T : class, Internal.IMemberInfo
        {
            var typeInformation = GetTypeInformation(member.DeclaringType);
            var identifier = MemberIdentifier.New(member);

            IMemberInfo result;
            if (!typeInformation.Members.TryGetValue(identifier, out result)) {
                Console.Error.WriteLine("Warning: member not defined: {0}", member.FullName);
                return null;
            }

            return (T)result;
        }

        protected void TranslateModule (DecompilerContext context, JavascriptFormatter output, ModuleDefinition module, List<Action> initializer, bool stubbed) {
            var moduleInfo = GetModuleInformation(module);
            if (moduleInfo.IsIgnored)
                return;

            context.CurrentModule = module;

            // Probably should be an argument, not a member variable...
            AstEmitter = new JavascriptAstEmitter(output, new JSILIdentifier(context.CurrentModule.TypeSystem), context.CurrentModule.TypeSystem, this);

            foreach (var typedef in module.Types)
                ForwardDeclareType(context, output, typedef);

            var sealedTypes = new HashSet<TypeDefinition>();

            foreach (var typedef in module.Types) {
                TranslateTypeDefinition(context, output, typedef, initializer, stubbed);
                SealType(context, output, typedef, sealedTypes);
            }

            if (sealedTypes.Count > 0) {
                var groups = (from st in sealedTypes
                                let parent = JavascriptFormatter.GetParent(st, null)
                                group st by parent);

                foreach (var g in groups) {
                    var _ = g;
                    initializer.Add(() => {
                        output.Identifier("JSIL.SealTypes", null);
                        output.LPar();

                        output.Identifier(output.PrivateToken);
                        output.Comma();
                        output.Value(_.Key);
                        output.Comma();

                        output.NewLine();

                        output.CommaSeparatedList(
                            (from typedef in _ select typedef.Name), ListValueType.Primitive
                        );

                        output.NewLine();
                        output.RPar();
                        output.Semicolon();
                    });
                }
            }
        }

        protected void TranslateInterface (DecompilerContext context, JavascriptFormatter output, TypeDefinition iface) {
            output.Identifier("JSIL.MakeInterface", null);
            output.LPar();
            output.NewLine();

            output.Value(Util.EscapeIdentifier(iface.FullName, EscapingMode.String));
            output.Comma();

            output.OpenBrace();

            bool isFirst = true;
            foreach (var m in iface.Methods) {
                var methodInfo = this.GetMethod(m);
                if ((methodInfo != null) && methodInfo.IsIgnored)
                    continue;

                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Value(Util.EscapeIdentifier(m.Name));
                output.Token(": ");
                output.Identifier("Function");

                isFirst = false;
            }

            foreach (var p in iface.Properties) {
                var propertyInfo = this.GetProperty(p);
                if ((propertyInfo != null) && propertyInfo.IsIgnored)
                    continue;

                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Value(Util.EscapeIdentifier(p.Name));
                output.Token(": ");
                output.Identifier("Property");

                isFirst = false;
            }

            output.NewLine();
            output.CloseBrace(false);

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void TranslateEnum (DecompilerContext context, JavascriptFormatter output, TypeDefinition enm) {
            output.Identifier("JSIL.MakeEnum", null);
            output.LPar();
            output.NewLine();

            output.Value(Util.EscapeIdentifier(enm.FullName, EscapingMode.String));
            output.Comma();
            output.OpenBrace();

            var typeInformation = GetTypeInformation(enm);
            if (typeInformation == null)
                throw new InvalidOperationException();

            bool isFirst = true;
            foreach (var em in typeInformation.EnumMembers.Values) {
                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Identifier(em.Name);
                output.Token(": ");
                output.Value(em.Value);

                isFirst = false;
            }

            output.NewLine();
            output.CloseBrace(false);
            output.Comma();
            output.Value(typeInformation.IsFlagsEnum);
            output.NewLine();

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void TranslateDelegate (DecompilerContext context, JavascriptFormatter output, TypeDefinition del, TypeInfo typeInfo) {
            output.Identifier("JSIL.MakeDelegate", null);
            output.LPar();

            output.Value(Util.EscapeIdentifier(del.FullName, EscapingMode.String));

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void ForwardDeclareType (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef) {
            var typeInfo = GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return;

            if (DeclaredTypes.Contains(typedef.FullName)) {
                Debug.WriteLine("Cycle in type references detected: {0}", typedef);
                return;
            }

            context.CurrentType = typedef;

            output.DeclareNamespace(typedef.Namespace);

            DeclaredTypes.Add(typedef.FullName);

            if (typedef.IsInterface) {
                TranslateInterface(context, output, typedef);
                return;
            } else if (typedef.IsEnum) {
                TranslateEnum(context, output, typedef);
                return;
            } else if (typeInfo.IsDelegate) {
                TranslateDelegate(context, output, typedef, typeInfo);
                return;
            }

            var declaringType = typedef.DeclaringType;
            if (declaringType != null) {
                if (!DeclaredTypes.Contains(declaringType.FullName))
                    ForwardDeclareType(context, output, declaringType);
            }

            var baseClass = typedef.Module.TypeSystem.Object;
            if (typedef.BaseType != null) {
                baseClass = typedef.BaseType;

                var resolved = baseClass.Resolve();
                if (
                    (resolved != null) &&
                    !DeclaredTypes.Contains(resolved.FullName) &&
                    (resolved.Module.Assembly == typedef.Module.Assembly)
                ) {
                    ForwardDeclareType(context, output, resolved);
                }
            }

            bool isStatic = typedef.IsAbstract && typedef.IsSealed;

            if (isStatic) {
                output.Identifier("JSIL.MakeStaticClass", null);
                output.LPar();

                output.Value(Util.EscapeIdentifier(typedef.FullName, EscapingMode.String));
                output.Comma();
                output.Value(typedef.IsPublic);

                output.RPar();
                output.Semicolon();
            } else {
                if (typedef.IsValueType)
                    output.Identifier("JSIL.MakeStruct", null);
                else
                    output.Identifier("JSIL.MakeClass", null);

                output.LPar();
                if (!typedef.IsValueType) {
                    output.TypeReference(baseClass);
                    output.Comma();
                }

                output.Value(Util.EscapeIdentifier(typedef.FullName, EscapingMode.String));
                output.Comma();
                output.Value(typedef.IsPublic);

                output.RPar();
                output.Semicolon();
            }

            foreach (var nestedTypeDef in typedef.NestedTypes) {
                if (!DeclaredTypes.Contains(nestedTypeDef.FullName))
                    ForwardDeclareType(context, output, nestedTypeDef);
            }

            output.NewLine();
        }

        protected void SealType (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, HashSet<TypeDefinition> sealedTypes) {
            var typeInfo = GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored)
                return;

            context.CurrentType = typedef;

            if (typedef.IsInterface)
                return;
            else if (typedef.IsEnum)
                return;

            foreach (var nestedTypedef in typedef.NestedTypes)
                SealType(context, output, nestedTypedef, sealedTypes);

            if (typeInfo.StaticConstructor != null)
                sealedTypes.Add(typedef);
        }

        protected void TranslateTypeDefinition (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, List<Action> initializer, bool stubbed) {
            var typeInfo = GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return;

            context.CurrentType = typedef;

            if (typedef.IsInterface)
                return;
            else if (typedef.IsEnum)
                return;
            else if (typeInfo.IsDelegate)
                return;

            var info = GetTypeInformation(typedef);
            if (info == null)
                throw new InvalidOperationException();

            var externalMemberNames = new HashSet<string>();
            var staticExternalMemberNames = new HashSet<string>();

            foreach (var method in typedef.Methods) {
                // We translate the static constructor explicitly later, and inject field initialization
                if (method.Name == ".cctor")
                    continue;

                TranslateMethod(
                    context, output, method, method, 
                    stubbed, externalMemberNames, staticExternalMemberNames
                );
            }
            
            Action initializeOverloadsAndProperties = () => {
                foreach (var methodGroup in info.MethodGroups)
                    TranslateMethodGroup(context, output, methodGroup);

                foreach (var property in typedef.Properties)
                    TranslateProperty(context, output, property);
            };

            if (!stubbed)
                initializeOverloadsAndProperties();

            Func<TypeReference, bool> isInterfaceIgnored = (i) => {
                var interfaceInfo = GetTypeInformation(i);
                if (interfaceInfo != null)
                    return interfaceInfo.IsIgnored;
                else
                    return true;
            };

            var interfaces = (from i in typeInfo.Interfaces
                              where !i.IsIgnored
                              select i).ToArray();

            if (interfaces.Length > 0) {
                initializer.Add(() => {
                    output.Identifier("JSIL.ImplementInterfaces", null);
                    output.LPar();
                    output.Identifier(typedef);
                    output.Comma();
                    output.OpenBracket(true);
                    output.CommaSeparatedList(interfaces, ListValueType.TypeReference);
                    output.CloseBracket(true);
                    output.RPar();
                    output.Semicolon();
                });
            }

            Func<FieldDefinition, bool> isFieldIgnored = (f) => {
                IMemberInfo memberInfo;
                if (typeInfo.Members.TryGetValue(MemberIdentifier.New(f), out memberInfo))
                    return memberInfo.IsIgnored;
                else
                    return true;
            };

            var structFields = 
                (from field in typedef.Fields
                where !isFieldIgnored(field) && 
                    !field.HasConstant &&
                    EmulateStructAssignment.IsStruct(field.FieldType) &&
                    !field.IsStatic
                select field).ToArray();

            if (structFields.Length > 0) {
                initializer.Add(() => {
                    output.Identifier(typedef);
                    output.Dot();
                    output.Identifier("prototype");
                    output.Dot();
                    output.Identifier("__StructFields__");
                    output.Token(" = ");
                    output.OpenBracket(true);

                    bool isFirst = true;
                    foreach (var sf in structFields) {
                        if (!isFirst) {
                            output.Comma();
                            output.NewLine();
                        }

                        output.OpenBracket();
                        output.Value(sf.Name);
                        output.Comma();
                        output.Identifier(sf.FieldType);
                        output.CloseBracket();

                        isFirst = false;
                    }

                    output.CloseBracket(true);
                    output.Semicolon();
                });
            }

            TranslateTypeStaticConstructor(context, output, typedef, info.StaticConstructor, stubbed);

            if (externalMemberNames.Count > 0) {
                initializer.Add(() => {
                    output.Identifier("JSIL.ExternalMembers", null);
                    output.LPar();
                    output.Identifier(typedef);
                    output.Dot();
                    output.Keyword("prototype");
                    output.Comma();
                    output.NewLine();

                    output.CommaSeparatedList(externalMemberNames, ListValueType.Primitive);
                    output.NewLine();

                    output.RPar();
                    output.Semicolon();
                    output.NewLine();
                    return;
                });
            }

            if (staticExternalMemberNames.Count > 0) {
                initializer.Add(() => {
                    output.Identifier("JSIL.ExternalMembers", null);
                    output.LPar();
                    output.Identifier(typedef);
                    output.Comma();
                    output.NewLine();

                    output.CommaSeparatedList(staticExternalMemberNames, ListValueType.Primitive);
                    output.NewLine();

                    output.RPar();
                    output.Semicolon();
                    output.NewLine();
                    return;
                });
            }

            if (stubbed &&
                (info.MethodGroups.Count + typedef.Properties.Count) > 0
            ) {
                initializer.Add(initializeOverloadsAndProperties);
            }

            output.NewLine();

            foreach (var nestedTypedef in typedef.NestedTypes)
                TranslateTypeDefinition(context, output, nestedTypedef, initializer, stubbed);
        }

        protected void TranslateMethodGroup (DecompilerContext context, JavascriptFormatter output, MethodGroupInfo methodGroup) {
            int i = 0;

            var methods = (from m in methodGroup.Methods where !m.IsIgnored select m).ToArray();
            if (methods.Length == 0)
                return;

            foreach (var method in methods) {
                foreach (var p in method.Member.Parameters) {
                    var resolved = p.ParameterType.Resolve();
                    if ((resolved != null) && 
                        !DeclaredTypes.Contains(resolved.FullName) &&
                        (resolved.Module.Assembly == methodGroup.DeclaringType.Definition.Module.Assembly)
                    ) {
                        ForwardDeclareType(context, output, resolved);
                    }
                }
            }

            output.Identifier("JSIL.OverloadedMethod", null);
            output.LPar();

            output.Identifier(methodGroup.DeclaringType.Definition);
            if (!methodGroup.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }

            output.Comma();
            output.Value(Util.EscapeIdentifier(methodGroup.Name));
            output.Comma();
            output.OpenBracket(true);

            bool isFirst = true;
            i = 0;
            foreach (var method in methods) {
                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.OpenBracket();
                output.Value(Util.EscapeIdentifier(method.GetName(true)));
                output.Comma();

                output.OpenBracket();
                output.CommaSeparatedList(
                    from p in method.Member.Parameters select p.ParameterType
                );
                output.CloseBracket();

                output.CloseBracket();
                isFirst = false;
            }

            output.CloseBracket(true);
            output.RPar();
            output.Semicolon();
        }

        internal JSFunctionExpression TranslateMethodExpression (DecompilerContext context, MethodReference method, MethodDefinition methodDef, Action<JSFunctionExpression> bodyTransformer = null) {
            var oldMethod = context.CurrentMethod;
            try {
                if (method == null)
                    throw new ArgumentNullException("method");
                if (methodDef == null)
                    throw new ArgumentNullException("methodDef");

                context.CurrentMethod = methodDef;
                if (methodDef.Body.Instructions.Count > LargeMethodThreshold)
                    this.StartedDecompilingMethod(method.FullName);

                ILBlock ilb;
                var decompiler = new ILAstBuilder();
                var optimizer = new ILAstOptimizer();

                try {
                    ilb = new ILBlock(decompiler.Build(methodDef, true));
                    optimizer.Optimize(context, ilb);
                } catch (Exception exception) {
                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(method.ToString(), exception);

                    return null;
                }

                var allVariables = ilb.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
                    .Where(v => v != null && !v.IsParameter).Distinct();

                foreach (var v in allVariables)
                    if (ILBlockTranslator.IsIgnoredType(v.Type))
                        return null;

                NameVariables.AssignNamesToVariables(context, decompiler.Parameters, allVariables, ilb);

                var translator = new ILBlockTranslator(
                    this, context, method, methodDef, ilb, decompiler.Parameters, allVariables
                );
                var body = translator.Translate();

                if (body == null)
                    return null;

                var function = new JSFunctionExpression(
                    methodDef, method,
                    translator.Variables,
                    from p in translator.ParameterNames select translator.Variables[p], 
                    body
                );

                // Run elimination repeatedly, since eliminating one variable may make it possible to eliminate others
                if (EliminateTemporaries) {
                    bool eliminated;
                    do {
                        var visitor = new EliminateSingleUseTemporaries(
                            context.CurrentModule.TypeSystem,
                            translator.Variables
                        );
                        visitor.Visit(function);
                        eliminated = visitor.EliminatedVariables.Count > 0;
                    } while (eliminated);
                }

                new EmulateStructAssignment(
                    context.CurrentModule.TypeSystem,
                    translator.CLR
                ).Visit(function);

                new IntroduceVariableDeclarations(
                    translator.Variables,
                    this
                ).Visit(function);

                new IntroduceVariableReferences(
                    translator.JSIL,
                    translator.Variables,
                    translator.ParameterNames
                ).Visit(function);

                // Temporary elimination makes it possible to simplify more operators, so do it last
                if (SimplifyOperators)
                    new SimplifyOperators(
                        translator.JSIL,
                        context.CurrentModule.TypeSystem
                    ).Visit(function);

                new IntroduceEnumCasts(
                    translator.TypeSystem
                ).Visit(function);

                if (bodyTransformer != null)
                    bodyTransformer(function);

                if (methodDef.Body.Instructions.Count > LargeMethodThreshold)
                    this.FinishedDecompilingMethod(method.FullName);

                return function;
            } finally {
                context.CurrentMethod = oldMethod;
            }
        }

        protected static bool NeedsStaticConstructor (TypeReference type) {
            if (EmulateStructAssignment.IsStruct(type))
                return true;
            else if (type.MetadataType != MetadataType.ValueType)
                return false;

            var resolved = type.Resolve();
            if (resolved == null)
                return true;

            if (resolved.IsEnum)
                return false;

            return true;
        }

        protected JSExpression TranslateField (FieldDefinition field) {
            JSDotExpression target;
            var fieldInfo = GetMemberInformation<Internal.FieldInfo>(field);
            if ((fieldInfo == null) || fieldInfo.IsIgnored)
                return null;
            
            if (field.IsStatic)
                target = JSDotExpression.New(
                    new JSType(field.DeclaringType), new JSField(field, fieldInfo)
                );
            else
                target = JSDotExpression.New(
                    new JSType(field.DeclaringType), new JSStringIdentifier("prototype"), new JSField(field, fieldInfo)
                );

            if (field.HasConstant) {
                return new JSInvocationExpression(
                    JSDotExpression.New(
                        new JSStringIdentifier("Object"), new JSStringIdentifier("defineProperty")
                    ),
                    target.Target, target.Member.ToLiteral(),
                    new JSObjectExpression(new JSPairExpression(
                        JSLiteral.New("value"),
                        JSLiteral.New(field.Constant as dynamic)
                    ))
                );
            } else {
                return new JSBinaryOperatorExpression(
                    JSOperator.Assignment, target,
                    new JSDefaultValueLiteral(field.FieldType), 
                    field.FieldType
                );
            }
        }

        protected void TranslateTypeStaticConstructor (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, MethodDefinition cctor, bool stubbed) {
            var typeSystem = context.CurrentModule.TypeSystem;
            var fieldsToEmit =
                (from f in typedef.Fields
                 where f.IsStatic && NeedsStaticConstructor(f.FieldType)
                 select f).ToArray();

            // We initialize all static fields in the cctor to avoid ordering issues
            Action<JSFunctionExpression> fixupCctor = (f) => {
                int insertPosition = 0;

                foreach (var field in fieldsToEmit) {
                    var expr = TranslateField(field);
                    if (expr != null) {
                        var stmt = new JSExpressionStatement(expr);
                        f.Body.Statements.Insert(insertPosition++, stmt);
                    }
                }
            };

            // Default values for instance fields of struct types are handled
            //  by the instance constructor.
            // Default values for static fields of struct types are handled
            //  by the cctor.
            // Everything else is emitted inline.

            foreach (var f in typedef.Fields) {
                if (f.IsStatic && NeedsStaticConstructor(f.FieldType))
                    continue;

                if (EmulateStructAssignment.IsStruct(f.FieldType))
                    continue;

                var expr = TranslateField(f);
                if (expr != null)
                    AstEmitter.Visit(new JSExpressionStatement(expr));
            }

            if ((cctor != null) && !stubbed) {
                TranslateMethod(context, output, cctor, cctor, false, null, null, fixupCctor);
            } else if (fieldsToEmit.Length > 0) {
                var fakeCctor = new MethodDefinition(".cctor", Mono.Cecil.MethodAttributes.Static, typeSystem.Void);
                fakeCctor.DeclaringType = typedef;

                var typeInfo = GetTypeInformation(typedef);
                typeInfo.StaticConstructor = fakeCctor;
                var identifier = MemberIdentifier.New(fakeCctor);
                typeInfo.Members[identifier] = new Internal.MethodInfo(typeInfo, identifier, fakeCctor, new ProxyInfo[0]);

                TranslateMethod(context, output, fakeCctor, fakeCctor, false, null, null, fixupCctor);
            }
        }

        protected void TranslateMethod (
            DecompilerContext context, JavascriptFormatter output, 
            MethodReference methodRef, MethodDefinition method, 
            bool stubbed, 
            HashSet<string> externalMemberNames,
            HashSet<string> staticExternalMemberNames,
            Action<JSFunctionExpression> bodyTransformer = null
        ) {
            var methodInfo = GetMemberInformation<Internal.MethodInfo>(method);
            if (methodInfo == null)
                return;

            if (methodInfo.IsExternal || stubbed) {
                if (methodInfo.Metadata.HasAttribute("JSIL.Meta.JSReplacement"))
                    return;

                if (externalMemberNames == null)
                    throw new ArgumentNullException("externalMemberNames");
                if (staticExternalMemberNames == null)
                    throw new ArgumentNullException("staticExternalMemberNames");

                if ((methodInfo.DeclaringProperty == null) || !methodInfo.Member.IsCompilerGenerated()) {
                    (method.IsStatic ? staticExternalMemberNames : externalMemberNames)
                        .Add(Util.EscapeIdentifier(methodInfo.GetName(true)));

                    return;
                }
            }

            if (methodInfo.IsIgnored)
                return;
            if (!method.HasBody)
                return;

            output.Identifier(method.DeclaringType);
            if (!method.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Dot();

            output.Identifier(methodInfo.GetName(true));

            output.Token(" = ");

            var function = TranslateMethodExpression(context, methodRef, method, (f) => {
                if (bodyTransformer != null)
                    bodyTransformer(f);

                AstEmitter.Visit(f);
                output.Semicolon();
            });

            if (function == null) {
                output.Identifier("JSIL.UntranslatableFunction", null);
                output.LPar();
                output.Value(method.FullName);
                output.RPar();
                output.Semicolon();
            }

            output.NewLine();
        }

        protected void TranslateProperty (DecompilerContext context, JavascriptFormatter output, PropertyDefinition property) {
            var propertyInfo = GetMemberInformation<Internal.PropertyInfo>(property);
            if ((propertyInfo == null) || propertyInfo.IsIgnored)
                return;

            output.Identifier("JSIL.MakeProperty", null);
            output.LPar();

            var isStatic = !(property.SetMethod ?? property.GetMethod).IsStatic;

            output.Identifier(property.DeclaringType);
            if (isStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Comma();

            output.Value(Util.EscapeIdentifier(propertyInfo.Name));

            output.Comma();
            output.NewLine();

            if (property.GetMethod != null) {
                output.Identifier(property.DeclaringType);
                if (isStatic) {
                    output.Dot();
                    output.Keyword("prototype");
                }
                output.Dot();
                output.Identifier(property.GetMethod, false);
            } else {
                output.Keyword("null");
            }

            output.Comma();

            if (property.SetMethod != null) {
                output.Identifier(property.DeclaringType);
                if (isStatic) {
                    output.Dot();
                    output.Keyword("prototype");
                }
                output.Dot();
                output.Identifier(property.SetMethod, false);
            } else {
                output.Keyword("null");
            }

            output.RPar();
            output.Semicolon();
        }
    }
}
