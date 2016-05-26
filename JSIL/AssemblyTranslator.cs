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
using Mono.Cecil.Cil;
using ICSharpCode.Decompiler;
using JSIL.Compiler.Extensibility;

using GenericParameterAttributes = Mono.Cecil.GenericParameterAttributes;
using MethodInfo = JSIL.Internal.MethodInfo;
using TypeInfo = JSIL.Internal.TypeInfo;

namespace JSIL {
    public delegate void AssemblyLoadedHandler (string assemblyName, string classification);
    public delegate void ProgressHandler (ProgressReporter pr);
    public delegate void DecompilingMethodHandler (string methodName, ProgressReporter pr);
    public delegate void LoadErrorHandler (string name, Exception error);

    public class AssemblyTranslator : IDisposable {
        public struct Cachers {
            public readonly TypeExpressionCacher Type;
            public readonly SignatureCacher Signature;
            public readonly BaseMethodCacher BaseMethod;

            public Cachers (TypeExpressionCacher type, SignatureCacher signature, BaseMethodCacher baseMethod) {
                if (type == null)
                    throw new ArgumentNullException("type");
                else if (signature == null)
                    throw new ArgumentNullException("signature");
                else if (baseMethod == null)
                    throw new ArgumentNullException("baseMethod");

                Type = type;
                Signature = signature;
                BaseMethod = baseMethod;
            }
        }

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

        public const int DefaultStreamCapacity = 4 * (1024 * 1024);

        public readonly Configuration Configuration;

        public readonly SymbolProvider SymbolProvider = new SymbolProvider();
        public readonly AssemblyDataResolver AssemblyDataResolver;
        public readonly FunctionCache FunctionCache;
        public readonly AssemblyManifest Manifest;

        public readonly System.Collections.ObjectModel.ReadOnlyCollection<IAnalyzer> Analyzers;
        internal readonly IFunctionTransformer[] FunctionTransformers;

        public readonly List<Exception> Failures = new List<Exception>();

        public event AssemblyLoadedHandler AssemblyLoaded;
        public event AssemblyLoadedHandler AssemblyNotLoaded;
        public event AssemblyLoadedHandler ProxyAssemblyLoaded;

        public event ProgressHandler RunningAnalyzers;
        public event ProgressHandler Decompiling;
        public event ProgressHandler RunningTransforms;
        public event ProgressHandler Writing;
        public event DecompilingMethodHandler DecompilingMethod;

        public event LoadErrorHandler CouldNotLoadSymbols;
        public event LoadErrorHandler CouldNotResolveAssembly;
        public event LoadErrorHandler CouldNotDecompileMethod;
        
        public event Action<string> Warning;
        public event Action<string, string[]> IgnoredMethod;

        public event Action<TypeIdentifier> ProxyNotMatched;
        public event Action<QualifiedMemberIdentifier> ProxyMemberNotMatched;

        public readonly TypeInfoProvider TypeInfoProvider;
        public readonly IEmitterGroupFactory[] EmitterGroupFactories;

        protected bool OwnsAssemblyDataResolver;
        protected bool OwnsTypeInfoProvider;

        public AssemblyTranslator (
            Configuration configuration,
            TypeInfoProvider typeInfoProvider = null,
            AssemblyManifest manifest = null,
            AssemblyDataResolver assemblyDataResolver = null,
            AssemblyLoadedHandler onProxyAssemblyLoaded = null,
            IEnumerable<IAnalyzer> analyzers = null,
            IEmitterGroupFactory[] emitterGroupFactories = null
        )
        {
            ProxyAssemblyLoaded = onProxyAssemblyLoaded;
            Warning = (s) =>
                Console.Error.WriteLine("// {0}", s);

            Manifest = manifest ?? new AssemblyManifest();
            EmitterGroupFactories = emitterGroupFactories ?? new IEmitterGroupFactory[] { new JavascriptEmitterGroupFactory() };

            foreach (var emitterFactory in EmitterGroupFactories) {
                //TODO: Split FilterConfiguration from IEmitterGroupFactory
                Configuration = emitterFactory.FilterConfiguration(configuration);
            }
            bool useDefaultProxies = configuration.UseDefaultProxies.GetValueOrDefault(true);

            OwnsAssemblyDataResolver = (assemblyDataResolver == null);
            AssemblyDataResolver = assemblyDataResolver ?? new AssemblyDataResolver(configuration);
            AssemblyDataResolver.AssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(Util.GetPathOfAssembly(Assembly.GetExecutingAssembly())));

            var analyzerList = new List<IAnalyzer>();
            if (analyzers != null)
                analyzerList.AddRange(analyzers);

            foreach (var emitterFactory in EmitterGroupFactories) {
                //TODO: Split GetAnalyzers from IEmitterGroupFactory
                analyzerList.AddRange(emitterFactory.GetAnalyzers());
            }

            if (typeInfoProvider != null) {
                TypeInfoProvider = typeInfoProvider;
                OwnsTypeInfoProvider = false;

                if (configuration.Assemblies.Proxies.Count > 0)
                    throw new InvalidOperationException("Cannot reuse an existing type provider if explicitly loading proxies");
            } else {
                TypeInfoProvider = new JSIL.TypeInfoProvider();
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

            FunctionCache = new FunctionCache(TypeInfoProvider);
            Analyzers = analyzerList.AsReadOnly();

            FunctionTransformers = analyzerList.SelectMany(a => a.FunctionTransformers).ToArray();
        }

