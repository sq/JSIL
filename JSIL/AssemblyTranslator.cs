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
using Mono.Cecil.Cil;

namespace JSIL {
    public enum FrameworkVersion {
        V35,
        V40
    }

    public class AssemblyTranslator {
        public const int LargeMethodThreshold = 1024;

        public readonly SymbolProvider SymbolProvider = new SymbolProvider();

        public readonly FunctionCache FunctionCache = new FunctionCache();
        public readonly TypeInfoProvider TypeInfoProvider;

        public readonly HashSet<string> GeneratedFiles = new HashSet<string>();
        public readonly List<Regex> IgnoredAssemblies = new List<Regex>();
        public readonly List<Regex> StubbedAssemblies = new List<Regex>();
        public readonly HashSet<string> DeclaredTypes = new HashSet<string>();

        public event Action<string, ProgressReporter> LoadingAssembly;

        public event Action<ProgressReporter> Decompiling;
        public event Action<ProgressReporter> Optimizing;
        public event Action<ProgressReporter> Writing;
        public event Action<string, ProgressReporter> DecompilingMethod;

        public event Action<string, Exception> CouldNotLoadSymbols;
        public event Action<string, Exception> CouldNotResolveAssembly;
        public event Action<string, Exception> CouldNotDecompileMethod;

        public string OutputDirectory = Environment.CurrentDirectory;

        public bool EliminateTemporaries = true;
        public bool OptimizeStructCopies = true;
        public bool SimplifyLoops = true;
        public bool SimplifyOperators = true;
        public bool IncludeDependencies = true;
        public bool UseSymbols = true;

        protected JavascriptAstEmitter AstEmitter;

