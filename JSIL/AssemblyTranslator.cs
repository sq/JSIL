using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using JSIL.Translator;
using Mono.Cecil;
using ICSharpCode.Decompiler;
using MethodInfo = JSIL.Internal.MethodInfo;

namespace JSIL {
    public class AssemblyTranslator : IDisposable {
        struct MethodToAnalyze {
            public readonly MethodDefinition MD;
            public readonly MethodInfo MI;

            public MethodToAnalyze (MethodDefinition md) {
                MD = md;
                MI = null;
            }

            public MethodToAnalyze (MethodInfo mi) {
                MD = mi.Member;
                MI = mi;
            }
        }

        public const int LargeMethodThreshold = 20 * 1024;

        public readonly Configuration Configuration;

        public readonly SymbolProvider SymbolProvider = new SymbolProvider();
        public readonly AssemblyCache AssemblyCache;
        public readonly FunctionCache FunctionCache;
        public readonly AssemblyManifest Manifest;

        public event Action<string> AssemblyLoaded;

        public event Action<ProgressReporter> Decompiling;
        public event Action<ProgressReporter> Optimizing;
        public event Action<ProgressReporter> Writing;
        public event Action<string, ProgressReporter> DecompilingMethod;

        public event Action<string, Exception> CouldNotLoadSymbols;
        public event Action<string, Exception> CouldNotResolveAssembly;
        public event Action<string, Exception> CouldNotDecompileMethod;
        public event Action<string> Warning;

        internal readonly TypeInfoProvider _TypeInfoProvider;

        protected bool OwnsAssemblyCache;
        protected bool OwnsTypeInfoProvider;

        protected readonly static HashSet<string> TypeDeclarationsToSuppress = new HashSet<string> {
            "System.Object", "System.Type", "System.RuntimeType",
            "System.Reflection.MemberInfo", "System.Reflection.MethodBase", 
            "System.Reflection.MethodInfo", "System.Reflection.FieldInfo",
            "System.Reflection.ConstructorInfo", "System.Reflection.PropertyInfo",
            "System.Array", "System.Delegate", "System.MulticastDelegate"
        }; 

        public AssemblyTranslator (
            Configuration configuration,
            TypeInfoProvider typeInfoProvider = null,
            AssemblyManifest manifest = null,
            AssemblyCache assemblyCache = null
        ) {
            Warning = (s) => {
                Console.Error.WriteLine("// {0}", s);
            };

            Configuration = configuration;
            bool useDefaultProxies = configuration.UseDefaultProxies.GetValueOrDefault(true);

            if (manifest != null)
                Manifest = manifest;
            else
                Manifest = new AssemblyManifest();

            if (typeInfoProvider != null) {
                _TypeInfoProvider = typeInfoProvider;
                OwnsTypeInfoProvider = false;

                if (configuration.Assemblies.Proxies.Count > 0)
                    throw new InvalidOperationException("Cannot reuse an existing type provider if explicitly loading proxies");
            } else {
                _TypeInfoProvider = new JSIL.TypeInfoProvider();
                OwnsTypeInfoProvider = true;

                if (useDefaultProxies) {
                    var defaultProxyAssembly =
                        GetDefaultProxyAssembly(configuration.FrameworkVersion.GetValueOrDefault(4.0));

                    if (defaultProxyAssembly == null)
                        throw new InvalidOperationException("No default proxy assembly was loaded.");

                    AddProxyAssembly(defaultProxyAssembly);    
                }
              
                foreach (var fn in configuration.Assemblies.Proxies.Distinct())
                    AddProxyAssembly(fn);
            }

            OwnsAssemblyCache = (assemblyCache == null);
            if (assemblyCache != null)
                AssemblyCache = assemblyCache;
            else
                AssemblyCache = new AssemblyCache();

            FunctionCache = new FunctionCache(_TypeInfoProvider);
        }

        public static Assembly GetDefaultProxyAssembly (double frameworkVersion) {
            var myAssemblyPath = Util.GetPathOfAssembly(Assembly.GetExecutingAssembly());
            var proxyFolder = Path.GetDirectoryName(myAssemblyPath);
            string proxyPath = null;

            try {
                if (frameworkVersion == 4.0) {
                    proxyPath = Path.Combine(proxyFolder, "JSIL.Proxies.4.0.dll");
                } else if (frameworkVersion <= 3.5) {
                    proxyPath = Path.Combine(proxyFolder, "JSIL.Proxies.3.5.dll");
                } else {
                    throw new ArgumentOutOfRangeException(
                        "FrameworkVersion",
                        String.Format("Framework version '{0}' not supported", frameworkVersion)
                    );
                }

                return Assembly.LoadFile(proxyPath);
            } catch (FileNotFoundException fnf) {
                throw new FileNotFoundException(
                    String.Format("Could not load the .NET proxies assembly from '{0}'.", proxyPath),
                    fnf
                );
            }        
        }

        internal void WarningFormat (string format, params object[] args) {
            Warning(String.Format(format, args));
        }

        protected virtual ReaderParameters GetReaderParameters (bool useSymbols, string mainAssemblyPath = null) {
            var readerParameters = new ReaderParameters {
                ReadingMode = ReadingMode.Deferred,
                ReadSymbols = useSymbols
            };

            if (mainAssemblyPath != null) {
                readerParameters.AssemblyResolver = new AssemblyResolver(new string[] { 
                    Path.GetDirectoryName(mainAssemblyPath),
                    Path.GetDirectoryName(Util.GetPathOfAssembly(Assembly.GetExecutingAssembly())) 
                }, AssemblyCache);
                readerParameters.MetadataResolver = new CachingMetadataResolver(readerParameters.AssemblyResolver);
            }

            if (useSymbols)
                readerParameters.SymbolReaderProvider = SymbolProvider;

            return readerParameters;
        }

        public void AddProxyAssembly (string path) {
            var assemblies = LoadAssembly(path, Configuration.UseSymbols.GetValueOrDefault(true), false);

            _TypeInfoProvider.AddProxyAssemblies(assemblies);
        }

        public void AddProxyAssembly (Assembly assembly) {
            var path = Util.GetPathOfAssembly(assembly);

            AddProxyAssembly(path);
        }

        public AssemblyDefinition[] LoadAssembly (string path) {
            return LoadAssembly(
                path, 
                Configuration.UseSymbols.GetValueOrDefault(true), 
                Configuration.IncludeDependencies.GetValueOrDefault(true)
            );
        }

        protected AssemblyDefinition AssemblyLoadErrorWrapper<T> (
            Func<T, ReaderParameters, AssemblyDefinition> loader,
            T arg1, ReaderParameters readerParameters, 
            bool useSymbols, string mainAssemblyPath
        ) {
            AssemblyDefinition result = null;

            try {
                result = loader(arg1, readerParameters);
            } catch (Exception ex) {
                if (useSymbols) {
                    try {
                        result = loader(arg1, GetReaderParameters(false, mainAssemblyPath));
                        if (CouldNotLoadSymbols != null)
                            CouldNotLoadSymbols(arg1.ToString(), ex);
                    } catch (Exception ex2) {
                        if (CouldNotResolveAssembly != null)
                            CouldNotResolveAssembly(arg1.ToString(), ex2);
                    }
                } else {
                    if (CouldNotResolveAssembly != null)
                        CouldNotResolveAssembly(arg1.ToString(), ex);
                }
            }

            return result;
        }

