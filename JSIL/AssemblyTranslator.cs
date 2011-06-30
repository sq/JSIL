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

    public class AssemblyTranslator {
        public const int LargeMethodThreshold = 1024;

        public readonly FunctionCache FunctionCache = new FunctionCache();
        public readonly TypeInfoProvider TypeInfoProvider;

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
        public bool SimplifyLoops = true;
        public bool SimplifyOperators = true;
        public bool IncludeDependencies = true;
        public bool UseSymbols = true;

        protected JavascriptAstEmitter AstEmitter;

        public AssemblyTranslator (TypeInfoProvider typeInfoProvider = null) {
            // Important to avoid preserving the proxy list from previous translations in this process
            MemberIdentifier.ResetProxies();

            if (typeInfoProvider != null) {
                TypeInfoProvider = typeInfoProvider;
            } else {
                TypeInfoProvider = new JSIL.TypeInfoProvider();
                AddProxyAssembly(typeof(JSIL.Proxies.ObjectProxy).Assembly, false);
            }
        }

        protected virtual ReaderParameters GetReaderParameters (bool useSymbols, string mainAssemblyPath = null) {
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

        public void AddProxyAssembly (string path, bool includeDependencies) {
            var assemblies = LoadAssembly(path, UseSymbols, includeDependencies);

            TypeInfoProvider.AddProxyAssemblies(assemblies);
        }

        public void AddProxyAssembly (Assembly assembly, bool includeDependencies) {
            var uri = new Uri(assembly.CodeBase);
            var path = Uri.UnescapeDataString(uri.AbsolutePath);
            if (String.IsNullOrWhiteSpace(path))
                path = assembly.Location;

            AddProxyAssembly(path, includeDependencies);
        }

        public AssemblyDefinition[] LoadAssembly (string path) {
            return LoadAssembly(path, UseSymbols, IncludeDependencies);
        }

        protected AssemblyDefinition[] LoadAssembly (string path, bool useSymbols, bool includeDependencies) {
            if (String.IsNullOrWhiteSpace(path))
                throw new InvalidDataException("Path was empty.");

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
                            StartedLoadingAssembly(reference.Name);

                        AssemblyDefinition refAssembly = null;
                        assemblyNames.Add(reference.FullName);
                        try {
                            refAssembly = readerParameters.AssemblyResolver.Resolve(reference, readerParameters);
                        } catch (Exception ex) {
                            if (useSymbols) {
                                try {
                                    refAssembly = readerParameters.AssemblyResolver.Resolve(reference, GetReaderParameters(false, path));
                                    if (CouldNotLoadSymbols != null)
                                        CouldNotLoadSymbols(reference.Name, ex);
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

        public AssemblyDefinition[] Translate (string assemblyPath, Stream outputStream = null, bool scanForProxies = true) {
            var assemblies = LoadAssembly(assemblyPath);

            if (scanForProxies)
                TypeInfoProvider.AddProxyAssemblies(assemblies);

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

            return assemblies;
        }

        public void Translate (AssemblyDefinition assembly, Stream outputStream) {
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
            var formatter = new JavascriptFormatter(tw, this.TypeInfoProvider, assembly);

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            formatter.Comment(
                "Generated by JSIL v{0}.{1}.{2} build {3}. See http://jsil.org/ for more information.", 
                version.Major, version.Minor, version.Build, version.Revision
            );
            formatter.NewLine();

            if (stubbed) {
                formatter.Comment("Generating type stubs only");
                formatter.NewLine();
            }

            formatter.DeclareAssembly();

            var sealedTypes = new HashSet<TypeDefinition>();

            // Important to clear this because types with the exact same full names can be defined in multiple assemblies
            DeclaredTypes.Clear();
            foreach (var module in assembly.Modules)
                TranslateModule(context, formatter, module, initializer, sealedTypes, stubbed);

            if (sealedTypes.Count > 0) {
                var groups = (from st in sealedTypes
                              let parent = JavascriptFormatter.GetParent(st, null)
                              group st by parent);

                initializer.Add(() => {
                    foreach (var g in groups) {
                        formatter.Identifier("JSIL.SealTypes", null);
                        formatter.LPar();

                        formatter.Identifier(formatter.PrivateToken);
                        formatter.Comma();
                        formatter.Value(g.Key);
                        formatter.Comma();

                        formatter.NewLine();

                        formatter.CommaSeparatedList(
                            (from typedef in g select typedef.Name), ListValueType.Primitive
                        );

                        formatter.NewLine();
                        formatter.RPar();
                        formatter.Semicolon();
                    }
                });
            }

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

        protected void TranslateModule (DecompilerContext context, JavascriptFormatter output, ModuleDefinition module, List<Action> initializer, HashSet<TypeDefinition> sealedTypes, bool stubbed) {
            var moduleInfo = TypeInfoProvider.GetModuleInformation(module);
            if (moduleInfo.IsIgnored)
                return;

            context.CurrentModule = module;

            var js = new JSSpecialIdentifiers(context.CurrentModule.TypeSystem);
            var jsil = new JSILIdentifier(context.CurrentModule.TypeSystem, js);

            // Probably should be an argument, not a member variable...
            AstEmitter = new JavascriptAstEmitter(
                output, jsil, context.CurrentModule.TypeSystem, this.TypeInfoProvider
            );

            foreach (var typedef in module.Types)
                ForwardDeclareType(context, output, typedef);

            foreach (var typedef in module.Types) {
                TranslateTypeDefinition(context, output, typedef, initializer, stubbed);
                SealType(context, output, typedef, sealedTypes);
            }
        }

        protected void TranslateInterface (DecompilerContext context, JavascriptFormatter output, TypeDefinition iface) {
            output.Identifier("JSIL.MakeInterface", null);
            output.LPar();
            output.NewLine();

            output.Value(Util.EscapeIdentifier(iface.FullName, EscapingMode.String));
            output.Comma();

            output.OpenBracket();
            output.CommaSeparatedList(
                (from p in iface.GenericParameters select p.Name), ListValueType.Primitive
            );
            output.CloseBracket();
            output.Comma();

            output.OpenBrace();

            bool isFirst = true;
            foreach (var m in iface.Methods) {
                var methodInfo = TypeInfoProvider.GetMethod(m);
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
                var propertyInfo = TypeInfoProvider.GetProperty(p);
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

            var typeInformation = TypeInfoProvider.GetTypeInformation(enm);
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
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
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

                if (typedef.HasGenericParameters) {
                    output.Comma();
                    output.OpenBracket();
                    output.CommaSeparatedList(
                        (from p in typedef.GenericParameters select p.Name), ListValueType.Primitive
                    );
                    output.CloseBracket();
                }

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

                if (typedef.HasGenericParameters) {
                    output.Comma();
                    output.OpenBracket();
                    output.CommaSeparatedList(
                        (from p in typedef.GenericParameters select p.Name), ListValueType.Primitive
                    );
                    output.CloseBracket();
                }

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
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored)
                return;

            context.CurrentType = typedef;

            if (typedef.IsInterface)
                return;
            else if (typedef.IsEnum)
                return;

            foreach (var nestedTypedef in typedef.NestedTypes)
                SealType(context, output, nestedTypedef, sealedTypes);

            if (
                (typeInfo.StaticConstructor != null) ||
                (typedef.BaseType is GenericInstanceType)
            ) {
                sealedTypes.Add(typedef);
            }
        }

        protected void TranslateTypeDefinition (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, List<Action> initializer, bool stubbed) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return;

            context.CurrentType = typedef;

            if (typedef.IsInterface)
                return;
            else if (typedef.IsEnum)
                return;
            else if (typeInfo.IsDelegate)
                return;

            var info = TypeInfoProvider.GetTypeInformation(typedef);
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
                var interfaceInfo = TypeInfoProvider.GetTypeInformation(i);
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
                    output.CloseBracket(true, () => {
                        output.RPar();
                        output.Semicolon();
                    });
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

                    output.CloseBracket(true, output.Semicolon);
                });
            }

            TranslateTypeStaticConstructor(context, output, typedef, info.StaticConstructor, stubbed);

            if (externalMemberNames.Count + staticExternalMemberNames.Count > 0) {
                initializer.Add(() => {
                    if (externalMemberNames.Count > 0) {
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
                    }

                    if (staticExternalMemberNames.Count > 0) {
                        output.Identifier("JSIL.ExternalMembers", null);
                        output.LPar();
                        output.Identifier(typedef);
                        output.Comma();
                        output.NewLine();

                        output.CommaSeparatedList(staticExternalMemberNames, ListValueType.Primitive);
                        output.NewLine();

                        output.RPar();
                        output.Semicolon();
                    }
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
            var methods = (from m in methodGroup.Methods where !m.IsIgnored select m).ToArray();
            if (methods.Length < 1)
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

            output.Identifier(
                (methods.First().IsGeneric) ? "JSIL.OverloadedGenericMethod" : "JSIL.OverloadedMethod", null
            );
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
                    from p in method.Member.Parameters select p.ParameterType, ListValueType.TypeIdentifier
                );
                output.CloseBracket();

                output.CloseBracket();
                isFirst = false;
            }

            output.CloseBracket(true, () => {
                output.RPar();
                output.Semicolon();
            });
        }

        internal JSFunctionExpression TranslateMethodExpression (DecompilerContext context, MethodReference method, MethodDefinition methodDef, Action<JSFunctionExpression> bodyTransformer = null) {
            var oldMethod = context.CurrentMethod;
            try {
                if (method == null)
                    throw new ArgumentNullException("method");
                if (methodDef == null)
                    throw new ArgumentNullException("methodDef");

                var identifier = new QualifiedMemberIdentifier(
                    new TypeIdentifier(methodDef.DeclaringType),
                    new MemberIdentifier(method)
                );
                JSFunctionExpression function = null;

                var methodInfo = TypeInfoProvider.GetMemberInformation<JSIL.Internal.MethodInfo>(methodDef);
                if (methodInfo.IsExternal)
                    return null;

                var bodyDef = methodDef;
                if (methodInfo.IsFromProxy && methodInfo.Member.HasBody)
                    bodyDef = methodInfo.Member;

                context.CurrentMethod = methodDef;
                if (methodDef.Body.Instructions.Count > LargeMethodThreshold)
                    this.StartedDecompilingMethod(method.FullName);

                ILBlock ilb;
                var decompiler = new ILAstBuilder();
                var optimizer = new ILAstOptimizer();

                try {
                    ilb = new ILBlock(decompiler.Build(bodyDef, true));
                    optimizer.Optimize(context, ilb);
                } catch (Exception exception) {
                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(bodyDef.FullName, exception);

                    return null;
                }

                var allVariables = ilb.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
                    .Where(v => v != null && !v.IsParameter).Distinct();

                foreach (var v in allVariables) {
                    if (ILBlockTranslator.IsIgnoredType(v.Type)) {
                        return null;
                    }
                }

                NameVariables.AssignNamesToVariables(context, decompiler.Parameters, allVariables, ilb);

                var translator = new ILBlockTranslator(
                    this, context, method, methodDef, 
                    ilb, decompiler.Parameters, allVariables
                );
                var body = translator.Translate();

                if (body == null) {
                    return null;
                }

                var parameters = from p in translator.ParameterNames select translator.Variables[p];

                if (method.HasGenericParameters) {
                    var type = new TypeReference("System", "Type", context.CurrentModule.TypeSystem.Object.Module, context.CurrentModule.TypeSystem.Object.Scope);
                    parameters = (from gp in method.GenericParameters select new JSVariable(gp.Name, type, method)).Concat(parameters);
                }

                function = FunctionCache.Create(
                    methodDef, method, identifier, 
                    translator, parameters, body
                );

                OptimizeFunction(context, translator, function);

                if (bodyTransformer != null)
                    bodyTransformer(function);

                if (methodDef.Body.Instructions.Count > LargeMethodThreshold)
                    this.FinishedDecompilingMethod(method.FullName);

                return function;
            } finally {
                context.CurrentMethod = oldMethod;
            }
        }

        private void OptimizeFunction (DecompilerContext context, ILBlockTranslator translator, JSFunctionExpression function) {
            // Run elimination repeatedly, since eliminating one variable may make it possible to eliminate others
            if (EliminateTemporaries) {
                bool eliminated;
                do {
                    var visitor = new EliminateSingleUseTemporaries(
                        context.CurrentModule.TypeSystem,
                        translator.Variables, FunctionCache
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
                TypeInfoProvider
            ).Visit(function);

            new IntroduceVariableReferences(
                translator.JSIL,
                translator.Variables,
                translator.ParameterNames
            ).Visit(function);

            if (SimplifyLoops)
                new SimplifyLoops(
                    context.CurrentModule.TypeSystem
                ).Visit(function);

            // Temporary elimination makes it possible to simplify more operators, so do it last
            if (SimplifyOperators)
                new SimplifyOperators(
                    translator.JSIL, translator.JS,
                    context.CurrentModule.TypeSystem
                ).Visit(function);

            new IntroduceEnumCasts(
                context.CurrentModule.TypeSystem
            ).Visit(function);
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
            var fieldInfo = TypeInfoProvider.GetMemberInformation<Internal.FieldInfo>(field);
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
                JSLiteral constant;
                if (field.Constant == null) {
                    constant = JSLiteral.Null(field.FieldType);
                } else {
                    constant = JSLiteral.New(field.Constant as dynamic);
                }

                return JSInvocationExpression.InvokeStatic(
                    JSDotExpression.New(
                        new JSStringIdentifier("Object"), new JSFakeMethod("defineProperty", field.Module.TypeSystem.Void)
                    ), new[] { 
                        target.Target, target.Member.ToLiteral(),
                        new JSObjectExpression(new JSPairExpression(
                            JSLiteral.New("value"), constant                            
                        ))
                    }
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

                var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
                typeInfo.StaticConstructor = fakeCctor;
                var identifier = MemberIdentifier.New(fakeCctor);
                typeInfo.Members[identifier] = new Internal.MethodInfo(typeInfo, identifier, fakeCctor, new ProxyInfo[0], false);

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
            var methodInfo = TypeInfoProvider.GetMemberInformation<Internal.MethodInfo>(method);
            if (methodInfo == null)
                return;

            bool isReplaced = methodInfo.Metadata.HasAttribute("JSIL.Meta.JSReplacement");
            bool methodIsProxied = (methodInfo.IsFromProxy && methodInfo.Member.HasBody) && 
                !methodInfo.IsExternal && !isReplaced;

            if (methodInfo.IsExternal || (stubbed && !methodIsProxied)) {
                if (isReplaced)
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

            if (methodIsProxied) {
                output.Comment("Implementation from {0}", methodInfo.Member.DeclaringType.FullName);
                output.NewLine();
            }

            output.Identifier(method.DeclaringType);
            if (!method.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Dot();

            output.Identifier(methodInfo.GetName(true));

            output.Token(" = ");

            if (method.HasGenericParameters) {
                output.Identifier("JSIL.GenericMethod", null);
                output.LPar();
                output.NewLine();
                output.OpenBracket();

                output.CommaSeparatedList((from p in method.GenericParameters select p.Name), ListValueType.Primitive);

                output.CloseBracket();
                output.Comma();
                output.NewLine();
            }

            var function = TranslateMethodExpression(context, methodRef, method, (f) => {
                if (bodyTransformer != null)
                    bodyTransformer(f);
            });

            if (function != null) {
                AstEmitter.Visit(function);
            } else {
                output.Identifier("JSIL.UntranslatableFunction", null);
                output.LPar();
                output.Value(method.FullName);
                output.RPar();
            }

            if (method.HasGenericParameters) {
                output.NewLine();
                output.RPar();
            }

            output.Semicolon();
        }

        protected void TranslateProperty (DecompilerContext context, JavascriptFormatter output, PropertyDefinition property) {
            var propertyInfo = TypeInfoProvider.GetMemberInformation<Internal.PropertyInfo>(property);
            if ((propertyInfo == null) || propertyInfo.IsIgnored)
                return;

            var isStatic = (property.SetMethod ?? property.GetMethod).IsStatic;

            if (property.DeclaringType.HasGenericParameters && isStatic)
                output.Identifier("JSIL.MakeGenericProperty", null);
            else
                output.Identifier("JSIL.MakeProperty", null);

            output.LPar();

            output.Identifier(property.DeclaringType);
            if (!isStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Comma();

            output.Value(Util.EscapeIdentifier(propertyInfo.Name));

            output.Comma();
            output.NewLine();

            if (property.GetMethod != null) {
                output.Identifier(property.DeclaringType);
                if (!isStatic) {
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
                if (!isStatic) {
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