        public AssemblyTranslator (
            FrameworkVersion frameworkVersion = FrameworkVersion.V40, 
            TypeInfoProvider typeInfoProvider = null
        ) {
            // Important to avoid preserving the proxy list from previous translations in this process
            MemberIdentifier.ResetProxies();

            if (typeInfoProvider != null) {
                TypeInfoProvider = typeInfoProvider;
            } else {
                TypeInfoProvider = new JSIL.TypeInfoProvider();

                var ar = new AssemblyResolver(new string[] { 
                    Path.GetDirectoryName(Util.GetPathOfAssembly(Assembly.GetExecutingAssembly())) 
                });

                switch (frameworkVersion) {
                    case FrameworkVersion.V35:
                        TypeInfoProvider.AddProxyAssemblies(ar.Resolve("JSIL.Proxies.3.5"));
                    break;
                    case FrameworkVersion.V40:
                        TypeInfoProvider.AddProxyAssemblies(ar.Resolve("JSIL.Proxies.4.0"));
                    break;
                }
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
                readerParameters.SymbolReaderProvider = SymbolProvider;

            return readerParameters;
        }

        public void AddProxyAssembly (string path, bool includeDependencies) {
            var assemblies = LoadAssembly(path, UseSymbols, includeDependencies);

            TypeInfoProvider.AddProxyAssemblies(assemblies);
        }

        public void AddProxyAssembly (Assembly assembly, bool includeDependencies) {
            var path = Util.GetPathOfAssembly(assembly);

            AddProxyAssembly(path, includeDependencies);
        }

        public AssemblyDefinition[] LoadAssembly (string path) {
            return LoadAssembly(path, UseSymbols, IncludeDependencies);
        }

        protected AssemblyDefinition[] LoadAssembly (string path, bool useSymbols, bool includeDependencies) {
            if (String.IsNullOrWhiteSpace(path))
                throw new InvalidDataException("Path was empty.");

            var readerParameters = GetReaderParameters(useSymbols, path);

            var pr = new ProgressReporter();
            if (LoadingAssembly != null)
                LoadingAssembly(path, pr);

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
                            SymbolReaderProvider = SymbolProvider
                        };

                        var pr2 = new ProgressReporter();
                        if (LoadingAssembly != null)
                            LoadingAssembly(reference.Name, pr2);

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

                        pr2.OnFinished();
                    }
                }
            }

            pr.OnFinished();
            return result.ToArray();
        }

        public AssemblyDefinition[] Translate (string assemblyPath, Stream outputStream = null, bool scanForProxies = true) {
            var assemblies = LoadAssembly(assemblyPath);

            if (scanForProxies)
                TypeInfoProvider.AddProxyAssemblies(assemblies);

            GeneratedFiles.Add(assemblyPath);

            var context = new DecompilerContext(assemblies.First().MainModule);

            context.Settings.YieldReturn = false;
            context.Settings.AnonymousMethods = true;
            context.Settings.QueryExpressions = false;
            context.Settings.LockStatement = false;
            context.Settings.FullyQualifyAmbiguousTypeNames = true;
            context.Settings.ForEachStatement = false;

            var pr = new ProgressReporter();
            if (Decompiling != null)
                Decompiling(pr);

            for (int i = 0; i < assemblies.Length; i++) {
                pr.OnProgressChanged(i, assemblies.Length);
                Analyze(context, assemblies[i]);
            }

            pr.OnFinished();

            OptimizeAll();

            Action<AssemblyDefinition> handler;

            if (outputStream == null) {
                handler = (assembly) => {
                    var outputPath = Path.Combine(OutputDirectory, assembly.Name + ".js");

                    if (File.Exists(outputPath))
                        File.Delete(outputPath);

                    using (outputStream = File.OpenWrite(outputPath))
                        Translate(context, assembly, outputStream);
                };

                if (!Directory.Exists(OutputDirectory))
                    Directory.CreateDirectory(OutputDirectory);
            } else {
                handler = (assembly) =>
                    Translate(context, assembly, outputStream);
            }

            pr = new ProgressReporter();
            if (Writing != null)
                Writing(pr);

            for (int i = 0; i < assemblies.Length; i++) {
                pr.OnProgressChanged(i, assemblies.Length);
                handler(assemblies[i]);
            }

            pr.OnFinished();

            return assemblies;
        }

        protected void OptimizeAll () {
            var pr = new ProgressReporter();
            if (Optimizing != null)
                Optimizing(pr);

            int i = 0;
            while (FunctionCache.OptimizationQueue.Count > 0) {
                var id = FunctionCache.OptimizationQueue.First();
                FunctionCache.OptimizationQueue.Remove(id);

                var e = FunctionCache.Cache[id];
                if (e.Expression == null) {
                    i++;
                    continue;
                }

                pr.OnProgressChanged(i++, i + FunctionCache.OptimizationQueue.Count);
                OptimizeFunction(e.SpecialIdentifiers, e.ParameterNames, e.Variables, e.Expression);
            }

            pr.OnFinished();
        }

        protected void Analyze (DecompilerContext context, AssemblyDefinition assembly) {
            bool isStubbed = IsStubbed(assembly);

            var allMethods = new Queue<MethodDefinition>();

            foreach (var module in assembly.Modules) {
                var moduleInfo = TypeInfoProvider.GetModuleInformation(module);
                if (moduleInfo.IsIgnored)
                    continue;

                var allTypes = new Queue<TypeDefinition>(module.Types);

                while (allTypes.Count > 0) {
                    var td = allTypes.Dequeue();

                    foreach (var nt in td.NestedTypes)
                        allTypes.Enqueue(nt);

                    if (!ShouldTranslateMethods(td))
                        continue;

                    foreach (var m in td.Methods) {
                        var methodInfo = TypeInfoProvider.GetMethod(m);

                        if ((methodInfo == null) || methodInfo.IsIgnored)
                            continue;
                        if (!m.HasBody)
                            continue;

                        var isProperty = (methodInfo.DeclaringProperty != null);

                        if (isStubbed && !isProperty)
                            continue;
                        if (isStubbed && isProperty)
                            if (!methodInfo.Member.IsCompilerGenerated())
                                continue;

                        allMethods.Enqueue(m);
                    }
                }
            }

            foreach (var m in allMethods) {
                context.CurrentModule = m.Module;
                context.CurrentType = m.DeclaringType;
                context.CurrentMethod = m;

                TranslateMethodExpression(context, m, m);
            }
        }

        protected bool IsStubbed (AssemblyDefinition assembly) {
            bool stubbed = false;
            foreach (var sa in StubbedAssemblies) {
                if (sa.IsMatch(assembly.FullName)) {
                    return true;
                    break;
                }
            }

            return false;
        }

        protected void Translate (DecompilerContext context, AssemblyDefinition assembly, Stream outputStream) {
            bool stubbed = IsStubbed(assembly);

            var manifest = new AssemblyManifest();
            var tw = new StreamWriter(outputStream, Encoding.ASCII);
            var formatter = new JavascriptFormatter(tw, this.TypeInfoProvider, manifest, assembly);

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
                TranslateModule(context, formatter, module, sealedTypes, stubbed);

            tw.Flush();
        }

        protected void TranslateModule (DecompilerContext context, JavascriptFormatter output, ModuleDefinition module, HashSet<TypeDefinition> sealedTypes, bool stubbed) {
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
                DeclareType(context, output, typedef, stubbed);
        }

        protected void TranslateInterface (DecompilerContext context, JavascriptFormatter output, TypeDefinition iface) {
            output.Identifier("JSIL.MakeInterface", null);
            output.LPar();
            output.NewLine();

            output.Value(Util.EscapeIdentifier(iface.FullName, EscapingMode.String));
            output.Comma();

            output.Value(iface.IsPublic);
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

            output.Value(enm.IsPublic);
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

        protected void DeclareType (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, bool stubbed) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return;

            // Basic type translation/definition logic does not work for System.Object. The definition from JSIL.Core must be used.
            if (typedef.FullName == "System.Object")
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
                    DeclareType(context, output, declaringType, IsStubbed(declaringType.Module.Assembly));
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
                    DeclareType(context, output, resolved, IsStubbed(resolved.Module.Assembly));
                }
            }

            bool isStatic = typedef.IsAbstract && typedef.IsSealed;

            if (isStatic) {
                output.Identifier("JSIL.MakeStaticClass", null);
                output.LPar();

                output.Value(Util.EscapeIdentifier(typedef.FullName, EscapingMode.String));
                output.Comma();
                output.Value(typedef.IsPublic);

                output.Comma();
                output.OpenBracket();
                if (typedef.HasGenericParameters)
                    output.CommaSeparatedList(
                        (from p in typedef.GenericParameters select p.Name), ListValueType.Primitive
                    );
                output.CloseBracket();

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

                output.Comma();
                output.OpenBracket();
                if (typedef.HasGenericParameters)
                    output.CommaSeparatedList(
                        (from p in typedef.GenericParameters select p.Name), ListValueType.Primitive
                    );
                output.CloseBracket();

            }

            output.Comma();
            output.OpenFunction(null, (f) => {
                f.Identifier("$");
            });

            TranslateTypeDefinition(context, output, typedef, stubbed);

            output.NewLine();

            output.CloseBrace(false);

            output.RPar();
            output.Semicolon();
            output.NewLine();

            foreach (var nestedTypeDef in typedef.NestedTypes) {
                if (!DeclaredTypes.Contains(nestedTypeDef.FullName))
                    DeclareType(context, output, nestedTypeDef, stubbed);
            }
        }

        protected bool ShouldTranslateMethods (TypeDefinition typedef) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return false;

            if (typedef.IsInterface)
                return false;
            else if (typedef.IsEnum)
                return false;
            else if (typeInfo.IsDelegate)
                return false;

            return true;
        }

        protected void TranslateTypeDefinition (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, bool stubbed) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if (!ShouldTranslateMethods(typedef))
                return;

            context.CurrentType = typedef;

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
                foreach (var methodGroup in typeInfo.MethodGroups)
                    TranslateMethodGroup(context, output, methodGroup);

                foreach (var property in typedef.Properties)
                    TranslateProperty(context, output, property);
            };

            Func<TypeReference, bool> isInterfaceIgnored = (i) => {
                var interfaceInfo = TypeInfoProvider.GetTypeInformation(i);
                if (interfaceInfo != null)
                    return interfaceInfo.IsIgnored;
                else
                    return true;
            };

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
                output.Identifier("$", null);
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
                    output.TypeReference(sf.FieldType);
                    output.CloseBracket();

                    isFirst = false;
                }

                output.CloseBracket(true, output.Semicolon);
            }

            TranslateTypeStaticConstructor(context, output, typedef, typeInfo.StaticConstructor, stubbed);

            if (externalMemberNames.Count > 0) {
                output.Identifier("JSIL.ExternalMembers", null);
                output.LPar();
                output.Identifier("$", null);
                output.Comma();
                output.Value(true);
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
                output.Identifier("$", null);
                output.Comma();
                output.Value(false);
                output.Comma();
                output.NewLine();

                output.CommaSeparatedList(staticExternalMemberNames, ListValueType.Primitive);
                output.NewLine();

                output.RPar();
                output.Semicolon();
            }

            if ((typeInfo.MethodGroups.Count + typedef.Properties.Count) > 0)
                initializeOverloadsAndProperties();

            var interfaces = (from i in typeInfo.Interfaces
                              where !i.IsIgnored
                              select i).ToArray();

            if (interfaces.Length > 0) {
                output.Identifier("JSIL.ImplementInterfaces", null);
                output.LPar();
                output.Identifier("$", null);
                output.Comma();
                output.OpenBracket(true);
                output.CommaSeparatedList(interfaces, ListValueType.TypeReference);
                output.CloseBracket(true, () => {
                    output.RPar();
                    output.Semicolon();
                });
            }
        }

        protected void TranslateMethodGroup (DecompilerContext context, JavascriptFormatter output, MethodGroupInfo methodGroup) {
            var methods = (from m in methodGroup.Methods where !m.IsIgnored select m).ToArray();
            if (methods.Length < 1)
                return;

            output.Identifier(
                (methods.First().IsGeneric) ? "JSIL.OverloadedGenericMethod" : "JSIL.OverloadedMethod", null
            );
            output.LPar();

            output.Identifier("$", null);
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
                output.Value(method.OverloadIndex.Value);
                output.Comma();

                output.OpenBracket();
                output.CommaSeparatedList(
                    from p in method.Member.Parameters select p.ParameterType, ListValueType.TypeReference
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

        internal JSFunctionExpression TranslateMethodExpression (DecompilerContext context, MethodReference method, MethodDefinition methodDef) {
            var oldMethod = context.CurrentMethod;
            try {
                if (method == null)
                    throw new ArgumentNullException("method");
                if (methodDef == null)
                    throw new ArgumentNullException("methodDef");

                var methodInfo = TypeInfoProvider.GetMemberInformation<JSIL.Internal.MethodInfo>(methodDef);

                var identifier = new QualifiedMemberIdentifier(
                    methodInfo.DeclaringType.Identifier, methodInfo.Identifier
                );
                JSFunctionExpression function = null;

                if (FunctionCache.TryGetExpression(identifier, out function))
                    return function;

                if (methodInfo.IsExternal) {
                    FunctionCache.CreateNull(methodInfo, method, identifier);
                    return null;
                }

                var bodyDef = methodDef;
                if (methodInfo.IsFromProxy && methodInfo.Member.HasBody)
                    bodyDef = methodInfo.Member;

                var pr = new ProgressReporter();

                context.CurrentMethod = methodDef;
                if ((methodDef.Body.Instructions.Count > LargeMethodThreshold) && (this.DecompilingMethod != null))
                    this.DecompilingMethod(method.FullName, pr);

                ILBlock ilb;
                var decompiler = new ILAstBuilder();
                var optimizer = new ILAstOptimizer();

                try {
                    ilb = new ILBlock(decompiler.Build(bodyDef, true, context));
                    optimizer.Optimize(context, ilb);
                } catch (Exception exception) {
                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(bodyDef.FullName, exception);

                    FunctionCache.CreateNull(methodInfo, method, identifier);
                    pr.OnFinished();
                    return null;
                }

                var allVariables = ilb.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
                    .Where(v => v != null && !v.IsParameter).Distinct();

                foreach (var v in allVariables) {
                    if (ILBlockTranslator.IsIgnoredType(v.Type)) {
                        FunctionCache.CreateNull(methodInfo, method, identifier);
                        pr.OnFinished();
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
                    FunctionCache.CreateNull(methodInfo, method, identifier);
                    pr.OnFinished();
                    return null;
                }

                var parameters = from p in translator.ParameterNames select translator.Variables[p];

                if (method.HasGenericParameters) {
                    var type = new TypeReference("System", "Type", context.CurrentModule.TypeSystem.Object.Module, context.CurrentModule.TypeSystem.Object.Scope);
                    parameters = (from gp in method.GenericParameters select new JSVariable(gp.Name, type, method)).Concat(parameters);
                }

                function = FunctionCache.Create(
                    methodInfo, methodDef, method, identifier, 
                    translator, parameters, body
                );

                pr.OnFinished();
                return function;
            } finally {
                context.CurrentMethod = oldMethod;
            }
        }

        private void OptimizeFunction (
            SpecialIdentifiers si, HashSet<string> parameterNames,
            Dictionary<string, JSVariable> variables, JSFunctionExpression function
        ) {
            // Run elimination repeatedly, since eliminating one variable may make it possible to eliminate others
            if (EliminateTemporaries) {
                bool eliminated;
                do {
                    var visitor = new EliminateSingleUseTemporaries(
                        si.TypeSystem, variables, FunctionCache
                    );
                    visitor.Visit(function);
                    eliminated = visitor.EliminatedVariables.Count > 0;
                } while (eliminated);
            }

            new EmulateStructAssignment(
                si.TypeSystem,
                FunctionCache,
                si.CLR,
                OptimizeStructCopies
            ).Visit(function);

            new IntroduceVariableDeclarations(
                variables,
                TypeInfoProvider
            ).Visit(function);

            new IntroduceVariableReferences(
                si.JSIL,
                variables,
                parameterNames
            ).Visit(function);

            if (SimplifyLoops)
                new SimplifyLoops(
                    si.TypeSystem
                ).Visit(function);

            // Temporary elimination makes it possible to simplify more operators, so do it last
            if (SimplifyOperators)
                new SimplifyOperators(
                    si.JSIL, si.JS, si.TypeSystem
                ).Visit(function);

            new IntroduceEnumCasts(
                si.TypeSystem
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
                    new JSStringIdentifier("$", field.DeclaringType), new JSField(field, fieldInfo)
                );
            else
                target = JSDotExpression.New(
                    new JSStringIdentifier("$", field.DeclaringType), new JSStringIdentifier("prototype"), new JSField(field, fieldInfo)
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
                        new JSStringIdentifier("JSIL"), new JSFakeMethod("MakeConstant", field.Module.TypeSystem.Void)
                    ), new[] { 
                        target.Target, target.Member.ToLiteral(), constant                            
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

                typeInfo.Members[identifier] = new Internal.MethodInfo(
                    typeInfo, identifier, fakeCctor, new ProxyInfo[0], false
                );

                // Generate the fake constructor, since it wasn't created during the analysis pass
                TranslateMethodExpression(context, fakeCctor, fakeCctor);

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

                var isProperty = methodInfo.DeclaringProperty != null;

                if (!isProperty || !methodInfo.Member.IsCompilerGenerated()) {
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

            output.Identifier("$", null);
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

            JSFunctionExpression function;
            function = FunctionCache.GetExpression(new QualifiedMemberIdentifier(
                methodInfo.DeclaringType.Identifier,
                methodInfo.Identifier
            ));

            if (bodyTransformer != null)
                bodyTransformer(function);

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

            output.Identifier("$", null);
            if (!isStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Comma();

            output.Value(Util.EscapeIdentifier(propertyInfo.Name));

            output.Comma();
            output.NewLine();

            if (property.GetMethod != null) {
                output.Identifier("$", null);
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
                output.Identifier("$", null);
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