        protected ParallelOptions GetParallelOptions () {
            return new ParallelOptions {
                MaxDegreeOfParallelism = Configuration.UseThreads.GetValueOrDefault(true) ? -1 : 1
            };
        }

        protected AssemblyDefinition[] LoadAssembly (string path, bool useSymbols, bool includeDependencies) {
            if (String.IsNullOrWhiteSpace(path))
                throw new InvalidDataException("Assembly path was empty.");

            var readerParameters = GetReaderParameters(useSymbols, path);

            var assembly = AssemblyLoadErrorWrapper(
                AssemblyDefinition.ReadAssembly,
                path, readerParameters, 
                useSymbols, path
            );
            if (assembly == null)
                throw new FileNotFoundException("Could not load the assembly '" + path + "'");

            var result = new List<AssemblyDefinition>();
            result.Add(assembly);

            if (AssemblyLoaded != null)
                AssemblyLoaded(path);

            if (includeDependencies) {
                var parallelOptions = GetParallelOptions();
                var modulesToVisit = new List<ModuleDefinition>(assembly.Modules);
                var assembliesToLoad = new List<AssemblyNameReference>();
                var visitedModules = new HashSet<string>();
                var assemblyNames = new HashSet<string>();

                while ((modulesToVisit.Count > 0) || (assembliesToLoad.Count > 0)) {
                    foreach (var module in modulesToVisit) {
                        if (visitedModules.Contains(module.FullyQualifiedName))
                            continue;

                        visitedModules.Add(module.FullyQualifiedName);

                        foreach (var reference in module.AssemblyReferences) {
                            bool ignored = false;
                            foreach (var ia in Configuration.Assemblies.Ignored) {
                                if (Regex.IsMatch(reference.FullName, ia, RegexOptions.IgnoreCase)) {
                                    ignored = true;
                                    break;
                                }
                            }

                            if (ignored)
                                continue;
                            if (assemblyNames.Contains(reference.FullName))
                                continue;

                            assemblyNames.Add(reference.FullName);
                            assembliesToLoad.Add(reference);
                        }
                    }
                    modulesToVisit.Clear();

                    Parallel.For(
                        0, assembliesToLoad.Count, parallelOptions, (i) => {
                            var anr = assembliesToLoad[i];

                            AssemblyDefinition refAssembly = null;
                            refAssembly = AssemblyLoadErrorWrapper(
                                readerParameters.AssemblyResolver.Resolve,
                                anr, readerParameters,
                                useSymbols, path
                            );

                            if (AssemblyLoaded != null)
                                AssemblyLoaded(refAssembly.MainModule.FullyQualifiedName);

                            if (refAssembly != null) {
                                lock (result)
                                    result.Add(refAssembly);

                                lock (modulesToVisit)
                                    modulesToVisit.AddRange(refAssembly.Modules);
                            }
                        }
                    );
                    assembliesToLoad.Clear();
                }
            }

            return result.ToArray();
        }

        protected DecompilerContext MakeDecompilerContext (ModuleDefinition module) {
            var context = new DecompilerContext(module);

            context.Settings.YieldReturn = false;
            context.Settings.AnonymousMethods = true;
            context.Settings.QueryExpressions = false;
            context.Settings.LockStatement = false;
            context.Settings.FullyQualifyAmbiguousTypeNames = true;
            context.Settings.ForEachStatement = false;

            return context;
        }

        public TranslationResult Translate (string assemblyPath, bool scanForProxies = true) {
            var result = new TranslationResult(this.Configuration);
            var assemblies = LoadAssembly(assemblyPath);
            var parallelOptions = GetParallelOptions();

            if (scanForProxies)
                _TypeInfoProvider.AddProxyAssemblies(assemblies);

            var pr = new ProgressReporter();
            if (Decompiling != null)
                Decompiling(pr);

            var methodsToAnalyze = new ConcurrentBag<MethodToAnalyze>();
            for (int i = 0; i < assemblies.Length; i++) {
                pr.OnProgressChanged(i, assemblies.Length * 2);
                GetMethodsToAnalyze(assemblies[i], methodsToAnalyze);
            }

            {
                int i = 0, mc = methodsToAnalyze.Count;
                Parallel.For(
                    0, methodsToAnalyze.Count, parallelOptions,
                    () => MakeDecompilerContext(assemblies[0].MainModule),
                    (_, loopState, ctx) => {
                        MethodToAnalyze m;
                        if (!methodsToAnalyze.TryTake(out m))
                            throw new InvalidDataException("Method collection mutated during analysis. Try setting UseThreads=false (and report an issue!)");

                        ctx.CurrentModule = m.MD.Module;
                        ctx.CurrentType = m.MD.DeclaringType;
                        ctx.CurrentMethod = m.MD;

                        TranslateMethodExpression(ctx, m.MD, m.MD, m.MI);

                        var j = Interlocked.Increment(ref i);
                        pr.OnProgressChanged(mc + j, mc * 2);

                        return ctx;
                    },
                    (ctx) => { }
                );
            }

            pr.OnFinished();

            OptimizeAll();

            pr = new ProgressReporter();
            if (Writing != null)
                Writing(pr);

            // Assign a unique identifier for all participating assemblies up front
            foreach (var assembly in assemblies)
                Manifest.GetPrivateToken(assembly);
            Manifest.AssignIdentifiers();

            Parallel.For(
                0, assemblies.Length, parallelOptions, (i) => {
                    var assembly = assemblies[i];
                    var outputPath = assembly.Name + ".js";

                    long existingSize;

                    if (!Manifest.GetExistingSize(assembly, out existingSize)) {
                        using (var outputStream = new MemoryStream()) {
                            var context = MakeDecompilerContext(assembly.MainModule);
                            Translate(context, assembly, outputStream);

                            var segment = new ArraySegment<byte>(
                                outputStream.GetBuffer(), 0, (int)outputStream.Length
                            );

                            result.AddFile(outputPath, segment);

                            Manifest.SetAlreadyTranslated(assembly, outputStream.Length);
                        }

                        lock (result.Assemblies)
                            result.Assemblies.Add(assembly);
                    } else {
                        Debug.WriteLine(String.Format("Skipping '{0}' because it is already translated...", assembly.Name));

                        result.AddExistingFile(outputPath, existingSize);
                    }

                    pr.OnProgressChanged(result.Assemblies.Count, assemblies.Length);
                }
            );

            pr.OnFinished();

            return result;
        }