        public static Assembly GetDefaultProxyAssembly (double frameworkVersion) {
            var myAssemblyPath = Util.GetPathOfAssembly(Assembly.GetExecutingAssembly());
            var proxyFolder = Path.GetDirectoryName(myAssemblyPath);
            string proxyPath = null;

            try {
                if (frameworkVersion == 4.0) {
                    proxyPath = Path.Combine(proxyFolder, "JSIL.Proxies.4.0.dll");
                } else {
                    throw new ArgumentOutOfRangeException(
                        "frameworkVersion",
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

        internal void WarningFormatFunction (string functionName, string format, params object[] args) {
            Warning(String.Format("{0}: {1}", functionName, String.Format(format, args)));
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
                AssemblyDataResolver.AssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(mainAssemblyPath));
                readerParameters.AssemblyResolver = AssemblyDataResolver.AssemblyResolver;
                readerParameters.MetadataResolver = AssemblyDataResolver.CachingMetadataResolver;
            }

            if (useSymbols)
                readerParameters.SymbolReaderProvider = SymbolProvider;

            return readerParameters;
        }

        private void OnProxiesFoundHandler (AssemblyDefinition asm) {
            if (ProxyAssemblyLoaded != null)
                ProxyAssemblyLoaded(asm.Name.Name, "proxy");
        }

        public void AddProxyAssembly (string path) {
            var assemblies = LoadAssembly(path, Configuration.UseSymbols.GetValueOrDefault(true), false);
            TypeInfoProvider.AddProxyAssemblies(OnProxiesFoundHandler, assemblies);
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
            T assemblyName, ReaderParameters readerParameters, 
            bool useSymbols, string mainAssemblyPath
        ) {
            AssemblyDefinition result = null;            

            try {
                result = loader(assemblyName, readerParameters);
            } catch (Exception ex) {
                if (useSymbols) {
                    try {
                        result = loader(assemblyName, GetReaderParameters(false, mainAssemblyPath));
                        if (CouldNotLoadSymbols != null)
                            CouldNotLoadSymbols(assemblyName.ToString(), ex);
                    } catch (Exception ex2) {
                        if (CouldNotResolveAssembly != null)
                            CouldNotResolveAssembly(assemblyName.ToString(), ex2);
                    }
                } else {
                    if (CouldNotResolveAssembly != null)
                        CouldNotResolveAssembly(assemblyName.ToString(), ex);
                }
            }

            return result;
        }

        protected ParallelOptions GetParallelOptions () {
            return new ParallelOptions {
                MaxDegreeOfParallelism = Configuration.UseThreads.GetValueOrDefault(false) 
                    ? (Environment.ProcessorCount + 2) 
                    : 1
            };
        }

        protected bool IsIgnored (string assemblyName) {
            foreach (var ia in Configuration.Assemblies.Ignored) {
                if (Regex.IsMatch(assemblyName, ia, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        protected string ResolveRedirectedName (string assemblyName) {
            foreach (var ra in Configuration.Assemblies.Redirects) {
                if (Regex.IsMatch(assemblyName, ra.Key, RegexOptions.IgnoreCase))
                    return ra.Value;
            }

            return assemblyName;
        }

        public string ClassifyAssembly (AssemblyDefinition asm) {
            if (IsIgnored(asm.FullName))
                return "ignored";
            else if (IsStubbed(asm))
                return "stubbed";
            else
                return "translate";
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

            var result = new List<AssemblyDefinition> {
                assembly
            };

            if (AssemblyLoaded != null)
                AssemblyLoaded(path, ClassifyAssembly(assembly));

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
                            bool ignored = IsIgnored(reference.FullName);

                            if (ignored) {
                                if (AssemblyNotLoaded != null)
                                    AssemblyNotLoaded(reference.FullName, "ignored");

                                continue;
                            }

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

                            var refAssembly = AssemblyLoadErrorWrapper(
                                readerParameters.AssemblyResolver.Resolve,
                                anr, readerParameters,
                                useSymbols, path
                            );

                            if (refAssembly != null) {
                                if (AssemblyLoaded != null)
                                    AssemblyLoaded(refAssembly.MainModule.FullyQualifiedName, ClassifyAssembly(refAssembly));

                                lock (result)
                                    result.Add(refAssembly);

                                lock (modulesToVisit)
                                    modulesToVisit.AddRange(refAssembly.Modules);
                            } else {
                                Warning(String.Format(
                                    "Failed to load assembly '{0}'", anr.FullName
                                ));
                            }
                        }
                    );
                    assembliesToLoad.Clear();
                }
            }

            // HACK: If an assembly we loaded has indirect references to multiple versions of BCL assemblies,
            //  Cecil will resolve them all to the same version. As a result, we'll end up with multiple copies
            //  of the same assembly in result. We need to filter those out so we only return each assembly once.
            return result.Distinct(new FullNameAssemblyComparer()).ToArray();
        }

        protected DecompilerContext MakeDecompilerContext (ModuleDefinition module) {
            return new DecompilerContext(module) {
                Settings = {
                    AsyncAwait = false,
                    YieldReturn = false,
                    AnonymousMethods = true,
                    QueryExpressions = false,
                    LockStatement = false,
                    FullyQualifyAmbiguousTypeNames = true,
                    ForEachStatement = false,
                    ExpressionTrees = false,
                    ObjectOrCollectionInitializers = false,
                    // FIXME
                    MakeCompoundAssignmentExpressions = true
                }
            };
        }

        protected virtual string FormatOutputFilename (string fileName) {
            var name = Path.GetFileNameWithoutExtension(fileName);
            foreach (var filenameReplaceRegex in Configuration.FilenameReplaceRegexes) {
                name = Regex.Replace(name, filenameReplaceRegex.Key, filenameReplaceRegex.Value);
            }

            if (Configuration.FilenameEscapeRegex != null)
                name = Regex.Replace(name, Configuration.FilenameEscapeRegex, "_");

            return Path.Combine(Path.GetDirectoryName(fileName), name + Path.GetExtension(fileName));
        }

        public TranslationResultCollection Translate (
            string assemblyPath, bool scanForProxies = true
        ) {
            var originalLatencyMode = System.Runtime.GCSettings.LatencyMode;

            try {
#if TARGETTING_FX_4_5
                if (Configuration.TuneGarbageCollection.GetValueOrDefault(true))
                    System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
#endif

                var sw = Stopwatch.StartNew();

                if (Configuration.RunBugChecks.GetValueOrDefault(true))
                    BugChecks.RunBugChecks();
                else
                    Console.Error.WriteLine("// WARNING: Bug checks have been suppressed. You may be running JSIL on a broken/unsupported .NET runtime.");

                var result = TranslateInternal(assemblyPath, scanForProxies);

                sw.Stop();
                result.Elapsed = sw.Elapsed;
                return result;
            } finally {
                System.Runtime.GCSettings.LatencyMode = originalLatencyMode;
            }
        }

        private TranslationResultCollection TranslateInternal (
            string assemblyPath, bool scanForProxies = true
        ) {
            var result = new TranslationResultCollection();
            var results = EmitterGroupFactories.Select(item => new {EmitterGroup = item, TranslationResult = new TranslationResult(this.Configuration, assemblyPath, Manifest)}).ToList();
            foreach (var pairs in results) {
                result.TranslationResults.Add(pairs.TranslationResult);
            }

            var assemblies = new [] {assemblyPath}.Union(this.Configuration.Assemblies.TranslateAdditional).Distinct()
                .SelectMany(LoadAssembly).Distinct(new FullNameAssemblyComparer()).ToArray();
            var parallelOptions = GetParallelOptions();

            if (scanForProxies)
                TypeInfoProvider.AddProxyAssemblies(OnProxiesFoundHandler, assemblies);
            
            var pr = new ProgressReporter();
            if (RunningAnalyzers != null)
                RunningAnalyzers(pr);

            foreach (var analyzer in Analyzers)
                analyzer.Analyze(this, assemblies, TypeInfoProvider);

            TriggerAutomaticGC();
            pr.OnFinished();

            pr = new ProgressReporter();
            if (Decompiling != null)
                Decompiling(pr);

            var methodsToAnalyze = new ConcurrentBag<MethodToAnalyze>();
            for (int i = 0; i < assemblies.Length; i++) {
                pr.OnProgressChanged(i, assemblies.Length * 2);
                GetMethodsToAnalyze(assemblies[i], methodsToAnalyze);
            }

            AnalyzeFunctions(
                parallelOptions, assemblies,
                methodsToAnalyze, pr
            );

            TriggerAutomaticGC();
            pr.OnFinished();

            pr = new ProgressReporter();
            if (RunningTransforms != null)
                RunningTransforms(pr);

            RunTransformsOnAllFunctions(parallelOptions, pr, result.Log);
            pr.OnFinished();

            TriggerAutomaticGC();

            pr = new ProgressReporter();
            if (Writing != null)
                Writing(pr);

            // Assign a unique identifier for all participating assemblies up front
            foreach (var assembly in assemblies) {
                if (ResolveRedirectedName(assembly.FullName) != assembly.FullName)
                    continue;

                Manifest.GetPrivateToken(assembly);
            }

            Manifest.AssignIdentifiers();

            Action<int> writeAssembly = (i) => {
                var assembly = assemblies[i];
                bool stubbed = IsStubbed(assembly);

                foreach (var pair in results) {
                    long existingSize;
                    foreach (var emmitterFactory in pair.EmitterGroup.MakeAssemblyEmitterFactory(this, assembly)) {
                        var outputPath = FormatOutputFilename(emmitterFactory.AssemblyPathAndFilename);
                        if (!Manifest.GetExistingSize(assembly, emmitterFactory.Id, out existingSize)) {
                            using (var outputStream = new MemoryStream(DefaultStreamCapacity)) {
                                var sourceMapBuilder = Configuration.BuildSourceMap.GetValueOrDefault() ? new SourceMapBuilder() : null;
                                var context = MakeDecompilerContext(assembly.MainModule);

                                try {
                                    var tw = new StreamWriter(outputStream, Encoding.ASCII);
                                    var formatter = new JavascriptFormatter(tw, sourceMapBuilder, this.TypeInfoProvider, Manifest, assembly, Configuration, stubbed);
                                    var emitter = emmitterFactory.MakeAssemblyEmitter(formatter);
                                    TranslateSingleAssemblyInternal(emitter, context, assembly, outputStream, sourceMapBuilder);
                                    tw.Flush();
                                }
                                catch (Exception exc) {
                                    throw new Exception("Error occurred while generating javascript for assembly '" + assembly.FullName + "'.", exc);
                                }
                                var segment = new ArraySegment<byte>(
                                    outputStream.GetBuffer(), 0, (int) outputStream.Length
                                    );

                                pair.TranslationResult.AddFile(emmitterFactory.ArtifactType, outputPath, segment, sourceMapBuilder: sourceMapBuilder);

                                Manifest.SetAlreadyTranslated(assembly, emmitterFactory.Id, outputStream.Length);
                            }

                            lock (pair.TranslationResult.Assemblies)
                                pair.TranslationResult.Assemblies.Add(assembly);
                        } else {
                            Console.WriteLine("Skipping '{0}' because it is already translated...", assembly.Name);

                            pair.TranslationResult.AddExistingFile(emmitterFactory.ArtifactType, outputPath, existingSize);
                        }
                    //TODO!
                    //pr.OnProgressChanged(translationResultFactory.Item2.Assemblies.Count, assemblies.Length);
                    }
                }
            };

            if (Configuration.UseThreads.GetValueOrDefault(false)) {
                Parallel.For(
                    0, assemblies.Length, parallelOptions, writeAssembly
                );
            } else {
                for (var i = 0; i < assemblies.Length; i++)
                    writeAssembly(i);
            }

            TriggerAutomaticGC();

            pr.OnFinished();

            DoProxyDiagnostics();

            foreach (var pair in results) {
                pair.EmitterGroup.RunPostprocessors(Manifest, assemblyPath, pair.TranslationResult);
            }

            return result;
        }

        private void DoProxyDiagnostics () {
            if ((ProxyNotMatched == null) && (ProxyMemberNotMatched == null))
                return;

            var methodsToSkip = new HashSet<MemberIdentifier>(new MemberIdentifier.Comparer(TypeInfoProvider));

            foreach (var p in TypeInfoProvider.Proxies) {
                var ti = new TypeIdentifier(p.Definition);

                if ((p.UsageCount == 0) && (ProxyNotMatched != null)) {
                    ProxyNotMatched(ti);
                    continue;
                }

                // If they explicitly disabled replacement for the type, none of the members got replaced
                if (p.MemberPolicy == Proxy.JSProxyMemberPolicy.ReplaceNone)
                    continue;

                if (ProxyMemberNotMatched != null) {
                    methodsToSkip.Clear();

                    foreach (var kvp in p.Properties) {
                        if (kvp.Value.CustomAttributes.Any(ca => ca.AttributeType.FullName == "JSIL.Proxy.JSNeverReplace")) {
                            var mi = kvp.Key;

                            if (kvp.Value.GetMethod != null)
                                methodsToSkip.Add(mi.Getter);

                            if (kvp.Value.SetMethod != null)
                                methodsToSkip.Add(mi.Setter);

                            continue;
                        }
                    }

                    foreach (var kvp in p.Methods) {
                        if (methodsToSkip.Contains(kvp.Key))
                            continue;

                        var identifier = kvp.Key;

                        // Don't log warnings on failed 0-arg default ctor replacement.
                        // Very often this just means the 0-arg ctor the compiler synthesized for the proxy didn't replace anything.
                        if (
                            (identifier.Name == ".ctor") && 
                            (
                                (identifier.ParameterTypes == null) || (identifier.ParameterTypes.Length == 0)
                            )
                        )
                            continue;

                        bool used;
                        p.MemberReplacedTable.TryGetValue(identifier, out used);

                        if (!used) {
                            // Member was explicitly marked as neverreplace, so of course it didn't replace anything
                            if (kvp.Value.CustomAttributes.Any(ca => ca.AttributeType.FullName == "JSIL.Proxy.JSNeverReplace"))
                                continue;

                            ProxyMemberNotMatched(new QualifiedMemberIdentifier(ti, identifier));
                        }
                    }
                }
            }
        }

        private void TriggerAutomaticGC () {
            if (Configuration.TuneGarbageCollection.GetValueOrDefault(true))
#if TARGETTING_FX_4_5
                GC.Collect(2, GCCollectionMode.Optimized, false);
#else
                GC.Collect(2, GCCollectionMode.Optimized);
#endif
        }




        private void AnalyzeFunctions (
            ParallelOptions parallelOptions, AssemblyDefinition[] assemblies,
            ConcurrentBag<MethodToAnalyze> methodsToAnalyze, ProgressReporter pr
        ) {
            int i = 0, mc = methodsToAnalyze.Count;
            Func<int, ParallelLoopState, DecompilerContext, DecompilerContext> analyzeAMethod = (_, loopState, ctx) => {
                MethodToAnalyze m;
                if (!methodsToAnalyze.TryTake(out m))
                    throw new InvalidDataException("Method collection mutated during analysis. Try setting UseThreads=false (and report an issue!)");

                ctx.CurrentModule = m.MD.Module;
                ctx.CurrentType = m.MD.DeclaringType;
                ctx.CurrentMethod = m.MD;

                try {
                    TranslateMethodExpression(ctx, m.MD, m.MD, m.MI);
                } catch (Exception exc) {
                    throw new Exception("Error occurred while translating method '" + m.MD.FullName + "'.", exc);
                }

                var j = Interlocked.Increment(ref i);
                pr.OnProgressChanged(mc + j, mc * 2);

                return ctx;
            };

            if (Configuration.UseThreads.GetValueOrDefault(false)) {
                Parallel.For(
                    0, methodsToAnalyze.Count, parallelOptions,
                    () => MakeDecompilerContext(assemblies[0].MainModule),
                    analyzeAMethod,
                    (ctx) => { }
                );
            } else {
                var ctx = MakeDecompilerContext(assemblies[0].MainModule);

                while (methodsToAnalyze.Count > 0)
                    analyzeAMethod(0, default(ParallelLoopState), ctx);
            }
        }

        protected void RunTransformsOnAllFunctions (ParallelOptions parallelOptions, ProgressReporter pr, StringBuilder log) {
            int i = 0;

            const int autoGcInterval = 256;

            Action<QualifiedMemberIdentifier> itemHandler = (id) => {
                var e = FunctionCache.GetCacheEntry(id);

                // We can end up with multiple copies of a function in the pipeline, so we should just early out if we hit a duplicate
                if (e.TransformPipelineHasCompleted)
                    return;

                var _i = Interlocked.Increment(ref i);

                if ((_i % autoGcInterval) == 0)
                    TriggerAutomaticGC();

                if (e.Expression == null)
                    return;

                pr.OnProgressChanged(_i, _i + FunctionCache.PendingTransformsQueue.Count);

                if (RunTransformsOnFunction(id, e.Expression, e.SpecialIdentifiers, log)) {
                    // Release our SpecialIdentifiers instance so it doesn't leak indefinitely.
                    // e.SpecialIdentifiers = null;
                }
            };

            while (FunctionCache.PendingTransformsQueue.Count > 0) {
                // FIXME: Disabled right now because there is a race condition where the optimizer can be
                //  altering the static analysis information for a function while another function
                //  that depends on it is being optimized.
                if (Configuration.CodeGenerator.EnableThreadedTransforms.GetValueOrDefault(true)) {
                    Parallel.ForEach(
                        FunctionCache.PendingTransformsQueue.TryDequeueAll,
                        parallelOptions, itemHandler
                    );
                } else {
                    QualifiedMemberIdentifier _id;

                    while (FunctionCache.PendingTransformsQueue.TryDequeue(out _id))
                        itemHandler(_id);
                }
            }
        }

        // Invoking this function populates the type information graph, and builds a list
        //  of functions to analyze/optimize/translate (omitting ignored functions, etc).
        private void GetMethodsToAnalyze (AssemblyDefinition assembly, ConcurrentBag<MethodToAnalyze> allMethods) {
            bool isStubbed = IsStubbed(assembly);

            var parallelOptions = GetParallelOptions();
            var allTypes = new List<TypeDefinition>();

            foreach (var module in assembly.Modules) {
                var moduleInfo = TypeInfoProvider.GetModuleInformation(module);
                if (moduleInfo.IsIgnored)
                    continue;

                allTypes.AddRange(module.Types);
            }

            while (allTypes.Count > 0) {
                var types = new HashSet<TypeDefinition>(allTypes).ToList();
                allTypes.Clear();

                Parallel.For(
                    0, types.Count, parallelOptions,
                    () => new List<TypeDefinition>(),
                    (i, loopState, typeList) => {
                        var type = types[i];

                        typeList.AddRange(type.NestedTypes);

                        if (!ShouldTranslateMethods(type)) {
                            var info = TypeInfoProvider.GetTypeInformation(type);
                            if (info != null)
                            {
                                if (info.IsProxy && info.Metadata.HasAttribute("JSIL.Meta.JSImportType"))
                                {
                                    typeList.AddRange(TypeInfoProvider.FindTypeProxy(new TypeIdentifier(info.Definition)).ProxiedTypes.Select(item => item.Resolve()));
                                }
                            }
                            return typeList;
                        }

                        IEnumerable<MethodDefinition> methods = type.Methods;
                        var typeInfo = TypeInfoProvider.GetExisting(type);
                        if (typeInfo != null) {
                            if (typeInfo.StaticConstructor != null) {
                                methods = methods.Concat(new[] { typeInfo.StaticConstructor });
                            }

                            foreach (var esc in typeInfo.ExtraStaticConstructors) {
                                allMethods.Add(new MethodToAnalyze(esc));
                            }
                        }

                        foreach (var m in methods) {
                            var mi = TypeInfoProvider.GetMethod(m);

                            if ((mi == null) || (mi.IsIgnored))
                                continue;

                            // A pinvoke method with no body can be replaced by a proxy method body
                            if (!m.HasBody && !mi.IsFromProxy)
                                continue;

                            if (isStubbed && !mi.IsUnstubbable) {
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

        public bool IsIgnored (AssemblyDefinition assembly) {
            return IsIgnoredAssembly(assembly.FullName);
        }

        public bool IsIgnoredAssembly(string assemblyName)
        {
            foreach (var sa in Configuration.Assemblies.Ignored)
            {
                if (Regex.IsMatch(assemblyName, sa, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsStubbed (AssemblyDefinition assembly) {
            foreach (var sa in Configuration.Assemblies.Stubbed) {
                if (Regex.IsMatch(assembly.FullName, sa, RegexOptions.IgnoreCase)) {
                    return true;
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

        protected void TranslateSingleAssemblyInternal (IAssemblyEmitter assemblyEmitter, DecompilerContext context, AssemblyDefinition assembly, Stream outputStream, SourceMapBuilder sourceMapBuilder) {
            bool stubbed = IsStubbed(assembly);

            string assemblyDeclarationReplacement = null;
            var metadata = new MetadataCollection(assembly);
            if (metadata.HasAttribute("JSIL.Meta.JSRepaceAssemblyDeclaration"))
            {
                assemblyDeclarationReplacement = (string) metadata.GetAttributeParameters("JSIL.Meta.JSRepaceAssemblyDeclaration")[0].Value;
            }

            Dictionary<AssemblyManifest.Token, string> assemblies = new Dictionary<AssemblyManifest.Token, string>();
            var wrapAssemblyInImmediatelyInvokedFunctionExpression = Configuration.InlineAssemblyReferences.GetValueOrDefault(false);
            if (wrapAssemblyInImmediatelyInvokedFunctionExpression)
            {
                assemblies = assembly.Modules
                    .SelectMany(item => item.AssemblyReferences)
                    .Select(item => AssemblyDataResolver.AssemblyResolver.Resolve(item))
                    .Where(item => item != null)
                    .Distinct()
                    .ToDictionary(
                        item => Manifest.GetPrivateToken(item),
                        item => item.FullName);

                assemblies.Remove(Manifest.GetPrivateToken(assembly.FullName));
            }

            Dictionary<AssemblyManifest.Token, string> overrides =
                assembly.CustomAttributes.Where(
                    item => item.AttributeType.FullName == "JSIL.Meta.JSOverrideAssemblyReference")
                    .ToDictionary(
                        item =>
                            Manifest.GetPrivateToken(
                                ((TypeReference) (item.ConstructorArguments[0].Value)).Resolve().Module.Assembly),
                        item => (string) (item.ConstructorArguments[1].Value));

            foreach (var pair in overrides)
            {
                assemblies[pair.Key] = pair.Value;
            }

            Manifest.AssignIdentifiers();

            wrapAssemblyInImmediatelyInvokedFunctionExpression |= assemblies.Count > 0;
            assemblies = wrapAssemblyInImmediatelyInvokedFunctionExpression ? assemblies : null;

            assemblyEmitter.EmitHeader(stubbed, wrapAssemblyInImmediatelyInvokedFunctionExpression);
            assemblyEmitter.EmitAssemblyReferences(assemblyDeclarationReplacement, assemblies);

            if (assembly.EntryPoint != null)
                TranslateEntryPoint(assemblyEmitter, assembly);

            var sealedTypes = new HashSet<TypeDefinition>();
            var declaredTypes = new HashSet<TypeDefinition>();

            foreach (var module in assembly.Modules) {
                if (module.Assembly != assembly) {
                    WarningFormat("Warning: Mono.Cecil failed to correctly load the module '{0}'. Skipping it.", module);
                    continue;
                }

                TranslateModule(context, assemblyEmitter, module, sealedTypes, declaredTypes, stubbed);
            }

            TranslateImportedTypes(assembly, assemblyEmitter, declaredTypes, stubbed);

            assemblyEmitter.EmitFooter(wrapAssemblyInImmediatelyInvokedFunctionExpression);
        }

        protected void TranslateEntryPoint (
            IAssemblyEmitter emitter,
            AssemblyDefinition assembly
        ) {
            var entryMethod = assembly.EntryPoint;

            var signature = new MethodSignature(
                TypeInfoProvider,
                entryMethod.ReturnType,
                (from p in entryMethod.Parameters select p.ParameterType).ToArray(),
                null
            );

            emitter.EmitAssemblyEntryPoint(
                assembly, entryMethod, signature
            );
        }

        protected void TranslateImportedTypes(AssemblyDefinition assembly, IAssemblyEmitter assemblyEmitter,
            HashSet<TypeDefinition> declaredTypes, bool stubbed)
        {
            var typesToImport = assembly
                .Modules
                .SelectMany(item => item.Types)
                .Select(type => TypeInfoProvider.GetTypeInformation(type))
                .Where(item => item.IsProxy && item.Metadata.HasAttribute("JSIL.Meta.JSImportType"))
                .Select(item => TypeInfoProvider.FindTypeProxy(new TypeIdentifier(item.Definition)))
                .SelectMany(item => item.ProxiedTypes)
                .Select(item => item.Resolve())
                .GroupBy(item => item.Module)
                .GroupBy(item => item.Key.Assembly).ToList();

            if (typesToImport.Count == 0)
            {
                return;
            }

            foreach (var byAssembly in typesToImport)
            {
                var context = MakeDecompilerContext(byAssembly.Key.MainModule);
                foreach (var byModule in byAssembly)
                {
                    context.CurrentModule = byModule.Key;

                    var js = new JSSpecialIdentifiers(FunctionCache.MethodTypes, context.CurrentModule.TypeSystem);
                    var jsil = new JSILIdentifier(FunctionCache.MethodTypes, context.CurrentModule.TypeSystem,
                        this.TypeInfoProvider, js);

                    var astEmitter = assemblyEmitter.MakeAstEmitter(
                        jsil, context.CurrentModule.TypeSystem,
                        TypeInfoProvider, Configuration
                        );

                    foreach (var typeDefinition in byModule)
                    {
                        DeclareType(context, typeDefinition, astEmitter, assemblyEmitter, declaredTypes, stubbed, true);
                    }
                }
            }
        }

        protected void TranslateModule (
            DecompilerContext context, IAssemblyEmitter assemblyEmitter, ModuleDefinition module, 
            HashSet<TypeDefinition> sealedTypes, HashSet<TypeDefinition> declaredTypes, bool stubbed
        ) {
            var moduleInfo = TypeInfoProvider.GetModuleInformation(module);
            if (moduleInfo.IsIgnored)
                return;

            context.CurrentModule = module;

            var js = new JSSpecialIdentifiers(FunctionCache.MethodTypes, context.CurrentModule.TypeSystem);
            var jsil = new JSILIdentifier(FunctionCache.MethodTypes, context.CurrentModule.TypeSystem, this.TypeInfoProvider, js);

            var astEmitter = assemblyEmitter.MakeAstEmitter(
                jsil, context.CurrentModule.TypeSystem, 
                TypeInfoProvider, Configuration
            );

            foreach (var typedef in module.Types)
                DeclareType(context, typedef, astEmitter, assemblyEmitter, declaredTypes, stubbed);
        }

        public bool ShouldSkipMember (MemberReference member) {
            if (member is MethodReference && member.Name == ".cctor")
                return false;

            foreach (var analyzer in Analyzers)
                if (analyzer.ShouldSkipMember(this, member))
                    return true;

            return false;
        }

        protected void DeclareType (
            DecompilerContext context, TypeDefinition typedef, 
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter, 
            HashSet<TypeDefinition> declaredTypes, bool stubbed, bool isImported = false
        ) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return;

            if (declaredTypes.Contains(typedef))
                return;

            if (typeInfo.IsStubOnly)
            {
                stubbed = true;
            }

            declaredTypes.Add(typedef);
            bool declareOnlyInternalTypes = ShouldSkipMember(typedef);

            // This type is defined in JSIL.Core so we don't want to cause a name collision.
            if (!declareOnlyInternalTypes && typeInfo.IsSuppressDeclaration && !isImported) {
                assemblyEmitter.EmitTypeAlias(typedef);

                declareOnlyInternalTypes = true;
            }

            if (declareOnlyInternalTypes && !isImported) {
                DeclareNestedTypes(
                    context, typedef, 
                    astEmitter, assemblyEmitter, 
                    declaredTypes, stubbed, true
                );

                return;
            }

            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.DefiningType = typedef;
            context.CurrentType = typedef;

            try {
                // type has a JS replacement, we can't correctly emit a stub or definition for it. 
                // We do want to process nested types, though.
                if (typeInfo.Replacement != null && !isImported) {
                    DeclareNestedTypes(
                        context, typedef, 
                        astEmitter, assemblyEmitter, 
                        declaredTypes, stubbed, false
                    );

                    return;
                }

                if (!assemblyEmitter.EmitTypeDeclarationHeader(context, astEmitter, typedef, typeInfo))
                    return;

                var declaringType = typedef.DeclaringType;
                if (declaringType != null && !isImported)
                    DeclareType(context, declaringType, astEmitter, assemblyEmitter, declaredTypes, IsStubbed(declaringType.Module.Assembly));

                var baseClass = typedef.BaseType;
                if (baseClass != null) {
                    var resolved = baseClass.Resolve();
                    if (
                        (resolved != null) &&
                        (resolved.Module.Assembly == typedef.Module.Assembly)
                        && !isImported
                    ) {
                        DeclareType(context, resolved, astEmitter, assemblyEmitter, declaredTypes, IsStubbed(resolved.Module.Assembly));
                    }
                }

                assemblyEmitter.BeginEmitTypeDeclaration(typedef);

                JSRawOutputIdentifier dollar = new JSRawOutputIdentifier(astEmitter.TypeSystem.Object, "$");
                int nextDisambiguatedId = 0;
                var cachers = EmitTypeMethodExpressions(
                    context, typedef, astEmitter, assemblyEmitter, stubbed, dollar, ref nextDisambiguatedId
                );

                assemblyEmitter.BeginEmitTypeDefinition(
                    astEmitter, typedef, typeInfo, baseClass
                );

                try {
                    TranslateTypeDefinition(
                        context, typedef, 
                        astEmitter, assemblyEmitter, 
                        stubbed, dollar, 
                        cachers
                    );
                } finally {
                    assemblyEmitter.EndEmitTypeDefinition(astEmitter, context, typedef);
                }

                if (!isImported)
                {
                    foreach (var nestedTypeDef in typedef.NestedTypes)
                        DeclareType(context, nestedTypeDef, astEmitter, assemblyEmitter, declaredTypes, stubbed);
                }
            } catch (Exception exc) {
                throw new Exception(String.Format("An error occurred while declaring the type '{0}'", typedef.FullName), exc);
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }

        private void DeclareNestedTypes (
            DecompilerContext context, TypeDefinition typedef, 
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter, 
            HashSet<TypeDefinition> declaredTypes, bool stubbed, bool skipped
        ) {
            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.EnclosingType = typedef;
            astEmitter.ReferenceContext.EnclosingTypeSkipped = skipped;

            try {
                foreach (var nestedTypeDef in typedef.NestedTypes)
                    DeclareType(context, nestedTypeDef, astEmitter, assemblyEmitter, declaredTypes, stubbed);
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }

        protected bool ShouldTranslateMethods (TypeDefinition typedef) {
            if (ShouldSkipMember(typedef))
                return false;

            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if ((typeInfo == null) || typeInfo.IsIgnored || typeInfo.IsProxy)
                return false;

            if (typeInfo.IsExternal && typeInfo.UnstubbableMemberCount == 0)
                return false;

            if (typedef.IsInterface)
                return false;
            else if (typedef.IsEnum)
                return false;
            else if (typeInfo.IsDelegate)
                return false;

            return true;
        }

        protected Cachers EmitTypeMethodExpressions (
            DecompilerContext context, TypeDefinition typedef,
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter,
            bool stubbed, JSRawOutputIdentifier dollar,
            ref int nextDisambiguatedId
        ) {
            var typeCacher = new TypeExpressionCacher(typedef);
            var signatureCacher = new SignatureCacher(
                TypeInfoProvider,
                !Configuration.CodeGenerator.DisableGenericSignaturesLocalCache.GetValueOrDefault(false),
                Configuration.CodeGenerator.PreferLocalCacheForGenericMethodSignatures.GetValueOrDefault(true),
                Configuration.CodeGenerator.PreferLocalCacheForGenericInterfaceMethodSignatures.GetValueOrDefault(true),
                Configuration.CodeGenerator.CacheOneMethodSignaturePerMethod.GetValueOrDefault(true));
            var baseMethodCacher = new BaseMethodCacher(TypeInfoProvider, typedef);

            TypeInfoProvider.GetTypeInformation(typedef);
            if (!ShouldTranslateMethods(typedef))
                return new Cachers(typeCacher, signatureCacher, baseMethodCacher);

            var methodsToTranslate = typedef.Methods.OrderBy((md) => md.Name).ToList();

            var cacheTypes = Configuration.CodeGenerator.CacheTypeExpressions.GetValueOrDefault(true);
            var cacheSignatures = Configuration.CodeGenerator.CacheMethodSignatures.GetValueOrDefault(true);
            // FIXME: This is *incredibly* slow in both V8 and SpiderMonkey presently.
            var cacheBaseMethods = Configuration.CodeGenerator.CacheBaseMethodHandles.GetValueOrDefault(false);

            var caching = cacheTypes || cacheSignatures || cacheBaseMethods;

            if (caching) {
                foreach (var method in methodsToTranslate) {
                    var mi = TypeInfoProvider.GetMemberInformation<Internal.MethodInfo>(method);

                    bool isExternal, b, c;
                    if (!ShouldTranslateMethodBody(
                        method, mi, stubbed, out isExternal, out b, out c
                    ))
                        continue;

                    var functionBody = GetFunctionBodyForMethod(isExternal, mi);
                    if (functionBody == null)
                        continue;

                    if (cacheTypes)
                        typeCacher.CacheTypesForFunction(functionBody);
                    if (cacheSignatures)
                        signatureCacher.CacheSignaturesForFunction(functionBody);
                    if (cacheBaseMethods)
                        baseMethodCacher.CacheMethodsForFunction(functionBody);
                }

                assemblyEmitter.EmitCachedValues(astEmitter, typeCacher, signatureCacher, baseMethodCacher);
            }

            var cachers = new Cachers(typeCacher, signatureCacher, baseMethodCacher);

            foreach (var method in methodsToTranslate) {
                if (ShouldSkipMember(method))
                    continue;

                // We translate the static constructor explicitly later, and inject field initialization
                if (method.Name == ".cctor")
                    continue;

                EmitMethodBody(
                    context, method, method, astEmitter, assemblyEmitter,
                    stubbed, cachers, ref nextDisambiguatedId
                );
            }

            return cachers;
        }

        protected void TranslateTypeDefinition (
            DecompilerContext context, TypeDefinition typedef, 
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter, 
            bool stubbed, JSRawOutputIdentifier dollar,
            Cachers cachers
        ) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            if (!ShouldTranslateMethods(typedef))
                return;

            context.CurrentType = typedef;

            if (typedef.IsPrimitive)
                assemblyEmitter.EmitPrimitiveDefinition(context, typedef, stubbed, dollar);

            var methodsToTranslate = typedef.Methods.OrderBy((md) => md.Name).ToArray();

            foreach (var method in methodsToTranslate) {
                if (ShouldSkipMember(method))
                    continue;

                // We translate the static constructor explicitly later, and inject field initialization
                if (method.Name == ".cctor")
                    continue;

                assemblyEmitter.EmitMethodDefinition(
                    context, method, method, astEmitter, stubbed, dollar
                );
            }

            Action translateProperties = () => {
                foreach (var property in typedef.Properties)
                    assemblyEmitter.EmitProperty(context, astEmitter, property, dollar);
            };

            Action translateEvents = () => {
                foreach (var @event in typedef.Events)
                    assemblyEmitter.EmitEvent(context, astEmitter, @event, dollar);
            };

            TranslateTypeStaticConstructor(
                context, typedef, astEmitter, 
                assemblyEmitter, typeInfo.StaticConstructor, 
                stubbed, dollar,
                cachers
            );

            if ((typeInfo.MethodGroups.Count + typedef.Properties.Count) > 0) {
                translateProperties();
            }

            if ((typeInfo.MethodGroups.Count + typedef.Events.Count) > 0) {
                translateEvents();
            }

            assemblyEmitter.EmitInterfaceList(
                typeInfo, astEmitter, dollar
            );
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
                    methodInfo = TypeInfoProvider.GetMemberInformation<JSIL.Internal.MethodInfo>(methodDef);

                if (methodInfo == null)
                    throw new InvalidDataException(String.Format(
                        "Method '{0}' has no method information!",
                        method.FullName
                    ));

                var identifier = new QualifiedMemberIdentifier(
                    methodInfo.DeclaringType.Identifier, methodInfo.Identifier
                );
                JSFunctionExpression function;

                if (FunctionCache.TryGetExpression(identifier, out function)) {
                    return function;
                }

                bool skip = 
                    ShouldSkipMember(method) ||
                    methodInfo.IsExternal ||
                    methodInfo.IsAbstract;
                
                if (skip) {
                    FunctionCache.CreateNull(methodInfo, method, identifier);
                    return null;
                }

                var bodyDef = methodDef;
                Func<TypeReference, TypeReference> typeReplacer = (originalType) =>
                    originalType;

                if (methodInfo.IsFromProxy && methodInfo.Member.HasBody) {
                    bodyDef = methodInfo.Member;

                    var sourceProxy = methodInfo.SourceProxy;
                    typeReplacer = (originalType) => {
                        if (TypeUtil.TypesAreEqual(sourceProxy.Definition, originalType))
                            return method.DeclaringType;
                        else
                            return originalType;
                    };
                }

                var pr = new ProgressReporter();

                context.CurrentMethod = methodDef;
                if ((bodyDef.Body.CodeSize > LargeMethodThreshold) && (this.DecompilingMethod != null))
                    this.DecompilingMethod(method.FullName, pr);

                ILBlock ilb;
                var decompiler = new ILAstBuilder();
                var optimizer = new ILAstOptimizer();

                try {
                    lock (bodyDef) {
                        ilb = new ILBlock(decompiler.Build(bodyDef, true, context));
                        optimizer.Optimize(context, ilb);
                    }
                } catch (Exception exception) {
                    Failures.Add(exception);

                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(bodyDef.FullName, exception);

                    FunctionCache.CreateNull(methodInfo, method, identifier);
                    pr.OnFinished();
                    return null;
                }

                IEnumerable<ILVariable> allVariables;
                {
                    var ignoredVariables = new List<string>();
                    allVariables = GetAllVariablesForMethod(context, decompiler.Parameters, ilb, ignoredVariables, Configuration.CodeGenerator.EnableUnsafeCode.GetValueOrDefault(false));
                    if (allVariables == null) {
                        _IgnoredMethod(
                            method.FullName, ignoredVariables
                        );

                        FunctionCache.CreateNull(methodInfo, method, identifier);
                        pr.OnFinished();
                        return null;
                    }
                }

                var translator = new ILBlockTranslator(
                    this, context, method, methodDef,
                    ReadMethodSymbolsIfSourceMapEnabled(methodDef),
                    ilb, decompiler.Parameters, allVariables,
                    typeReplacer
                );

                JSBlockStatement body;
                try {
                    body = translator.Translate();
                } catch (Exception exc) {
                    Failures.Add(exc);

                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(bodyDef.FullName, exc);

                    body = null;
                }

                if (body == null) {
                    FunctionCache.CreateNull(methodInfo, method, identifier);
                    pr.OnFinished();
                    return null;
                }

                var parameters = (from v in translator.Variables.Values where v.IsParameter && !v.IsThis select v);

                if (method.HasGenericParameters) {
                    var type = context.CurrentModule.TypeSystem.SystemType();
                    parameters = (from gp in method.GenericParameters select new JSVariable(gp.Name, type, method)).Concat(parameters);
                }

                if (
                    Configuration.CodeGenerator.FreezeImmutableObjects.GetValueOrDefault(false) &&
                    (method.Name == ".ctor") &&
                    methodInfo.DeclaringType.IsImmutable &&
                    TypeUtil.IsStruct(method.DeclaringType)
                ) {
                    var freezeInvocation = translator.SpecialIdentifiers.JSIL.FreezeImmutableObject(new JSIndirectVariable(translator.Variables, "this", method));
                    body.Statements.Add(new JSExpressionStatement(freezeInvocation));
                }

                function = FunctionCache.Create(
                    methodInfo, methodDef, method, identifier,
                    translator, parameters.ToArray(), body
                );
                function.TemporaryVariableTypes.AddRange(translator.TemporaryVariableTypes);

                pr.OnFinished();
                return function;
            } finally {
                context.CurrentMethod = oldMethod;
            }
        }

        private void _IgnoredMethod (string methodName, IEnumerable<string> ignoredVariableNames) {
            var variableNames = ignoredVariableNames.ToArray();

            if (IgnoredMethod == null)
                WarningFormat(
                    "Ignoring method '{0}' because of {1} untranslatable variables:\r\n{2}",
                    methodName, variableNames.Length, String.Join(", ", variableNames)
                );
            else
                IgnoredMethod(methodName, variableNames);
        }

        private static IEnumerable<ILNode> ExpressionSelfAndChildrenRecursive (ILNode root) {
            yield return root;

            foreach (var child in root.GetChildren()) {
                foreach (var item in ExpressionSelfAndChildrenRecursive(child))
                    yield return item;
            }
        }

        private static ILVariable[] GatherLocalVariablesForMethod (ILBlock methodBody) {
            var result = new HashSet<ILVariable>();

            foreach (var node in ExpressionSelfAndChildrenRecursive(methodBody)) {
                var ile = node as ILExpression;
                if (ile == null)
                    continue;

                var operand = ile.Operand as ILVariable;
                if (operand == null)
                    continue;

                if (operand.IsParameter)
                    continue;

                result.Add(operand);
            }

            return result.ToArray();
        }

        internal static ILVariable[] GetAllVariablesForMethod(
            DecompilerContext context, IEnumerable<ILVariable> parameters, ILBlock methodBody,
            List<string> ignoredVariables, bool enableUnsafeCode
        ) {
            var allVariables = GatherLocalVariablesForMethod(methodBody);
            bool ignored = false;

            foreach (var v in allVariables) {
                if (TypeUtil.IsIgnoredType(v.Type, enableUnsafeCode)) {
                    ignoredVariables.Add(v.Name);
                    ignored = true;
                }
            }

            if (ignored)
                return null;

            NameVariables.AssignNamesToVariables(context, parameters, allVariables, methodBody);

            return allVariables;
        }

        private bool RunTransformsOnFunction (
            QualifiedMemberIdentifier memberIdentifier, JSFunctionExpression function,
            SpecialIdentifiers si, StringBuilder log
        ) {
            FunctionTransformPipeline pipeline;

            if (!FunctionCache.ActiveTransformPipelines.TryGetValue(memberIdentifier, out pipeline)) {
                pipeline = new FunctionTransformPipeline(
                    this, memberIdentifier, function, si
                );

                foreach (var functionTransformer in FunctionTransformers)
                    functionTransformer.InitializeTransformPipeline(this, pipeline);
            }

            bool completed = pipeline.RunUntilCompletion();

            if (completed) {
                if (pipeline.SuspendCount >= FunctionTransformPipeline.SuspendCountLogThreshold) {
                    lock (log)
                        log.AppendFormat(
                            "Transform pipeline for {0}::{1} was suspended {2} time(s) before completion{3}",
                            pipeline.Identifier.Type.Name,
                            pipeline.Identifier.Member.Name,
                            pipeline.SuspendCount,
                            Environment.NewLine
                        );
                }
            }

            return completed;
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
            bool cctorContext, JSRawOutputIdentifier dollar, JSStringIdentifier fieldSelfIdentifier
        ) {
            if (ShouldSkipMember(field))
                return null;

            var fieldInfo = TypeInfoProvider.GetMemberInformation<Internal.FieldInfo>(field);
            if ((fieldInfo == null) || fieldInfo.IsIgnored || fieldInfo.IsExternal)
                return null;

            var dollarIdentifier = new JSRawOutputIdentifier(field.DeclaringType, dollar.Format, dollar.Arguments);
            var descriptor = new JSMemberDescriptor(
                field.IsPublic, field.IsStatic, 
                isReadonly: field.IsInitOnly, 
                offset: field.DeclaringType.IsExplicitLayout
                    ? (int?)field.Offset
                    : null
            );

            var fieldName = Util.EscapeIdentifier(fieldInfo.Name, EscapingMode.MemberIdentifier);

            if (field.HasConstant) {
                JSLiteral constant;
                if (field.Constant == null) {
                    constant = JSLiteral.Null(fieldInfo.FieldType);
                } else {
                    constant = JSLiteral.New(field.Constant as dynamic);
                }

                JSExpression fieldTypeExpression = new JSTypeReference(fieldInfo.FieldType, field.DeclaringType);

                return new JSConstantDeclaration(
                    fieldInfo, descriptor, fieldName, fieldTypeExpression, constant 
                );
            } else {
                bool forCctor = false;
                if (field.IsStatic && NeedsStaticConstructor(fieldInfo.FieldType))
                    forCctor = true;
                else if (TypeUtil.IsStruct(fieldInfo.FieldType))
                    forCctor = true;

                JSExpression defaultValue;
                if (!defaultValues.TryGetValue(field, out defaultValue))
                    defaultValue = null;

                JSExpression fieldTypeExpression = new JSTypeReference(fieldInfo.FieldType, field.DeclaringType);

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
                        defaultValue is JSNewArrayExpression ||
                        defaultValue is JSEnumLiteral ||
                        defaultValue is JSCastExpression ||
                        defaultValue is JSTypeOfExpression
                    )
                ) {
                    // We have to represent the default value as a callable function, taking a single
                    //  argument that represents the public interface, so that recursive field initializations
                    //  will work correctly. InterfaceBuilder.Field will invoke this function for us.

                    defaultValue = new JSFunctionExpression(
                        // No method or variables. This could break things.
                        null, null, 
                        new JSVariable[] { 
                            new JSParameter(fieldSelfIdentifier.Identifier, fieldSelfIdentifier.IdentifierType, null) 
                        },
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
                        defaultValue = new JSDefaultValueLiteral(fieldInfo.FieldType);

                    return new JSBinaryOperatorExpression(
                        JSOperator.Assignment,
                        new JSFieldAccess(
                            thisParameter,
                            new JSField(field, fieldInfo),
                            true
                        ),
                        defaultValue,
                        fieldInfo.FieldType
                    );
                } else {
                    return new JSFieldDeclaration(
                        fieldInfo, descriptor, fieldName, fieldTypeExpression, defaultValue
                    );
                }
            }
        }

        protected void TranslateTypeStaticConstructor (
            DecompilerContext context, TypeDefinition typedef, 
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter, 
            MethodDefinition cctor, bool stubbed, JSRawOutputIdentifier dollar,
            Cachers cachers
        ) {
            var typeInfo = TypeInfoProvider.GetTypeInformation(typedef);
            var typeSystem = context.CurrentModule.TypeSystem;
            var staticFields = 
                (from f in typedef.Fields
                 where f.IsStatic
                 select f).ToArray();
            var fieldsToEmit =
                (from f in staticFields
                 where NeedsStaticConstructor(f.FieldType)
                 let fi = TypeInfoProvider.GetField(f)
                 where ((fi != null) && (!fi.IsExternal && !fi.IsIgnored)) || (fi == null)
                 select f).ToArray();
            var fieldsToStrip =
                new HashSet<FieldDefinition>(from f in staticFields
                 let fi = TypeInfoProvider.GetField(f)
                 where (fi != null) && (fi.IsExternal || fi.IsIgnored)
                 select f);

            // For fields with values assigned non-dynamically by the static constructor, we want to pull those values
            //  out of the static constructor and assign them ourselves. This ensures that these effective constants are
            //  carried over even if the static constructor (and other methods) are ignored/external.

            var fieldDefaults = new Dictionary<FieldDefinition, JSExpression>();
            JSStringIdentifier fieldSelfIdentifier = null;

            // It's possible for a proxy to replace the cctor, so we need to pull default values
            //  from the real cctor (if the type has one)
            var realCctor = typedef.Methods.FirstOrDefault((m) => m.Name == ".cctor");
            if ((realCctor != null) && (realCctor.HasBody)) {
                fieldSelfIdentifier = new JSStringIdentifier("$pi", realCctor.DeclaringType, true);

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
                var ignoreReasons = new List<string>();
                var variables = GetAllVariablesForMethod(
                    context, astBuilder.Parameters, block, ignoreReasons, Configuration.CodeGenerator.EnableUnsafeCode.GetValueOrDefault(false)
                );
                if (variables != null) {
                    // We need a translator to map the IL expressions for the default
                    //  values into JSAst expressions.
                    var translator = new ILBlockTranslator(
                        this, ctx, realCctor, realCctor, ReadMethodSymbolsIfSourceMapEnabled(realCctor), block, astBuilder.Parameters, variables
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

                            // Don't generate default value expressions for packed struct arrays.
                            var targetFieldInfo = TypeInfoProvider.GetField(targetField);
                            if ((targetFieldInfo != null) && PackedArrayUtil.IsPackedArrayType(targetFieldInfo.FieldType))
                                continue;

                            var expectedType = ile.Arguments[0].ExpectedType;

                            // If the field's value is of an ignored type then we ignore the initialization since it probably won't translate anyway.
                            if (TypeUtil.IsIgnoredType(expectedType))
                                continue;

                            JSExpression defaultValue;

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

#pragma warning disable 0642
                                if (
                                    (newArray != null) && (
                                        (newArray.SizeOrArrayInitializer == null) ||
                                        (newArray.SizeOrArrayInitializer.IsConstant)
                                    )
                                )
                                    ;
                                else if (!defaultValue.IsConstant)
                                    continue;
#pragma warning restore 0642

                            } catch (Exception ex) {
                                // This may fail because we didn't do a full translation.
                                WarningFormat("Warning: failed to translate default value for static field '{0}': {1}", targetField, ex);

                                continue;
                            }

                            var typeReferences = defaultValue.AllChildrenRecursive.OfType<JSType>();
                            foreach (var typeReference in typeReferences) {
                                if (TypeUtil.TypesAreEqual(typeReference.Type, realCctor.DeclaringType))
                                    defaultValue.ReplaceChildRecursive(typeReference, fieldSelfIdentifier);
                            }

                            var es = new JSExpressionStatement(defaultValue);
                            var ece = new ExpandCastExpressions(
                                translator.TypeSystem, translator.SpecialIdentifiers.JS, 
                                translator.SpecialIdentifiers.JSIL, translator.TypeInfo, 
                                FunctionCache.MethodTypes,
                                Configuration.CodeGenerator.EmulateInt64.GetValueOrDefault(true)
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
                var statements = f.Body.Children.OfType<JSExpressionStatement>().ToList();
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
                    var expr = TranslateField(field, fieldDefaults, true, dollar, fieldSelfIdentifier);

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

            Action<FieldDefinition> doTranslateField =
                (fd) => {
                    var expr = TranslateField(fd, fieldDefaults, false, dollar, fieldSelfIdentifier);
                    var fde = expr as JSFieldDeclaration;
                    var cde = expr as JSConstantDeclaration;

                    if (fde != null)
                        assemblyEmitter.EmitField(context, astEmitter, fd, dollar, fde.DefaultValue);
                    else if (cde != null)
                        assemblyEmitter.EmitConstant(context, astEmitter, fd, dollar, cde.Value);
                    else {
                        // FIXME: This probably isn't right
                        astEmitter.Emit(expr);
                        assemblyEmitter.EmitSemicolon();
                        assemblyEmitter.EmitSpacer();
                    }
                };

            foreach (var f in typedef.Fields) {
                var fi = TypeInfoProvider.GetField(f);
                if ((fi != null) && (fi.IsIgnored || fi.IsExternal || ShouldSkipMember(fi.Member)))
                    continue;

                doTranslateField(f);
            }
            
            // Added fields from proxies come after original fields, in their precise order.

            foreach (var af in typeInfo.AddedFieldsFromProxies) {
                if (af.Member.IsCompilerGeneratedOrIsInCompilerGeneratedClass() || ShouldSkipMember(af.Member))
                    continue;

                doTranslateField(af.Member);
            }

            int temp = 0;

            if ((cctor != null) && !stubbed) {
                assemblyEmitter.EmitSpacer();

                EmitAndDefineMethod(
                    context, cctor, cctor, 
                    astEmitter, assemblyEmitter, false, dollar, 
                    cachers, ref temp, null, fixupCctor
                );
            } else if (fieldsToEmit.Length > 0) {
                var fakeCctor = new MethodDefinition(".cctor", Mono.Cecil.MethodAttributes.Static, typeSystem.Void) {
                    DeclaringType = typedef
                };

                typeInfo.StaticConstructor = fakeCctor;
                var identifier = MemberIdentifier.New(this.TypeInfoProvider, fakeCctor);

                lock (typeInfo.Members)
                    typeInfo.Members[identifier] = new Internal.MethodInfo(
                        typeInfo, identifier, fakeCctor, new ArraySegment<ProxyInfo>(), null
                    );

                assemblyEmitter.EmitSpacer();

                // Generate the fake constructor, since it wasn't created during the analysis pass
                TranslateMethodExpression(context, fakeCctor, fakeCctor);

                EmitAndDefineMethod(
                    context, fakeCctor, fakeCctor, 
                    astEmitter, assemblyEmitter, false, dollar, 
                    cachers, ref temp, null, fixupCctor
                );
            }

            foreach (var extraCctor in typeInfo.ExtraStaticConstructors) {
                var declaringType = extraCctor.Member.DeclaringType;
                var newJSType = new JSType(typedef);

                EmitAndDefineMethod(
                    context, extraCctor.Member, extraCctor.Member, astEmitter,
                    assemblyEmitter, false, dollar, 
                    cachers, ref temp, extraCctor,
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

        protected void CreateMethodInformation (
            MethodInfo methodInfo, bool stubbed,
            out bool isExternal, out bool isJSReplaced, 
            out bool methodIsProxied
        ) {
            isJSReplaced = methodInfo.Metadata.HasAttribute("JSIL.Meta.JSReplacement");
            methodIsProxied = (methodInfo.IsFromProxy && methodInfo.Member.HasBody) &&
                !methodInfo.IsExternal && !isJSReplaced;

            isExternal = methodInfo.IsExternal || (stubbed && !methodInfo.IsUnstubbable);
        }

        internal bool ShouldTranslateMethodBody (
            MethodDefinition method, MethodInfo methodInfo, bool stubbed,
            out bool isExternal, out bool isJSReplaced,
            out bool methodIsProxied
        ) {
            if (methodInfo == null) {
                isExternal = isJSReplaced = methodIsProxied = false;
                return false;
            }

            CreateMethodInformation(
                methodInfo, stubbed,
                out isExternal, out isJSReplaced, out methodIsProxied
            );

            if (ShouldSkipMember(method))
                return false;

            if (isExternal) {
                if (isJSReplaced)
                    return false;

                var isProperty = methodInfo.DeclaringProperty != null;

                if (isProperty && methodInfo.DeclaringProperty.IsExternal)
                    return false;

                if (!isProperty || !methodInfo.Member.IsCompilerGenerated()) {
                } else {
                    isExternal = false;
                }
            }

            if (methodInfo.IsIgnored)
                return false;
            if (!method.HasBody && !isExternal && !methodIsProxied)
                return false;

            return true;
        }

        internal JSFunctionExpression GetFunctionBodyForMethod (bool isExternal, MethodInfo methodInfo) {
            if (!isExternal) {
                return FunctionCache.GetExpression(new QualifiedMemberIdentifier(
                    methodInfo.DeclaringType.Identifier,
                    methodInfo.Identifier
                ));
            }

            return null;
        }

        protected void EmitAndDefineMethod (
            DecompilerContext context, MethodReference methodRef, MethodDefinition method,
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter, bool stubbed,
            JSRawOutputIdentifier dollar, Cachers cachers,
            ref int nextDisambiguatedId, MethodInfo methodInfo = null, 
            Action<JSFunctionExpression> bodyTransformer = null
        ) {
            EmitMethodBody(
                context, methodRef, method,
                astEmitter, assemblyEmitter, stubbed, cachers,
                ref nextDisambiguatedId, methodInfo, bodyTransformer
            );
            assemblyEmitter.EmitMethodDefinition(
                context, methodRef, method, astEmitter, stubbed, dollar, methodInfo
            );
        }

        protected void EmitMethodBody (
            DecompilerContext context, MethodReference methodRef, MethodDefinition method,
            IAstEmitter astEmitter, IAssemblyEmitter assemblyEmitter, bool stubbed,
            Cachers cachers, ref int nextDisambiguatedId, MethodInfo methodInfo = null, 
            Action<JSFunctionExpression> bodyTransformer = null
        ) {
            if (methodInfo == null)
                methodInfo = TypeInfoProvider.GetMemberInformation<Internal.MethodInfo>(method);

            bool isExternal, isReplaced, methodIsProxied;

            if (!ShouldTranslateMethodBody(
                method, methodInfo, stubbed,
                out isExternal, out isReplaced, out methodIsProxied
            ))
                return;

            JSFunctionExpression function = GetFunctionBodyForMethod(
                isExternal, methodInfo
            );

            // FIXME
            astEmitter.SignatureCacher = cachers.Signature;

            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.EnclosingType = method.DeclaringType;
            astEmitter.ReferenceContext.EnclosingMethod = null;
            astEmitter.ReferenceContext.DefiningMethod = methodRef;

            assemblyEmitter.EmitSpacer();

            if (methodIsProxied)
                assemblyEmitter.EmitProxyComment(methodInfo.Member.DeclaringType.FullName);

            try {
                // Generating the function as a statement instead of an argument allows SpiderMonkey to apply more optimizations
                if (function != null) {
                    if (bodyTransformer != null)
                        bodyTransformer(function);

                    var displayName = String.Format("{0}.{1}", methodInfo.DeclaringType.Name, methodInfo.GetName(false));

                    // Disambiguate overloaded methods
                    if (methodInfo.IsOverloadedRecursive)
                        displayName += String.Format("${0:X2}", Interlocked.Increment(ref nextDisambiguatedId) - 1);

                    function.DisplayName = displayName;

                    assemblyEmitter.EmitFunctionBody(astEmitter, method, function);
                }
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }

        public void Dispose () {
            // _TypeInfoProvider.DumpSignatureCollectionStats();

            if (OwnsTypeInfoProvider)
                TypeInfoProvider.Dispose();

            FunctionCache.Dispose();

            if (OwnsAssemblyDataResolver)
                AssemblyDataResolver.Dispose();
        }

        public TypeInfoProvider GetTypeInfoProvider () {
            OwnsTypeInfoProvider = false;
            return TypeInfoProvider;
        }

        private SpecialIdentifiers _CachedSpecialIdentifiers;
        private object _CachedSpecialIdentifiersLock = new object();

        public SpecialIdentifiers GetSpecialIdentifiers (TypeSystem typeSystem) {
            lock (_CachedSpecialIdentifiersLock) {
                if (
                    (_CachedSpecialIdentifiers == null) ||
                    (_CachedSpecialIdentifiers.TypeSystem != typeSystem)
                )
                    _CachedSpecialIdentifiers = new JSIL.SpecialIdentifiers(FunctionCache.MethodTypes, typeSystem, TypeInfoProvider);
            }

            return _CachedSpecialIdentifiers;
        }

        private MethodSymbols ReadMethodSymbolsIfSourceMapEnabled(MethodDefinition methodDef)
        {
            var methodSymbols = Configuration.BuildSourceMap.GetValueOrDefault() && methodDef.Module.HasSymbols
                ? new MethodSymbols(methodDef.MetadataToken)
                : null;
            if (methodSymbols != null)
            {
                methodDef.Module.SymbolReader.Read(methodSymbols);
            }

            return methodSymbols;
        }
    }

    public abstract class BaseEmitterGroupFactory : IEmitterGroupFactory {
        private Action<TranslationResult> _postprocessors;

        public void RegisterPostprocessor (Action<TranslationResult> action) {
            _postprocessors += action;
        }

        public virtual void RunPostprocessors (AssemblyManifest manifest, string assemblyPath, TranslationResult result) {
            if (_postprocessors != null)
                _postprocessors(result);
        }

        public abstract IEnumerable<IAssemblyEmmitterFactory> MakeAssemblyEmitterFactory (AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly);
        public virtual IEnumerable<IAnalyzer> GetAnalyzers()
        {
            yield break;
        }

        public virtual Configuration FilterConfiguration(Configuration configuration)
        {
            return configuration;
        }
    }

    public class JavascriptEmitterGroupFactory : BaseEmitterGroupFactory {
        private const string TranslatorId = "JS";

        public override IEnumerable<IAssemblyEmmitterFactory> MakeAssemblyEmitterFactory (AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly) {
            return new IAssemblyEmmitterFactory[] {new JavascriptAssemblyEmmitterFactory(assemblyTranslator, assembly)};
        }

        public override void RunPostprocessors (AssemblyManifest manifest, string assemblyPath, TranslationResult result) {
            base.RunPostprocessors(manifest, assemblyPath, result);
            if (!result.Configuration.SkipManifestCreation.GetValueOrDefault(false)) {
                GenerateManifest(manifest, assemblyPath, result);
            }
        }

        private static void GenerateManifest (AssemblyManifest manifest, string assemblyPath, TranslationResult result) {
            using (var ms = new MemoryStream())
            using (var tw = new StreamWriter(ms, new UTF8Encoding(false))) {
                tw.WriteLine("// {0} {1}", AssemblyTranslator.GetHeaderText(), Environment.NewLine);
                tw.WriteLine("'use strict';");

                foreach (var kvp in manifest.Entries) {
                    tw.WriteLine(
                        "var {0} = JSIL.GetAssembly({1});",
                        kvp.Key, Util.EscapeString(kvp.Value, '\"')
                        );
                }

                if (result.Configuration.GenerateContentManifest.GetValueOrDefault(true)) {
                    tw.WriteLine();
                    tw.WriteLine("if (typeof (contentManifest) !== \"object\") { JSIL.GlobalNamespace.contentManifest = {}; };");
                    tw.WriteLine("contentManifest[\"" + Path.GetFileName(assemblyPath).Replace("\\", "\\\\") + "\"] = [");

                    foreach (var fe in result.OrderedFiles) {
                        var propertiesObject = FormatFileProperties(fe);

                        tw.WriteLine(String.Format(
                            "    [{0}, {1}, {2}],",
                            Util.EscapeString(fe.Type),
                            Util.EscapeString(fe.Filename.Replace("\\", "/")),
                            propertiesObject
                            ));
                    }

                    tw.WriteLine("];");
                }

                tw.Flush();

                result.AddFile("Manifest", Path.GetFileName(assemblyPath) + ".manifest.js", new ArraySegment<byte>(ms.GetBuffer(), 0, (int) ms.Length), 0);
            }
        }

        private static string FormatFileProperties (TranslationResult.ResultFile fe) {
            var result = "{ ";
            result += "\"sizeBytes\": ";
            result += fe.Size;

            if (fe.Properties != null)
                foreach (var kvp in fe.Properties) {
                    result += ", \"" + kvp.Key + "\": ";

                    if (kvp.Value is string)
                        result += Util.EscapeString((string) kvp.Value, forJson: true);
                    else
                        throw new NotImplementedException("File property of type '" + kvp.Value.GetType().Name);
                }

            result += " }";

            return result;
        }

        public class JavascriptAssemblyEmmitterFactory : IAssemblyEmmitterFactory {
            private readonly AssemblyTranslator assemblyTranslator;
            private readonly AssemblyDefinition assembly;

            public JavascriptAssemblyEmmitterFactory (AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly) {
                this.assemblyTranslator = assemblyTranslator;
                this.assembly = assembly;
            }

            public string Id {
                get { return TranslatorId; }
            }

            public string AssemblyPathAndFilename {
                get { return assembly.FullName + ".js"; }
            }

            public string ArtifactType {
                get { return "Script"; }
            }

            public IAssemblyEmitter MakeAssemblyEmitter (JavascriptFormatter formatter) {
                return new JavascriptAssemblyEmitter(assemblyTranslator, formatter);
            }
        }
    }


    public class DefinitelyTypedEmitterGroupFactory : BaseEmitterGroupFactory {
        private const string TranslatorId = "D-TS";

        public override Configuration FilterConfiguration(Configuration configuration) {
            configuration.InlineAssemblyReferences = true;
            return configuration;
        }

        public override IEnumerable<IAssemblyEmmitterFactory> MakeAssemblyEmitterFactory(AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly)
        {
            return new IAssemblyEmmitterFactory[] {
                new DefinitelyTypedInternalsEmitterFactory(assemblyTranslator, assembly),
                new DefinitelyTypedExportEmitterFactory(assemblyTranslator, assembly),
                new DefinitelyTypedModuleEmitterFactory(assemblyTranslator, assembly), 
            };
        }

        public override void RunPostprocessors(AssemblyManifest manifest, string assemblyPath, TranslationResult result)
        {
            base.RunPostprocessors(manifest, assemblyPath, result);
            var jsilPath = Path.GetDirectoryName(JSIL.Internal.Util.GetPathOfAssembly(Assembly.GetExecutingAssembly()));
            var searchPath = Path.Combine(jsilPath, "JS Libraries\\DefinitelyTyped\\");
            foreach (var file in Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories)) {
                var bytes = File.ReadAllBytes(file);
                result.AddFile("common",
                    new Uri(searchPath).MakeRelativeUri(new Uri(file)).ToString(),
                    new ArraySegment<byte>(bytes));
            }
        }

        public class DefinitelyTypedInternalsEmitterFactory : IAssemblyEmmitterFactory
        {
            private AssemblyTranslator assemblyTranslator;
            private AssemblyDefinition assembly;

            public DefinitelyTypedInternalsEmitterFactory(AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly)
            {
                this.assemblyTranslator = assemblyTranslator;
                this.assembly = assembly;
            }

            public string Id { get { return TranslatorId + "/Internals"; ; } }

            public string AssemblyPathAndFilename
            {
                get { return "internals/" + assembly.FullName + ".d.ts"; }
            }

            public string ArtifactType { get { return Id; } }

            public IAssemblyEmitter MakeAssemblyEmitter(JavascriptFormatter formatter)
            {
                return new DefinitelyTypedInternalsEmitter(assemblyTranslator, formatter);
            }
        }

        public class DefinitelyTypedExportEmitterFactory : IAssemblyEmmitterFactory
        {
            private AssemblyTranslator assemblyTranslator;
            private AssemblyDefinition assembly;

            public DefinitelyTypedExportEmitterFactory(AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly)
            {
                this.assemblyTranslator = assemblyTranslator;
                this.assembly = assembly;
            }

            public string Id { get { return TranslatorId + "/Exports"; ; } }

            public string AssemblyPathAndFilename
            {
                get { return "module." + assembly.FullName + ".d.ts"; }
            }

            public string ArtifactType { get { return Id; } }

            public IAssemblyEmitter MakeAssemblyEmitter(JavascriptFormatter formatter)
            {
                return new DefinitelyTypedExportEmitter(assemblyTranslator, formatter);
            }
        }

        public class DefinitelyTypedModuleEmitterFactory : IAssemblyEmmitterFactory
        {
            private AssemblyTranslator assemblyTranslator;
            private AssemblyDefinition assembly;

            public DefinitelyTypedModuleEmitterFactory(AssemblyTranslator assemblyTranslator, AssemblyDefinition assembly)
            {
                this.assemblyTranslator = assemblyTranslator;
                this.assembly = assembly;
            }

            public string Id { get { return TranslatorId + "/Module"; ; } }

            public string AssemblyPathAndFilename
            {
                get { return "module." + assembly.FullName + ".js"; }
            }

            public string ArtifactType { get { return Id; } }

            public IAssemblyEmitter MakeAssemblyEmitter(JavascriptFormatter formatter)
            {
                return new DefinitelyTypedModuleEmitter(assemblyTranslator, formatter);
            }
        }
    }
}