        public static void GenerateManifest (AssemblyManifest manifest, string assemblyPath, TranslationResult result) {
            using (var ms = new MemoryStream())
            using (var tw = new StreamWriter(ms, new UTF8Encoding(false))) {
                tw.WriteLine("// {0} {1}", GetHeaderText(), Environment.NewLine);

                foreach (var kvp in manifest.Entries) {
                    tw.WriteLine(
                        "var {0} = JSIL.GetAssembly({1});",
                        kvp.Key, Util.EscapeString(kvp.Value, '\"')
                    );
                }

                tw.WriteLine("var $sig = new JSIL.MethodSignatureCache();");

                if (result.Configuration.GenerateContentManifest.GetValueOrDefault(true)) {
                    tw.WriteLine();
                    tw.WriteLine("if (typeof (contentManifest) !== \"object\") { contentManifest = {}; };");
                    tw.WriteLine("contentManifest[\"" + Path.GetFileName(assemblyPath).Replace("\\", "\\\\") + "\"] = [");

                    foreach (var fe in result.OrderedFiles) {
                        var propertiesObject = String.Format("{{ \"sizeBytes\": {0} }}", fe.Size);

                        tw.WriteLine(String.Format(
                            "  [\"{0}\", \"{1}\", {2}],",
                            "Script", fe.Filename.Replace("\\", "/"), propertiesObject
                        ));
                    }

                    tw.WriteLine("];");
                }

                tw.Flush();

                result.Manifest = new ArraySegment<byte>(
                    ms.GetBuffer(), 0, (int)ms.Length
                );
            }
        }

        protected void OptimizeAll () {
            var pr = new ProgressReporter();
            if (Optimizing != null)
                Optimizing(pr);

            int i = 0;
            QualifiedMemberIdentifier id;
            while (FunctionCache.OptimizationQueue.TryDequeue(out id)) {
                var e = FunctionCache.GetCacheEntry(id);
                if (e.Expression == null) {
                    i++;
                    continue;
                }

                pr.OnProgressChanged(i++, i + FunctionCache.OptimizationQueue.Count);
                OptimizeFunction(e.SpecialIdentifiers, e.ParameterNames, e.Variables, e.Expression);
            }

            pr.OnFinished();
        }

        private void GetMethodsToAnalyze (AssemblyDefinition assembly, ConcurrentBag<MethodToAnalyze> allMethods) {
            bool isStubbed = IsStubbed(assembly);

            var parallelOptions = GetParallelOptions();
            var allTypes = new List<TypeDefinition>();

            foreach (var module in assembly.Modules) {
                var moduleInfo = _TypeInfoProvider.GetModuleInformation(module);
                if (moduleInfo.IsIgnored)
                    continue;

                allTypes.AddRange(module.Types);
            }

            while (allTypes.Count > 0) {
                var types = new HashSet<TypeDefinition>(allTypes).ToArray();
                allTypes.Clear();

                Parallel.For(
                    0, types.Length, parallelOptions,
                    () => {
                        return new List<TypeDefinition>();
                    },
                    (i, loopState, typeList) => {
                        var type = types[i];

                        typeList.AddRange(type.NestedTypes);

                        if (!ShouldTranslateMethods(type))
                            return typeList;

                        IEnumerable<MethodDefinition> methods = type.Methods;

                        var typeInfo = _TypeInfoProvider.GetExisting(type);
                        if (typeInfo != null) {
                            if (typeInfo.StaticConstructor != null) {
                                methods = methods.Concat(new[] { typeInfo.StaticConstructor });
                            }

                            foreach (var esc in typeInfo.ExtraStaticConstructors) {
                                allMethods.Add(new MethodToAnalyze(esc));
                            }
                        }

                        foreach (var m in methods) {
                            if (!m.HasBody)
                                continue;

                            var mi = _TypeInfoProvider.GetMethod(m);

                            if ((mi == null) || (mi.IsIgnored))
                                continue;

                            if (isStubbed) {
                                var isProperty = mi.DeclaringProperty != null;

                                if (!(isProperty && m.IsCompilerGenerated()))
                                    continue;
                            }

                            allMethods.Add(new MethodToAnalyze(m));
                        }

                        return typeList;
                    },
                    (typeList) => {
                        lock (allTypes)
                            allTypes.AddRange(typeList);
                    }
                );
            }
        }

        protected bool IsStubbed (AssemblyDefinition assembly) {
            bool stubbed = false;
            foreach (var sa in Configuration.Assemblies.Stubbed) {
                if (Regex.IsMatch(assembly.FullName, sa, RegexOptions.IgnoreCase)) {
                    return true;
                    break;
                }
            }

            return false;
        }

        public static string GetHeaderText () {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return String.Format(
                "Generated by JSIL v{0}.{1}.{2} build {3}. See http://jsil.org/ for more information.",
                version.Major, version.Minor, version.Build, version.Revision
            );
        }

        protected void Translate (DecompilerContext context, AssemblyDefinition assembly, Stream outputStream) {
            bool stubbed = IsStubbed(assembly);

            var tw = new StreamWriter(outputStream, Encoding.ASCII);
            var formatter = new JavascriptFormatter(
                tw, this._TypeInfoProvider, Manifest, assembly, Configuration, stubbed
            );

            formatter.Comment(GetHeaderText());
            formatter.NewLine();

            if (stubbed) {
                if (Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false)) {
                    formatter.Comment("Generating type skeletons");
                } else {
                    formatter.Comment("Generating type stubs only");
                }
                formatter.NewLine();
            }

            formatter.DeclareAssembly();
            formatter.NewLine();

            var sealedTypes = new HashSet<TypeDefinition>();
            var declaredTypes = new HashSet<TypeDefinition>();

            foreach (var module in assembly.Modules) {
                if (module.Assembly != assembly) {
                    WarningFormat("Warning: Mono.Cecil failed to correctly load the module '{0}'. Skipping it.", module);
                    continue;
                }

                TranslateModule(context, formatter, module, sealedTypes, declaredTypes, stubbed);
            }

            tw.Flush();
        }

        protected void TranslateModule (
            DecompilerContext context, JavascriptFormatter output, ModuleDefinition module, 
            HashSet<TypeDefinition> sealedTypes, HashSet<TypeDefinition> declaredTypes, bool stubbed
        ) {
            var moduleInfo = _TypeInfoProvider.GetModuleInformation(module);
            if (moduleInfo.IsIgnored)
                return;

            context.CurrentModule = module;

            var js = new JSSpecialIdentifiers(FunctionCache.MethodTypes, context.CurrentModule.TypeSystem);
            var jsil = new JSILIdentifier(FunctionCache.MethodTypes, context.CurrentModule.TypeSystem, js);

            var astEmitter = new JavascriptAstEmitter(
                output, jsil, context.CurrentModule.TypeSystem, this._TypeInfoProvider
            );

            foreach (var typedef in module.Types)
                DeclareType(context, typedef, astEmitter, output, declaredTypes, stubbed);
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
                (from p in iface.GenericParameters select p.Name), null, ListValueType.Primitive
            );
            output.CloseBracket();

            output.Comma();
            output.OpenFunction(null, (f) =>
            {
                f.Identifier("$");
            });

            var refContext = new TypeReferenceContext {
                EnclosingType = iface,
                DefiningType = iface
            };

            bool isFirst = true;
            foreach (var methodGroup in iface.Methods.GroupBy(md => md.Name)) {
                foreach (var m in methodGroup) {
                    var methodInfo = _TypeInfoProvider.GetMethod(m);
                    if ((methodInfo != null) && methodInfo.IsIgnored)
                        continue;

                    output.Identifier("$", null);
                    output.Dot();
                    output.Identifier("Method", null);
                    output.LPar();

                    output.WriteRaw("{}");
                    output.Comma();

                    output.Value(Util.EscapeIdentifier(m.Name, EscapingMode.String));
                    output.Comma();

                    output.MethodSignature(m, methodInfo.Signature, refContext);

                    output.RPar();
                    output.Semicolon(true);
                }
            }

            foreach (var p in iface.Properties) {
                var propertyInfo = _TypeInfoProvider.GetProperty(p);
                if ((propertyInfo != null) && propertyInfo.IsIgnored)
                    continue;

                output.Identifier("$", null);
                output.Dot();
                output.Identifier("Property", null);
                output.LPar();

                output.WriteRaw("{}");
                output.Comma();

                output.Value(Util.EscapeIdentifier(p.Name, EscapingMode.String));

                output.RPar();
                output.Semicolon(true);
            }

            output.CloseBrace(false);

            output.Comma();

            refContext = new TypeReferenceContext {
                EnclosingType = iface.DeclaringType,
                DefiningType = iface
            };

            output.OpenBracket();
            foreach (var i in iface.Interfaces) {
                if (!isFirst) {
                    output.Comma();
                }

                output.TypeReference(i, refContext);

                isFirst = false;
            }
            output.CloseBracket();

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void TranslateEnum (DecompilerContext context, JavascriptFormatter output, TypeDefinition enm) {
            var typeInfo = _TypeInfoProvider.GetTypeInformation(enm);

            output.Identifier("JSIL.MakeEnum", null);
            output.LPar();
            output.NewLine();

            output.Value(Util.EscapeIdentifier(typeInfo.FullName, EscapingMode.String));
            output.Comma();

            output.Value(enm.IsPublic);
            output.Comma();

            output.OpenBrace();

            var typeInformation = _TypeInfoProvider.GetTypeInformation(enm);
            if (typeInformation == null)
                throw new InvalidDataException(String.Format(
                    "No type information for enum '{0}'!",
                    enm.FullName
                ));

            bool isFirst = true;
            foreach (var em in typeInformation.EnumMembers.Values.OrderBy((em) => em.Value)) {
                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Identifier(em.Name);
                output.WriteRaw(": ");
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
            output.Comma();

            output.Value(del.IsPublic);

            output.Comma();
            output.OpenBracket();
            if (del.HasGenericParameters)
                output.CommaSeparatedList(
                    (from p in del.GenericParameters select p.Name), null, ListValueType.Primitive
                );
            output.CloseBracket();

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected virtual bool ShouldGenerateTypeDeclaration (TypeDefinition typedef, bool makingSkeletons) {
            if (TypeDeclarationsToSuppress.Contains(typedef.FullName) && !makingSkeletons)
                return false;

            return true;
        }

        protected void DeclareType (
            DecompilerContext context, TypeDefinition typedef, 
            JavascriptAstEmitter astEmitter, JavascriptFormatter output, 
            HashSet<TypeDefinition> declaredTypes, bool stubbed
        ) {
            var makingSkeletons = stubbed && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false);

            var typeInfo = _TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return;

            if (declaredTypes.Contains(typedef))
                return;

            // This type is defined in JSIL.Core so we don't want to cause a name collision.
            if (!ShouldGenerateTypeDeclaration(typedef, makingSkeletons)) {
                declaredTypes.Add(typedef);

                output.WriteRaw("JSIL.MakeTypeAlias");
                output.LPar();

                output.WriteRaw("$jsilcore");
                output.Comma();

                output.Value(typedef.FullName);

                output.RPar();
                output.Semicolon();
                output.NewLine();

                return;
            }

            TypeReference oldEnclosing = astEmitter.ReferenceContext.EnclosingType;
            TypeReference oldDefining = astEmitter.ReferenceContext.DefiningType;
            astEmitter.ReferenceContext.DefiningType = typedef;
            context.CurrentType = typedef;

            try {
                declaredTypes.Add(typedef);

                // type has a JS replacement, we can't correctly emit a stub or definition for it.
                // We do want to process nested types and the type definition, though.
                if (typeInfo.Replacement != null) {
                    TranslateTypeDefinition(
                        context, typedef, astEmitter, output, stubbed,
                        (o) => o.WriteRaw(typeInfo.Replacement), makingSkeletons
                    );

                    output.NewLine();

                    oldEnclosing = astEmitter.ReferenceContext.EnclosingType;
                    astEmitter.ReferenceContext.EnclosingType = typedef;

                    try {
                        foreach (var nestedTypeDef in typedef.NestedTypes)
                            DeclareType(context, nestedTypeDef, astEmitter, output, declaredTypes, stubbed);
                    } finally {
                        astEmitter.ReferenceContext.EnclosingType = oldEnclosing;
                    }

                    return;
                }

                output.DeclareNamespace(typedef.Namespace);

                if (typeInfo.IsExternal) {
                    output.Identifier("JSIL.MakeExternalType", null);
                    output.LPar();

                    output.Value(typeInfo.FullName);
                    output.Comma();
                    output.Value(typedef.IsPublic);

                    output.RPar();
                    output.Semicolon();
                    output.NewLine();
                    return;
                }

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
                if (declaringType != null)
                    DeclareType(context, declaringType, astEmitter, output, declaredTypes, IsStubbed(declaringType.Module.Assembly));

                var baseClass = typedef.BaseType;
                if (baseClass != null) {
                    var resolved = baseClass.Resolve();
                    if (
                        (resolved != null) &&
                        (resolved.Module.Assembly == typedef.Module.Assembly)
                    ) {
                        DeclareType(context, resolved, astEmitter, output, declaredTypes, IsStubbed(resolved.Module.Assembly));
                    }
                }

                bool isStatic = typedef.IsAbstract && typedef.IsSealed;

                if (makingSkeletons) {
                    output.Identifier("JSIL.ImplementExternals", null);
                    output.LPar();

                    output.Value(typeInfo.FullName);

                } else if (isStatic) {
                    output.Identifier("JSIL.MakeStaticClass", null);
                    output.LPar();

                    output.Value(typeInfo.FullName);
                    output.Comma();
                    output.Value(typedef.IsPublic);

                    output.Comma();
                    output.OpenBracket();
                    if (typedef.HasGenericParameters)
                        output.CommaSeparatedList(
                            (from p in typedef.GenericParameters select p.Name), astEmitter.ReferenceContext, ListValueType.Primitive
                        );
                    output.CloseBracket();

                } else {
                    if (typedef.IsValueType)
                        output.Identifier("JSIL.MakeStruct", null);
                    else
                        output.Identifier("JSIL.MakeClass", null);

                    output.LPar();

                    if (baseClass == null) {
                        if (typedef.FullName != "System.Object") {
                            throw new InvalidDataException(String.Format(
                                "Type '{0}' has no base class and isn't System.Object.",
                                typedef.FullName
                            ));
                        }

                        output.Identifier("$jsilcore");
                        output.Dot();
                        output.Identifier("System");
                        output.Dot();
                        output.Identifier("Object");
                    } else if (typedef.FullName == "System.ValueType") {
                        output.Identifier("$jsilcore");
                        output.Dot();
                        output.Identifier("System");
                        output.Dot();
                        output.Identifier("ValueType");
                    } else {
                        output.TypeReference(baseClass, astEmitter.ReferenceContext);
                    }

                    output.Comma();

                    output.Value(typeInfo.FullName);
                    output.Comma();
                    output.Value(typedef.IsPublic);

                    output.Comma();
                    output.OpenBracket();
                    if (typedef.HasGenericParameters)
                        output.CommaSeparatedList(
                            (from p in typedef.GenericParameters select p.Name), astEmitter.ReferenceContext, ListValueType.Primitive
                        );
                    output.CloseBracket();

                }

                astEmitter.ReferenceContext.EnclosingType = typedef;
                try {
                    // Hack to force the indent level for type definitions to be 1 instead of 2.
                    output.Unindent();

                    output.Comma();
                    output.OpenFunction(null, (f) => {
                        f.Identifier("$");
                    });

                    TranslateTypeDefinition(
                        context, typedef, astEmitter, output, stubbed,
                        (o) => o.Identifier("$", null), makingSkeletons
                    );

                    output.NewLine();

                    output.CloseBrace(false);

                    // Hack to force the indent level for type definitions to be 1 instead of 2.
                    output.Indent();

                    output.RPar();
                    output.Semicolon();
                    output.NewLine();
                } finally {
                    astEmitter.ReferenceContext.EnclosingType = oldEnclosing;
                }

                foreach (var nestedTypeDef in typedef.NestedTypes)
                    DeclareType(context, nestedTypeDef, astEmitter, output, declaredTypes, stubbed);
            } finally {
                astEmitter.ReferenceContext.DefiningType = oldDefining;
            }
        }

        protected bool ShouldTranslateMethods (TypeDefinition typedef) {
            var typeInfo = _TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy || typeInfo.IsExternal)
                return false;

            if (typedef.IsInterface)
                return false;
            else if (typedef.IsEnum)
                return false;
            else if (typeInfo.IsDelegate)
                return false;

            return true;
        }

        protected void TranslatePrimitiveDefinition (
            DecompilerContext context, JavascriptFormatter output,
            TypeDefinition typedef, bool stubbed, Action<JavascriptFormatter> dollar
        ) {
            bool isIntegral = false;
            bool isNumeric = false;

            switch (typedef.FullName) {
                case "System.Boolean":
                    isIntegral = true;
                    isNumeric = true;
                    break;
                case "System.Char":
                    isIntegral = true;
                    isNumeric = true;
                    break;
                case "System.Byte":
                case "System.SByte":
                case "System.UInt16":
                case "System.Int16":
                case "System.UInt32":
                case "System.Int32":
                case "System.UInt64":
                case "System.Int64":
                    isIntegral = true;
                    isNumeric = true;
                    break;
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    isIntegral = false;
                    isNumeric = true;
                    break;
            }

            var setValue = (Action<string, bool>)((name, value) => {
                dollar(output);
                output.Dot();
                output.Identifier("SetValue", null);
                output.LPar();
                output.Value(name);
                output.Comma();
                output.Value(value);
                output.RPar();
                output.Semicolon(true);
            });

            setValue("__IsNativeType__", true);
            setValue("__IsIntegral__", isIntegral);
            setValue("__IsNumeric__", isNumeric);
        }

        protected void TranslateTypeDefinition (
            DecompilerContext context, TypeDefinition typedef, 
            JavascriptAstEmitter astEmitter, JavascriptFormatter output, 
            bool stubbed, Action<JavascriptFormatter> dollar, bool makingSkeletons
        ) {
            var typeInfo = _TypeInfoProvider.GetTypeInformation(typedef);
            if (!ShouldTranslateMethods(typedef))
                return;

            context.CurrentType = typedef;

            if (typedef.IsPrimitive)
                TranslatePrimitiveDefinition(context, output, typedef, stubbed, dollar);

            foreach (var method in typedef.Methods.OrderBy((md) => md.Name)) {
                // We translate the static constructor explicitly later, and inject field initialization
                if (method.Name == ".cctor")
                    continue;

                TranslateMethod(
                    context, method, method, astEmitter, output, 
                    stubbed, dollar
                );
            }

            Action initializeOverloadsAndProperties = () => {
                foreach (var property in typedef.Properties.OrderBy((p) => p.Name))
                    TranslateProperty(context, output, property, dollar);
            };

            Func<TypeReference, bool> isInterfaceIgnored = (i) => {
                var interfaceInfo = _TypeInfoProvider.GetTypeInformation(i);
                if (interfaceInfo != null)
                    return interfaceInfo.IsIgnored;
                else
                    return true;
            };

            Func<FieldDefinition, bool> isFieldIgnored = (f) => {
                IMemberInfo memberInfo;
                if (typeInfo.Members.TryGetValue(MemberIdentifier.New(this._TypeInfoProvider, f), out memberInfo))
                    return memberInfo.IsIgnored;
                else
                    return true;
            };

            if (!makingSkeletons)
                TranslateTypeStaticConstructor(
                    context, typedef, astEmitter, 
                    output, typeInfo.StaticConstructor, 
                    stubbed, dollar
                );

            if (!makingSkeletons && ((typeInfo.MethodGroups.Count + typedef.Properties.Count) > 0)) {
                initializeOverloadsAndProperties();
            }

            var interfaces = (from i in typeInfo.Interfaces
                              where !i.Item1.IsIgnored
                              select i.Item2).ToArray();

            if (!makingSkeletons && (interfaces.Length > 0)) {
                output.NewLine();

                dollar(output);
                output.Dot();
                output.Identifier("ImplementInterfaces", null);
                output.LPar();
                output.CommaSeparatedList(interfaces, astEmitter.ReferenceContext, ListValueType.TypeReference);
                output.RPar();
            }
        }

        internal JSFunctionExpression TranslateMethodExpression (
            DecompilerContext context, MethodReference method, 
            MethodDefinition methodDef, MethodInfo methodInfo = null
        ) {
            var oldMethod = context.CurrentMethod;
            try {
                if (method == null)
                    throw new ArgumentNullException("method");
                if (methodDef == null)
                    throw new ArgumentNullException("methodDef");

                if (methodInfo == null)
                    methodInfo = _TypeInfoProvider.GetMemberInformation<JSIL.Internal.MethodInfo>(methodDef);

                if (methodInfo == null)
                    throw new InvalidDataException(String.Format(
                        "Method '{0}' has no method information!",
                        method.FullName
                    ));

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
                if ((methodDef.Body.CodeSize > LargeMethodThreshold) && (this.DecompilingMethod != null))
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

                IEnumerable<ILVariable> allVariables = GetAllVariablesForMethod(context, decompiler.Parameters, ilb);
                if (allVariables == null)
                    return null;

                var translator = new ILBlockTranslator(
                    this, context, method, methodDef, 
                    ilb, decompiler.Parameters, allVariables
                );

                JSBlockStatement body = null;
                try {
                    body = translator.Translate();
                } catch (Exception exc) {
                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(bodyDef.FullName, exc);

                    body = null;
                }

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

        internal static ILVariable[] GetAllVariablesForMethod(
            DecompilerContext context, IEnumerable<ILVariable> parameters, ILBlock methodBody
        ) {
            var allVariables = methodBody.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
                .Where(v => v != null && !v.IsParameter).Distinct().ToArray();

            foreach (var v in allVariables) {
                if (TypeUtil.IsIgnoredType(v.Type)) {
                    return null; // return null;
                }
            }

            NameVariables.AssignNamesToVariables(context, parameters, allVariables, methodBody);

            return allVariables;
        }

        private void OptimizeFunction (
            SpecialIdentifiers si, HashSet<string> parameterNames,
            Dictionary<string, JSVariable> variables, JSFunctionExpression function
        ) {
            Action temporaryEliminationPass = () => {
                if (Configuration.Optimizer.EliminateTemporaries.GetValueOrDefault(true)) {
                    bool eliminated;
                    do {
                        var visitor = new EliminateSingleUseTemporaries(
                            si.TypeSystem, variables, FunctionCache
                        );
                        visitor.Visit(function);
                        eliminated = visitor.EliminatedVariables.Count > 0;
                    } while (eliminated);
                }
            };

            var la = new LabelAnalyzer();
            la.Visit(function);
            la.BuildLabelGroups();

            temporaryEliminationPass();

            new EmulateStructAssignment(
                si.TypeSystem,
                FunctionCache,
                si.CLR,
                Configuration.Optimizer.EliminateStructCopies.GetValueOrDefault(true)
            ).Visit(function);

            new IntroduceVariableDeclarations(
                variables,
                _TypeInfoProvider
            ).Visit(function);

            new IntroduceVariableReferences(
                si.JSIL,
                variables,
                parameterNames
            ).Visit(function);

            if (Configuration.Optimizer.SimplifyLoops.GetValueOrDefault(true))
                new SimplifyLoops(
                    si.TypeSystem, false
                ).Visit(function);

            // Temporary elimination makes it possible to simplify more operators, so do it later
            if (Configuration.Optimizer.SimplifyOperators.GetValueOrDefault(true))
                new SimplifyOperators(
                    si.JSIL, si.JS, si.TypeSystem
                ).Visit(function);

            new ReplaceMethodCalls(
                function.Method.Reference,
                si.JSIL, si.JS, si.TypeSystem
            ).Visit(function);

            new IntroduceCharCasts(
                si.TypeSystem, si.JS
            ).Visit(function);

            new IntroduceEnumCasts(
                si.TypeSystem, si.JS, _TypeInfoProvider, FunctionCache.MethodTypes
            ).Visit(function);

            new ExpandCastExpressions(
                si.TypeSystem, si.JS, si.JSIL, _TypeInfoProvider
            ).Visit(function);
            
            // We need another operator simplification pass to simplify expressions created by cast expressions
            if (Configuration.Optimizer.SimplifyOperators.GetValueOrDefault(true))
                new SimplifyOperators(
                    si.JSIL, si.JS, si.TypeSystem
                ).Visit(function);

            var dss = new DeoptimizeSwitchStatements(
                si.TypeSystem
            );
            dss.Visit(function);

            new CollapseNulls().Visit(function);

            if (Configuration.Optimizer.SimplifyLoops.GetValueOrDefault(true))
                new SimplifyLoops(
                    si.TypeSystem, true
                ).Visit(function);

            temporaryEliminationPass();

            if (Configuration.Optimizer.EliminateRedundantControlFlow.GetValueOrDefault(true))
                new ControlFlowSimplifier().Visit(function);

            var lnd = new LoopNameDetector();
            lnd.Visit(function);
            lnd.EliminateUnusedLoopNames();

            new ExpandCastExpressions(
                si.TypeSystem, si.JS, si.JSIL, _TypeInfoProvider
            ).Visit(function);
        }

        protected static bool NeedsStaticConstructor (TypeReference type) {
            if (TypeUtil.IsStruct(type))
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

        protected JSExpression TranslateField (
            FieldDefinition field, Dictionary<FieldDefinition, JSExpression> defaultValues, 
            bool cctorContext, Action<JavascriptFormatter> dollar
        ) {
            var fieldInfo = _TypeInfoProvider.GetMemberInformation<Internal.FieldInfo>(field);
            if ((fieldInfo == null) || fieldInfo.IsIgnored || fieldInfo.IsExternal)
                return null;

            var dollarIdentifier = new JSRawOutputIdentifier(dollar, field.DeclaringType);
            var descriptor = new JSMemberDescriptor(
                field.IsPublic, field.IsStatic
            );

            var fieldName = Util.EscapeIdentifier(fieldInfo.Name, EscapingMode.MemberIdentifier);

            if (field.HasConstant) {
                JSLiteral constant;
                if (field.Constant == null) {
                    constant = JSLiteral.Null(field.FieldType);
                } else {
                    constant = JSLiteral.New(field.Constant as dynamic);
                }

                return JSInvocationExpression.InvokeStatic(
                    JSDotExpression.New(
                        dollarIdentifier, new JSFakeMethod("Constant", field.Module.TypeSystem.Void, null, FunctionCache.MethodTypes)
                    ), new JSExpression[] {
                        descriptor, JSLiteral.New(fieldName), constant
                    }
                );
            } else {
                bool forCctor = false;
                if (field.IsStatic && NeedsStaticConstructor(field.FieldType))
                    forCctor = true;
                else if (TypeUtil.IsStruct(field.FieldType))
                    forCctor = true;

                JSExpression defaultValue;
                if (!defaultValues.TryGetValue(field, out defaultValue))
                    defaultValue = null;

                JSExpression fieldTypeExpression = new JSTypeReference(field.FieldType, field.DeclaringType);

                if (cctorContext != forCctor)
                    defaultValue = null;

                if (defaultValue is JSDefaultValueLiteral)
                    defaultValue = null;

                if (!cctorContext && !field.IsStatic) {
                    // Non-static fields' default values may contain expressions like 'this.T' which are impossible to
                    //  support correctly in this context. Leave the default value up to the ctor(s).
                    defaultValue = null;
                } else if (
                    !cctorContext && 
                    (defaultValue != null) &&
                    (
                        defaultValue.HasGlobalStateDependency || 
                        !defaultValue.IsConstant ||
                        TypeUtil.IsStruct(defaultValue.GetActualType(field.Module.TypeSystem)) ||
                        defaultValue is JSNewExpression ||
                        defaultValue is JSArrayExpression ||
                        defaultValue is JSInvocationExpressionBase ||
                        defaultValue is JSNewArrayExpression
                    )
                ) {
                    // We have to represent the default value as a callable function, taking a single
                    //  argument that represents the public interface, so that recursive field initializations
                    //  will work correctly. InterfaceBuilder.Field will invoke this function for us.

                    defaultValue = new JSFunctionExpression(
                        // No method or variables. This could break things.
                        null, null, 
                        // We redefine $ within the function so that it points to the public interface,
                        //  instead of pointing to the outside interface builder.
                        new JSVariable[] { new JSParameter("$", field.DeclaringType, null) },
                        new JSBlockStatement(
                            new JSExpressionStatement(new JSReturnExpression(defaultValue))
                        ),
                        FunctionCache.MethodTypes
                    );
                }

                if (cctorContext) {
                    JSExpression thisParameter;
                    if (field.IsStatic)
                        thisParameter = new JSType(field.DeclaringType);
                    else
                        thisParameter = new JSThisParameter(field.DeclaringType, null);

                    if (defaultValue == null)
                        defaultValue = new JSDefaultValueLiteral(field.FieldType);

                    return new JSBinaryOperatorExpression(
                        JSBinaryOperator.Assignment,
                        new JSFieldAccess(
                            thisParameter,
                            new JSField(field, fieldInfo)
                        ),
                        defaultValue,
                        field.FieldType
                    );
                } else {
                    JSExpression[] args;
                    if (defaultValue != null) {
                        args = new JSExpression[] {
                            descriptor, JSLiteral.New(fieldName), fieldTypeExpression, defaultValue
                        };
                    } else {
                        args = new JSExpression[] {
                            descriptor, JSLiteral.New(fieldName), fieldTypeExpression
                        };
                    }

                    return JSInvocationExpression.InvokeStatic(
                        JSDotExpression.New(
                            dollarIdentifier, new JSFakeMethod("Field", field.Module.TypeSystem.Void, null, FunctionCache.MethodTypes)
                        ), args
                    );
                }
            }
        }

        protected void TranslateTypeStaticConstructor (
            DecompilerContext context, TypeDefinition typedef, 
            JavascriptAstEmitter astEmitter, JavascriptFormatter output, 
            MethodDefinition cctor, bool stubbed, Action<JavascriptFormatter> dollar
        ) {
            var typeInfo = _TypeInfoProvider.GetTypeInformation(typedef);
            var typeSystem = context.CurrentModule.TypeSystem;
            var staticFields = 
                (from f in typedef.Fields
                 where f.IsStatic
                 select f).ToArray();
            var fieldsToEmit =
                (from f in staticFields
                 where NeedsStaticConstructor(f.FieldType)
                 let fi = _TypeInfoProvider.GetField(f)
                 where ((fi != null) && (!fi.IsExternal && !fi.IsIgnored)) || (fi == null)
                 select f).ToArray();
            var fieldsToStrip =
                new HashSet<FieldDefinition>(from f in staticFields
                 let fi = _TypeInfoProvider.GetField(f)
                 where (fi != null) && (fi.IsExternal || fi.IsIgnored)
                 select f);

            // For fields with values assigned non-dynamically by the static constructor, we want to pull those values
            //  out of the static constructor and assign them ourselves. This ensures that these effective constants are
            //  carried over even if the static constructor (and other methods) are ignored/external.

            var fieldDefaults = new Dictionary<FieldDefinition, JSExpression>();

            // It's possible for a proxy to replace the cctor, so we need to pull default values
            //  from the real cctor (if the type has one)
            var realCctor = typedef.Methods.FirstOrDefault((m) => m.Name == ".cctor");
            if ((realCctor != null) && (realCctor.HasBody)) {
                // Do the simplest possible IL disassembly of the static cctor, 
                //  because all we're looking for is static field assignments.
                var ctx = new DecompilerContext(realCctor.Module) {
                    CurrentMethod = realCctor,
                    CurrentType = realCctor.DeclaringType
                };

                var astBuilder = new ILAstBuilder();
                var block = new ILBlock(astBuilder.Build(realCctor, true, ctx));

                // We need to run the optimizer on the method to strip out the
                //  temporary locals created by field assignments.
                var optimizer = new ILAstOptimizer();
                // Save time by not running all the optimization stages.
                // Since we're generating an AST for *every* static constructor in the entire type graph,
                //  this adds up.
                optimizer.Optimize(ctx, block, ILAstOptimizationStep.SimplifyShortCircuit);

                // We need the set of variables used by the method in order to
                //  properly map default values.
                var variables = GetAllVariablesForMethod(
                    context, astBuilder.Parameters, block
                );
                if (variables != null) {
                    // We need a translator to map the IL expressions for the default
                    //  values into JSAst expressions.
                    var translator = new ILBlockTranslator(
                        this, ctx, realCctor, realCctor, block, astBuilder.Parameters, variables
                    );

                    // We may end up with nested blocks since we didn't run all the optimization passes.
                    var blocks = block.GetSelfAndChildrenRecursive<ILBasicBlock>();
                    foreach (var b in blocks) {

                        foreach (var node in b.Body) {
                            var ile = node as ILExpression;
                            if (ile == null)
                                continue;

                            if (ile.Code != ILCode.Stsfld)
                                continue;

                            var targetField = ile.Operand as FieldDefinition;
                            if (targetField == null)
                                continue;

                            if (targetField.DeclaringType != realCctor.DeclaringType)
                                continue;

                            var expectedType = ile.Arguments[0].ExpectedType;

                            // If the field's value is of an ignored type then we ignore the initialization since it probably won't translate anyway.
                            if (TypeUtil.IsIgnoredType(expectedType))
                                continue;

                            JSExpression defaultValue = null;

                            try {
                                defaultValue = translator.TranslateNode(ile.Arguments[0]);
                            } catch (Exception ex) {
                                WarningFormat("Warning: failed to translate default value for static field '{0}': {1}", targetField, ex);
                                continue;
                            }

                            if (defaultValue == null)
                                continue;

                            try {
                                // TODO: Expand this to include 'new X' expressions that are effectively constant, by using static analysis to ensure that
                                //  the new-expression doesn't have any global state dependencies and doesn't perform mutation.

                                var newArray = defaultValue as JSNewArrayExpression;

                                if (
                                    (newArray != null) && (
                                        (newArray.SizeOrArrayInitializer == null) ||
                                        (newArray.SizeOrArrayInitializer.IsConstant)
                                    )
                                )
                                    ;
                                else if (!defaultValue.IsConstant)
                                    continue;
                            } catch (Exception ex) {
                                // This may fail because we didn't do a full translation.
                                WarningFormat("Warning: failed to translate default value for static field '{0}': {1}", targetField, ex);
                                continue;
                            }

                            var typeReferences = defaultValue.AllChildrenRecursive.OfType<JSType>();
                            foreach (var typeReference in typeReferences) {
                                if (TypeUtil.TypesAreEqual(typeReference.Type, realCctor.DeclaringType))
                                    defaultValue.ReplaceChildRecursive(typeReference, new JSStringIdentifier("$", realCctor.DeclaringType));
                            }

                            var es = new JSExpressionStatement(defaultValue);
                            var ece = new ExpandCastExpressions(
                                translator.TypeSystem, translator.SpecialIdentifiers.JS, translator.SpecialIdentifiers.JSIL, translator.TypeInfo
                            );
                            ece.Visit(es);

                            fieldDefaults[targetField] = es.Expression;
                        }
                    }
                }
            }

            // We initialize all static fields in the cctor to avoid ordering issues
            Action<JSFunctionExpression> fixupCctor = (f) => {
                int insertPosition = 0;

                // Strip initializations of ignored and external fields from the cctor, since
                //  they are generated by the compiler
                var statements = f.Body.Children.OfType<JSExpressionStatement>().ToArray();
                foreach (var es in statements) {
                    var boe = es.Expression as JSBinaryOperatorExpression;
                    if (boe == null)
                        continue;

                    var fieldAccess = boe.Left as JSFieldAccess;
                    if (fieldAccess == null)
                        continue;

                    if (!fieldsToStrip.Contains(fieldAccess.Field.Field.Member))
                        continue;

                    // We simply strip the initialization, which leaves the field undefined at runtime.
                    // TODO: It might be be better to generate an external method style placeholder here.
                    f.Body.Statements.Remove(es);
                }

                // Generate field initializations that were not generated by the compiler
                foreach (var field in fieldsToEmit) {
                    var expr = TranslateField(field, fieldDefaults, true, dollar);

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

            foreach (var f in typedef.Fields.OrderBy((f) => f.Name)) {
                var fi = _TypeInfoProvider.GetField(f);
                if ((fi != null) && (fi.IsIgnored || fi.IsExternal))
                    continue;

                var expr = TranslateField(f, fieldDefaults, false, dollar);

                if (expr != null) {
                    output.NewLine();
                    astEmitter.Visit(new JSExpressionStatement(expr));
                }
            }

            if ((cctor != null) && !stubbed) {
                TranslateMethod(context, cctor, cctor, astEmitter, output, false, dollar, null, fixupCctor);
            } else if (fieldsToEmit.Length > 0) {
                var fakeCctor = new MethodDefinition(".cctor", Mono.Cecil.MethodAttributes.Static, typeSystem.Void);
                fakeCctor.DeclaringType = typedef;

                typeInfo.StaticConstructor = fakeCctor;
                var identifier = MemberIdentifier.New(this._TypeInfoProvider, fakeCctor);

                lock (typeInfo.Members)
                    typeInfo.Members[identifier] = new Internal.MethodInfo(
                        typeInfo, identifier, fakeCctor, new ProxyInfo[0], false
                    );

                // Generate the fake constructor, since it wasn't created during the analysis pass
                TranslateMethodExpression(context, fakeCctor, fakeCctor);

                TranslateMethod(context, fakeCctor, fakeCctor, astEmitter, output, false, dollar, null, fixupCctor);
            }

            foreach (var extraCctor in typeInfo.ExtraStaticConstructors) {
                var declaringType = extraCctor.Member.DeclaringType;
                var newJSType = new JSType(typedef);

                TranslateMethod(
                    context, extraCctor.Member, extraCctor.Member, astEmitter,
                    output, false, dollar, extraCctor,
                    // The static constructor may have references to the proxy type that declared it.
                    //  If so, replace them with references to the target type.
                    (fn) => {
                        var types = fn.AllChildrenRecursive.OfType<JSType>();

                        foreach (var t in types) {
                            if (TypeUtil.TypesAreEqual(t.Type, declaringType))
                                fn.ReplaceChildRecursive(t, newJSType);
                        }
                    }
                );
            }
        }

        protected void TranslateMethod (
            DecompilerContext context, MethodReference methodRef, MethodDefinition method,
            JavascriptAstEmitter astEmitter, JavascriptFormatter output, bool stubbed, 
            Action<JavascriptFormatter> dollar, MethodInfo methodInfo = null,
            Action<JSFunctionExpression> bodyTransformer = null
        ) {
            if (methodInfo == null) {
                methodInfo = _TypeInfoProvider.GetMemberInformation<Internal.MethodInfo>(method);
            }

            if (methodInfo == null)
                return;

            bool isReplaced = methodInfo.Metadata.HasAttribute("JSIL.Meta.JSReplacement");
            bool methodIsProxied = (methodInfo.IsFromProxy && methodInfo.Member.HasBody) && 
                !methodInfo.IsExternal && !isReplaced;

            bool isExternal = methodInfo.IsExternal || (stubbed && !methodIsProxied);
            if (isExternal) {
                if (isReplaced)
                    return;

                var isProperty = methodInfo.DeclaringProperty != null;

                if (isProperty && methodInfo.DeclaringProperty.IsExternal)
                    return;

                if (!isProperty || !methodInfo.Member.IsCompilerGenerated()) {

                } else {
                    isExternal = false;
                }
            }

            var makeSkeleton = stubbed && isExternal && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false);

            if (methodInfo.IsIgnored)
                return;
            if (!method.HasBody && !isExternal)
                return;

            astEmitter.ReferenceContext.EnclosingType = method.DeclaringType;
            astEmitter.ReferenceContext.EnclosingMethod = null;

            output.NewLine();

            if (methodIsProxied) {
                output.Comment("Implementation from {0}", methodInfo.Member.DeclaringType.FullName);
                output.NewLine();
            }

            MethodReference oldDefining = astEmitter.ReferenceContext.DefiningMethod;
            astEmitter.ReferenceContext.DefiningMethod = methodRef;

            try {
                dollar(output);
                output.Dot();
                if (isExternal && !Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false))
                    output.Identifier("ExternalMethod", null);
                else
                    output.Identifier("Method", null);
                output.LPar();

                output.MemberDescriptor(method.IsPublic, method.IsStatic);

                output.Comma();
                output.Value(Util.EscapeIdentifier(methodInfo.GetName(true), EscapingMode.String));

                output.Comma();
                output.NewLine();

                output.MethodSignature(methodRef, methodInfo.Signature, astEmitter.ReferenceContext);

                if (!isExternal) {
                    output.Comma();
                    output.NewLine();

                    JSFunctionExpression function;
                    function = FunctionCache.GetExpression(new QualifiedMemberIdentifier(
                        methodInfo.DeclaringType.Identifier,
                        methodInfo.Identifier
                    ));

                    if (function != null) {
                        if (bodyTransformer != null)
                            bodyTransformer(function);

                        function.DisplayName = methodInfo.GetName(false);

                        astEmitter.ReferenceContext.EnclosingMethod = method;

                        astEmitter.Visit(function);

                        astEmitter.ReferenceContext.EnclosingMethod = null;
                    } else {
                        output.Identifier("JSIL.UntranslatableFunction", null);
                        output.LPar();
                        output.Value(method.FullName);
                        output.RPar();
                    }
                } else if (makeSkeleton) {
                    output.Comma();
                    output.NewLine();

                    output.OpenFunction(
                        methodInfo.Name,
                        (o) => output.WriteParameterList(
                            (from gpn in methodInfo.GenericParameterNames select 
                             new JSParameter(gpn, methodRef.Module.TypeSystem.Object, methodRef))
                            .Concat(from p in methodInfo.Parameters select 
                             new JSParameter(p.Name, p.ParameterType, methodRef))
                        )
                    );

                    output.WriteRaw("throw new Error('Not implemented');");
                    output.NewLine();

                    output.CloseBrace(false);
                }

                output.NewLine();
                output.RPar();

                output.Semicolon();
            } finally {
                astEmitter.ReferenceContext.DefiningMethod = oldDefining;
            }
        }

        protected void TranslateProperty (
            DecompilerContext context, JavascriptFormatter output,
            PropertyDefinition property, Action<JavascriptFormatter> dollar
        ) {
            var propertyInfo = _TypeInfoProvider.GetMemberInformation<Internal.PropertyInfo>(property);
            if ((propertyInfo == null) || propertyInfo.IsIgnored || propertyInfo.IsExternal)
                return;

            var isStatic = (property.SetMethod ?? property.GetMethod).IsStatic;

            output.NewLine();

            dollar(output);
            output.Dot();

            if (property.DeclaringType.HasGenericParameters && isStatic)
                output.Identifier("GenericProperty", null);
            else
                output.Identifier("Property", null);

            output.LPar();

            output.MemberDescriptor(propertyInfo.IsPublic, propertyInfo.IsStatic);

            output.Comma();

            output.Value(Util.EscapeIdentifier(propertyInfo.Name, EscapingMode.String));

            output.RPar();
            output.Semicolon();
        }

        public void Dispose () {
            if (OwnsTypeInfoProvider)
                _TypeInfoProvider.Dispose();

            FunctionCache.Dispose();

            if (OwnsAssemblyCache)
                AssemblyCache.Dispose();
        }

        public TypeInfoProvider GetTypeInfoProvider () {
            OwnsTypeInfoProvider = false;
            return _TypeInfoProvider;
        }
    }
}
