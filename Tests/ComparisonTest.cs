using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JSIL.Internal;
using JSIL.Translator;
using NUnit.Framework;
using MethodInfo = System.Reflection.MethodInfo;

namespace JSIL.Tests {
    using System.Web.Script.Serialization;
    using Compiler.Extensibility;

    public class ComparisonTest : IDisposable {
        public static readonly bool UseAppDomains = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JsilUseAppDomainsInTest"));
        public float JavascriptExecutionTimeout = 15.0f;

        public static bool IsLinux
        {
            get
            {
                int p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static readonly Regex ElapsedRegex = new Regex(
            @"// elapsed: (?'elapsed'[0-9]+(\.[0-9]*)?)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
        public static readonly Regex ExceptionRegex = new Regex(
            @"(// EXCEPTION:)(?'errorText'.*)(// STACK:(?'stack'.*))(// ENDEXCEPTION)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline
        );

        public static readonly string JSILFolder;
        public static readonly string TestSourceFolder;
        public static readonly string JSShellPath;
        public static readonly string LoaderJSPath;
        public static readonly string EvaluatorSetupCode;
        public static readonly string EvaluatorRunCode;

        public static readonly string CurrentMetaRevision;

        public string StartupPrologue;

        public Func<string> GetTestRunnerQueryString = () => "";

        public readonly TypeInfoProvider TypeInfo;
        public readonly AssemblyCache AssemblyCache;
        public readonly string[] StubbedAssemblies;
        public readonly string[] JSFilenames;
        public readonly string OutputPath;
        public readonly string SourceDirectory;
        public readonly AssemblyUtility AssemblyUtility;
        public readonly Metacomment[] Metacomments;
        public readonly TimeSpan CompilationElapsed;
        public readonly bool     CompilationCacheHit;
        public readonly EvaluatorPool EvaluatorPool;

        private readonly AppDomain AssemblyAppDomain;
        private Evaluator Evaluator;

        static ComparisonTest () {
            var testAssembly = typeof(ComparisonTest).Assembly;
            var assemblyPath = Path.GetDirectoryName(Util.GetPathOfAssembly(testAssembly));

            JSILFolder = Path.GetDirectoryName(Util.GetPathOfAssembly(typeof(JSIL.AssemblyTranslator).Assembly));

            TestSourceFolder = Path.GetFullPath(Path.Combine(assemblyPath, "..", "Tests"));
            if (TestSourceFolder[TestSourceFolder.Length - 1] != Path.DirectorySeparatorChar)
                TestSourceFolder += Path.DirectorySeparatorChar;

            if (IsLinux) {
                JSShellPath = "js";
            } else {
                JSShellPath = Path.GetFullPath(Path.Combine(assemblyPath, "..", "Upstream", "SpiderMonkey", "js.exe"));
            }

            var librarySourceFolder = Path.GetFullPath(Path.Combine(TestSourceFolder, "..", "Libraries"));
            if (librarySourceFolder[librarySourceFolder.Length - 1] != Path.DirectorySeparatorChar)
                librarySourceFolder += Path.DirectorySeparatorChar;

            LoaderJSPath = Path.Combine(librarySourceFolder, @"JSIL.js");

            EvaluatorSetupCode = String.Format(
    @"var jsilConfig = {{
        libraryRoot: {0},
        environment: 'spidermonkey_shell'
    }};",
             Util.EscapeString(librarySourceFolder)
           );

            EvaluatorRunCode = String.Format(
    @"load({0});",
             Util.EscapeString(LoaderJSPath)
           );

            if (CompilerUtil.TryGetMetaVersion(out CurrentMetaRevision))
                Console.WriteLine("Using JSIL.Meta rev {0}", CurrentMetaRevision);
        }

        public static string MapSourceFileToTestFile (string sourceFile) {
            return Regex.Replace(
                sourceFile, "(\\.cs|\\.vb|\\.exe|\\.dll|\\.fs|\\.js|\\.il|\\.cpp)$", "$0.out"
            );
        }

        public static string EvaluatorPrepareEnvironmentCode(Dictionary<string, string> settings)
        {
            if (settings != null)
            {
                var jss = new JavaScriptSerializer();
                return string.Format("var jsilEnvironmentSettings = {0};", jss.Serialize(settings));
            }

            return string.Empty;
        }

        public ComparisonTest (
            EvaluatorPool pool,
            string filename, string[] stubbedAssemblies = null,
            TypeInfoProvider typeInfo = null, AssemblyCache assemblyCache = null
        )
            : this(
                  pool,
                  new[] { filename },
                  Path.Combine(
                      TestSourceFolder,
                      MapSourceFileToTestFile(filename)
                  ),
                  stubbedAssemblies, typeInfo, assemblyCache
              ) {
        }

        public ComparisonTest (
            EvaluatorPool pool,
            IEnumerable<string> filenames, string outputPath,
            string[] stubbedAssemblies = null, TypeInfoProvider typeInfo = null,
            AssemblyCache assemblyCache = null, string compilerOptions = ""
        ) {
            var started = DateTime.UtcNow.Ticks;
            OutputPath = outputPath;
            EvaluatorPool = pool;

            var extensions = (from f in filenames select Path.GetExtension(f).ToLower()).Distinct().ToArray();
            var absoluteFilenames = (from f in filenames select Path.Combine(TestSourceFolder, Portability.NormalizeDirectorySeparators(f)));

            if (extensions.Length != 1)
                throw new InvalidOperationException("Mixture of different source languages provided.");

            SourceDirectory = Path.GetDirectoryName(absoluteFilenames.First());

            var assemblyNamePrefix = Path.GetDirectoryName(outputPath).Split(new char[] { '\\', '/' }).Last();
            var assemblyName = Path.Combine(
                assemblyNamePrefix,
                Path.GetFileName(outputPath).Replace(".js", "")
            );

            JSFilenames = null;

            if (UseAppDomains)
                AssemblyAppDomain = AppDomain.CreateDomain("TestAssemblyDomain", null, new AppDomainSetup {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                });
            else
                AssemblyAppDomain = AppDomain.CurrentDomain;

            switch (extensions[0]) {
                case ".exe":
                case ".dll":
                    var fns = absoluteFilenames.ToArray();
                    if (fns.Length > 1)
                        throw new InvalidOperationException("Multiple binary assemblies provided.");
                    AssemblyUtility = CrossDomainHelper.CreateFromAssemblyPathOnRemoteDomain(AssemblyAppDomain, fns[0], extensions[0] == ".exe").AssemblyUtility;
                    break;
                case ".js":
                    JSFilenames = absoluteFilenames.ToArray();
                    Metacomments = null;
                    AssemblyUtility = null;
                    break;
                default:
                    bool ignore = false;
                    try
                    {
                        var helper = CrossDomainHelper.CreateFromCompileResultOnRemoteDomain(AssemblyAppDomain,
                            absoluteFilenames, assemblyName,
                            compilerOptions, CurrentMetaRevision);
                        Metacomments = helper.Metacomments;
                        AssemblyUtility = helper.AssemblyUtility;
                        CompilationCacheHit = helper.WasCached;
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (exception.InnerException is CompilerNotFoundException)
                        {
                            Assert.Ignore(exception.Message);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (CompilerNotFoundException exception)
                    {
                        Assert.Ignore(exception.Message);
                    }
                    break;
            }

            if (typeInfo != null)
                typeInfo.ClearCaches();

            StubbedAssemblies = stubbedAssemblies;
            TypeInfo = typeInfo;
            AssemblyCache = assemblyCache;

            var ended = DateTime.UtcNow.Ticks;
            CompilationElapsed = TimeSpan.FromTicks(ended - started);
        }

        public static string GetTestRunnerLink(IEnumerable<string> testFile, string queryString = "") {
            var rootPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(LoaderJSPath),
                @"..\"));

            var scriptFiles = string.Join(";", testFile.Select(item => MapSourceFileToTestFile(Path.GetFullPath(item))));

            var uri = new Uri(Path.Combine(rootPath, "test_runner.html"), UriKind.Absolute);

            return String.Format(
                "{0}?{1}#{2}", uri,
                queryString,
                scriptFiles
                    .Replace(rootPath, "")
                    .Replace("\\", "/"));
        }
        
        public static string GetTestRunnerLink (string testFile, string queryString = "") {
            return GetTestRunnerLink(Enumerable.Repeat(testFile, 1), queryString);
        }

        public void Dispose () {
            if (Evaluator != null)
                Evaluator.Dispose();

            if (AssemblyAppDomain != AppDomain.CurrentDomain) {
                var unloadSignal = new ManualResetEventSlim(false);
                ThreadPool.QueueUserWorkItem((_) => {
                    AppDomain.Unload(AssemblyAppDomain);
                    unloadSignal.Set();
                });

                unloadSignal.Wait(5000);
                if (!unloadSignal.IsSet)
                    throw new ThreadStateException("Timed out in AppDomain.Unload for test " + this.OutputPath);
            }
        }


        private int RunCSharpExecutable (string[] args, out string stdout, out string stderr) {
            var psi = new ProcessStartInfo(AssemblyUtility.AssemblyLocation, string.Join(" ", args)) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            string _stdout = null, _stderr = null;
            ManualResetEventSlim stdoutSignal, stderrSignal;
            stdoutSignal = new ManualResetEventSlim(false);
            stderrSignal = new ManualResetEventSlim(false);

            using (var process = Process.Start(psi)) {
                ThreadPool.QueueUserWorkItem((_) => {
                    try {
                        _stdout = process.StandardOutput.ReadToEnd();
                    } catch {
                    }
                    stdoutSignal.Set();
                });
                ThreadPool.QueueUserWorkItem((_) => {
                    try {
                        _stderr = process.StandardError.ReadToEnd();
                    } catch {
                    }
                    stderrSignal.Set();
                });

                stdoutSignal.Wait();
                stderrSignal.Wait();
                stdoutSignal.Dispose();
                stderrSignal.Dispose();

                stdout = _stdout;
                stderr = _stderr;

                process.WaitForExit();

                return process.ExitCode;
            }
        }

        public string RunCSharp (string[] args, out long elapsed) {
            string currentDir = null;

            try {
                lock (this) {
                    // FIXME: Not thread safe.
                    currentDir = Environment.CurrentDirectory;

                    // HACK: We chdir to the original location of the test. This ensures the test can find any DLLs it needs.
                    Environment.CurrentDirectory = this.SourceDirectory;
                }

                if (AssemblyUtility.AssemblyLocation.EndsWith(".exe")) {
                    long startedCs = DateTime.UtcNow.Ticks;

                    string stdout, stderr;
                    int exitCode = RunCSharpExecutable(args, out stdout, out stderr);

                    long endedCs = DateTime.UtcNow.Ticks;

                    elapsed = endedCs - startedCs;
                    if (exitCode != 0)
                        return String.Format("Process exited with code {0}\r\n{1}\r\n{2}", exitCode, stdout, stderr);
                    return stdout + stderr;
                } else
                {
                    string result = null;
                    long elapsedLocal = 0;
                    return AssemblyUtility.Run(args, out elapsed);
                }
            } finally {
                if (currentDir == null)
                    throw new InvalidOperationException();

                lock (this)
                    if (Environment.CurrentDirectory == this.SourceDirectory)
                        Environment.CurrentDirectory = currentDir;
            }
        }

        public static Configuration MakeDefaultConfiguration () {
            return new Configuration {
                FrameworkVersion = 4.0,
                IncludeDependencies = false,
                ApplyDefaults = false
            };
        }

        public static object MetacommentParseValue (string text, Type type) {
            var tNullable = typeof(Nullable<>);
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == tNullable)) {
                type = type.GetGenericArguments()[0];

                if (text == "null")
                    return null;
            }

            return Convert.ChangeType(text, type);
        }

        public Configuration ApplyMetacomments (Configuration configuration) {
            var result = new Configuration();
            configuration.MergeInto(result);

            if (this.Metacomments != null)
            foreach (var metacomment in this.Metacomments) {
                if (metacomment.Command != "jsiloption")
                    continue;

                object target = result;
                var parts = metacomment.Arguments.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                var key = parts[0].Split('.');
                var flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                if ((key.Length == 0) || (key.Length == 1 && String.IsNullOrWhiteSpace(key[0])))
                    throw new ArgumentException("key", "Key must specify a field/property name");

                for (var i = 0; i < key.Length; i++) {
                    var keyName = key[i];
                    var field = target.GetType().GetField(keyName, flags);
                    var property = target.GetType().GetProperty(keyName, flags);

                    if (i == key.Length - 1) {
                        if (field != null) {
                            field.SetValue(target, MetacommentParseValue(parts[1], field.FieldType));
                        } else if (property != null) {
                            property.SetValue(target, MetacommentParseValue(parts[1], property.PropertyType), null);
                        } else {
                            throw new KeyNotFoundException(keyName);
                        }
                    } else {
                        if (field != null) {
                            target = field.GetValue(target);
                        } else if (property != null) {
                            target = property.GetValue(target, null);
                        } else {
                            throw new KeyNotFoundException(keyName);
                        }
                    }
                }
            }

            return result;
        }

        public TOutput Translate<TOutput> (
            Func<TranslationResult, TOutput> processResult,
            Func<Configuration> makeConfiguration = null,
            Action<Exception> onTranslationFailure = null,
            Action<AssemblyTranslator> initializeTranslator = null,
            bool? scanForProxies = null,
            IEnumerable<IAnalyzer> analyzers = null
        ) {
            Configuration configuration;

            if (makeConfiguration != null)
                configuration = makeConfiguration();
            else
                configuration = MakeDefaultConfiguration();

            configuration = ApplyMetacomments(configuration);

            if (StubbedAssemblies != null)
                configuration.Assemblies.Stubbed.AddRange(StubbedAssemblies);

            TOutput result;

            using (var translator = new JSIL.AssemblyTranslator(
                configuration, TypeInfo, null, 
                assemblyDataResolver: new AssemblyDataResolver(configuration, AssemblyCache),
                analyzers: analyzers
            )) {
                if (initializeTranslator != null)
                    initializeTranslator(translator);

                var assemblyPath = AssemblyUtility.AssemblyLocation;
                TranslationResult translationResult = null;

                try {
                    translationResult = translator.Translate(
                        assemblyPath, scanForProxies == null ? TypeInfo == null : (bool)scanForProxies
                    ).TranslationResults[0];

                    result = processResult(translationResult);
                } finally {
                    if (onTranslationFailure != null) {
                        foreach (var failure in translator.Failures)
                            onTranslationFailure(failure);
                    }

                    // If we're using a preconstructed type information provider, we need to remove the type information
                    //  from the assembly we just translated
                    if ((TypeInfo != null) && (translationResult != null)) {
                        Assert.AreEqual(1, translationResult.Assemblies.Count);
                        TypeInfo.Remove(translationResult.Assemblies.ToArray());
                    }

                    // If we're using a preconstructed assembly cache, make sure the test case assembly didn't get into
                    //  the cache, since that would leak memory.
                    if (AssemblyCache != null) {
                        AssemblyCache.TryRemove(AssemblyUtility.AssemblyFullName);
                    }

                }
            }

            return result;
        }

        public string GenerateJavascript (
            string[] args, out Func<string> generateJavascript, out long elapsedTranslation,
            Func<Configuration> makeConfiguration = null,
            bool throwOnUnimplementedExternals = true,
            Action<Exception> onTranslationFailure = null,
            Action<AssemblyTranslator> initializeTranslator = null,
            bool? scanForProxies = null,
            bool shouldWritePrologue = true,
            IEnumerable<IAnalyzer> analyzers = null
        ) {
            var translationStarted = DateTime.UtcNow.Ticks;

            Action<Stream> writeResult;
            if ((AssemblyUtility == null) && (JSFilenames != null))
            {
                writeResult = stream =>
                {
                    using (var writer = new StreamWriter(stream,Encoding.UTF8, 1024, true))
                    {
                        writer.WriteLine();
                        writer.WriteLine();
                        foreach (var jsFilename in JSFilenames)
                        {
                            using (var reader = new StreamReader(jsFilename))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    writer.WriteLine(line);
                                }
                            }
                        }
                        writer.WriteLine();
                        writer.Flush();
                    }
                };
            } else if ((AssemblyUtility != null) && (JSFilenames == null)) {
                writeResult = Translate<Action<Stream>>(
                    tr => stream => tr.WriteToStream(stream), 
                    makeConfiguration, 
                    onTranslationFailure,
                    initializeTranslator,
                    scanForProxies,
                    analyzers
                );
            } else {
                throw new InvalidDataException("Provided both JS filenames and assembly");
            }

            elapsedTranslation = DateTime.UtcNow.Ticks - translationStarted;

            string testAssemblyName, testTypeName, testMethodName;

            bool mainAcceptsArguments = true;
            if (AssemblyUtility != null) {
                testAssemblyName = AssemblyUtility.AssemblyFullName;
                testTypeName = AssemblyUtility.EntryMethodTypeFullName;
                testMethodName = AssemblyUtility.EntryMethodName;
                mainAcceptsArguments = AssemblyUtility.EntryMethodAcceptsArguments;
            } else {
                testAssemblyName = "Test";
                testTypeName = "Program";
                testMethodName = "Main";
            }

            string argsJson;

            if (mainAcceptsArguments) {
                var jsonSerializer = new DataContractJsonSerializer(typeof(string[]));
                using (var ms2 = new MemoryStream()) {
                    jsonSerializer.WriteObject(ms2, args);
                    argsJson = Encoding.UTF8.GetString(ms2.GetBuffer(), 0, (int)ms2.Length);
                }
            } else {
                if ((args != null) && (args.Length > 0))
                    throw new ArgumentException("Test case does not accept arguments");

                argsJson = "[]";
            }

            var invocationJs = shouldWritePrologue ? String.Format(
                "var runTestCase = JSIL.Shell.TestPrologue(\r\n  {0}, \r\n  {1}, \r\n  {2}, \r\n  {3}, \r\n  {4}, \r\n  {5}\r\n);",
                JavascriptExecutionTimeout,
                Util.EscapeString(testAssemblyName),
                Util.EscapeString(testTypeName), 
                Util.EscapeString(testMethodName),
                argsJson,
                throwOnUnimplementedExternals ? "true" : "false"
            ) : String.Empty;

            var tempFilename = Path.GetTempFileName();
            using (var stream = File.Create(tempFilename))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("(function() {");
                    writer.Flush();
                    writeResult(stream);
                    writer.WriteLine();
                    writer.WriteLine("})()");
                    writer.WriteLine(invocationJs);
                    writer.Flush();
                }
            }

            var jsFile = OutputPath;
            if (File.Exists(jsFile))
                File.Delete(jsFile);
            File.Copy(tempFilename, jsFile);

            File.Delete(tempFilename);

            string generatedJavascript = null;
            generateJavascript = () =>
            {
                if (generatedJavascript == null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        writeResult(memoryStream);
                        memoryStream.Position = 0;
                        using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                        {
                            generatedJavascript = reader.ReadToEnd();
                        }
                    }
                }
                return generatedJavascript;
            };

            return OutputPath;
        }

        public string RunJavascript (
            string[] args, 
            Func<Configuration> makeConfiguration = null,
            Action<Exception> onTranslationFailure = null,
            Action<AssemblyTranslator> initializeTranslator = null,
            IEnumerable<IAnalyzer> analyzers = null
        ) {
            Func<string> temp1;
            string temp4, temp5;
            long temp2, temp3;

            return RunJavascript(
                args, 
                out temp1, out temp2, out temp3, out temp4, out temp5, 
                makeConfiguration: makeConfiguration, 
                onTranslationFailure: onTranslationFailure, 
                initializeTranslator: initializeTranslator, 
                analyzers: analyzers
            );
        }

        public string RunJavascript (
            string[] args, out Func<string> generateJavascript, out long elapsedTranslation, out long elapsedJs,
            Func<Configuration> makeConfiguration = null,
            JSEvaluationConfig evaluationConfig = null,
            Action<Exception> onTranslationFailure = null,
            Action<AssemblyTranslator> initializeTranslator = null,
            bool? scanForProxies = null,
            IEnumerable<IAnalyzer> analyzers = null
        ) {
            string temp1, temp2;

            return RunJavascript(
                args, out generateJavascript, out elapsedTranslation, out elapsedJs, 
                out temp1, out temp2,
                makeConfiguration: makeConfiguration, 
                evaluationConfig: evaluationConfig, 
                onTranslationFailure: onTranslationFailure, 
                initializeTranslator: initializeTranslator,
                scanForProxies: scanForProxies,
                analyzers: analyzers
            );
        }

        public string RunJavascript (
            string[] args, out Func<string> generateJavascript, out long elapsedTranslation, out long elapsedJs, out string stderr, out string trailingOutput,
            Func<Configuration> makeConfiguration = null,
            JSEvaluationConfig evaluationConfig = null,
            Action<Exception> onTranslationFailure = null,
            Action<AssemblyTranslator> initializeTranslator = null,
            bool? scanForProxies = null,
            IEnumerable<IAnalyzer> analyzers = null
        ) {
            var tempFilename = GenerateJavascript(
                args, out generateJavascript, out elapsedTranslation, 
                makeConfiguration,
                throwOnUnimplementedExternals:
                    evaluationConfig == null || evaluationConfig.ThrowOnUnimplementedExternals, 
                onTranslationFailure: onTranslationFailure,
                initializeTranslator: initializeTranslator,
                scanForProxies: scanForProxies,
                analyzers: analyzers
            );

            using (Evaluator = EvaluatorPool.Get()) {
                var startedJs = DateTime.UtcNow.Ticks;
                var sentinelStart = "// Test output begins here //";
                var sentinelEnd = "// Test output ends here //";
                var elapsedPrefix = "// elapsed: ";

                var manifest = String.Format(
                    "['Script', {0}]", Util.EscapeString(tempFilename)
                );

                if (evaluationConfig != null && evaluationConfig.AdditionalFilesToLoad != null)
                {
                    foreach (var file in evaluationConfig.AdditionalFilesToLoad)
                    {
                        manifest += "," + Environment.NewLine + String.Format("['Script', {0}]", Util.EscapeString(file));
                    }
                }

                var dlls = Directory.GetFiles(SourceDirectory, "*.emjs");
                foreach (var dll in dlls) {
                    manifest += "," + Environment.NewLine +
                        String.Format("['NativeLibrary', {0}]", Util.EscapeString(Path.GetFullPath(dll)));
                }

                StartupPrologue =
                    String.Format("contentManifest['Test'] = [{0}]; ", manifest);

                StartupPrologue += String.Format("function runMain () {{ " +
                    "print({0}); try {{ var elapsedTime = runTestCase(Date.now); }} catch (exc) {{ reportException(exc); }} print({1}); print({2} + elapsedTime);" +
                    "}}; shellStartup();",
                    Util.EscapeString(sentinelStart),
                    Util.EscapeString(sentinelEnd),
                    Util.EscapeString(elapsedPrefix)
                );

                Evaluator.WriteInput(StartupPrologue);

                Evaluator.Join();

                long endedJs = DateTime.UtcNow.Ticks;
                elapsedJs = endedJs - startedJs;

                if (Evaluator.ExitCode != 0) {
                    var _stdout = (Evaluator.StandardOutput ?? "").Trim();
                    var _stderr = (Evaluator.StandardError ?? "").Trim();

                    var exceptions = new List<JavaScriptException>();

                    var exceptionMatches = ExceptionRegex.Matches(_stdout);
                    foreach (Match match in exceptionMatches) {
                        var errorText = match.Groups["errorText"].Value;
                        string stackText = null;

                        if (match.Groups["stack"].Success)
                            stackText = match.Groups["stack"].Value;

                        var exception = new JavaScriptException(errorText, stackText);
                        exceptions.Add(exception);
                    }

                    throw new JavaScriptEvaluatorException(
                        Evaluator.ExitCode, _stdout, _stderr, exceptions.ToArray()
                    );
                }

                var stdout = Evaluator.StandardOutput;

                if (stdout != null) {
                    var m = ElapsedRegex.Match(stdout);
                    if (m.Success) {
                        elapsedJs = TimeSpan.FromMilliseconds(
                            double.Parse(m.Groups["elapsed"].Value, CultureInfo.InvariantCulture)
                        ).Ticks;
                        stdout = stdout.Replace(m.Value, "");
                    }
                }

                // Strip spurious output from the JS.exe REPL and from the standard libraries
                trailingOutput = null;
                if (stdout != null) {
                    var startOffset = stdout.LastIndexOf(sentinelStart);

                    if (startOffset >= 0) {
                        startOffset += sentinelStart.Length;

                        // End sentinel might not be there if the test case calls quit().
                        var endOffset = stdout.IndexOf(sentinelEnd, startOffset);
                        if (endOffset >= 0) {
                            trailingOutput = stdout.Substring(endOffset + sentinelEnd.Length);
                            stdout = stdout.Substring(startOffset, endOffset - startOffset);
                        } else {
                            stdout = stdout.Substring(startOffset);
                        }
                    }
                }

                stderr = Evaluator.StandardError;

                return stdout ?? "";
            }
        }

        public void Run (
            string[] args = null, 
            Func<Configuration> makeConfiguration = null, 
            JSEvaluationConfig evaluationConfig = null,
            bool dumpJsOnFailure = true,
            Action<Exception> onTranslationFailure = null,
            Action<AssemblyTranslator> initializeTranslator = null,
            bool? scanForProxies = null
        ) {
            var signals = new[] {
                    new ManualResetEventSlim(false), new ManualResetEventSlim(false)
                };
            Func<string> generateJs = null;
            var errors = new Exception[2];
            var outputs = new string[2];
            var elapsed = new long[3];

            args = args ?? new string[0];

            if (AssemblyUtility != null) {
                ThreadPool.QueueUserWorkItem((_) => {
                    var oldCulture = Thread.CurrentThread.CurrentCulture;
                    try {
                        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                        outputs[0] = RunCSharp(args, out elapsed[0]).Replace("\r", "").Trim();
                    } catch (Exception ex) {
                        errors[0] = ex;
                    } finally {
                        Thread.CurrentThread.CurrentCulture = oldCulture;
                    }
                    signals[0].Set();
                });
            } else {
                outputs[0] = "";
                signals[0].Set();
            }

            ThreadPool.QueueUserWorkItem((_) => {
                try {
                    outputs[1] = RunJavascript(
                        args, out generateJs, out elapsed[1], out elapsed[2], 
                        makeConfiguration: makeConfiguration,
                        evaluationConfig: evaluationConfig,
                        onTranslationFailure: onTranslationFailure,
                        initializeTranslator: initializeTranslator,
                        scanForProxies: scanForProxies
                    ).Replace("\r", "").Trim();
                } catch (Exception ex) {
                    errors[1] = ex;
                }
                signals[1].Set();
            });

            signals[0].Wait();
            signals[1].Wait();

            const int truncateThreshold = 4096;

            Action writeJSOutput = () => {
                Console.WriteLine("// JavaScript output begins //");
                if (outputs[1].Length > truncateThreshold) {
                    Console.WriteLine(outputs[1].Substring(0, truncateThreshold));
                    Console.WriteLine("(truncated)");
                } else
                    Console.WriteLine(outputs[1]);
            };

            try {
                if (errors[0] != null)
                    throw new Exception("C# test failed", errors[0]);
                else if (errors[1] != null)
                    throw errors[1];
                else if (AssemblyUtility != null)
                    Assert.AreEqual(outputs[0], outputs[1]);
                else
                    Console.WriteLine("// Output validation suppressed (raw JS)");

                if (AssemblyUtility == null)
                    writeJSOutput();

                string compileTime;
                if (CompilationCacheHit)
                    compileTime = "cached";
                else
                    compileTime = string.Format("{0:0000}ms", CompilationElapsed.TotalMilliseconds);

                Console.WriteLine(
                    "passed: CL:{0} TR:{2:0000}ms C#:{1:0000}ms JS:{3:0000}ms",
                    compileTime,
                    TimeSpan.FromTicks(elapsed[0]).TotalMilliseconds,
                    TimeSpan.FromTicks(elapsed[1]).TotalMilliseconds,
                    TimeSpan.FromTicks(elapsed[2]).TotalMilliseconds
                );
            } catch (Exception ex) {
                var jsex = ex as JavaScriptEvaluatorException;
                Console.WriteLine("failed: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message));

                var queryString = string.Join(
                    "&",
                    (EvaluatorPool.EnvironmentVariables ?? new Dictionary<string, string>())
                        .Select(item => string.Format("{0}={1}", item.Key, item.Value))
                        .Concat(Enumerable.Repeat(GetTestRunnerQueryString(), 1))
                        .Where(str => !string.IsNullOrEmpty(str))
                        .ToArray());

                var files = new List<string> { OutputPath };

                if (evaluationConfig != null) {
                    if (evaluationConfig.AdditionalFilesToLoad != null)
                        files.AddRange(evaluationConfig.AdditionalFilesToLoad);
                }

                Console.WriteLine("// {0}", GetTestRunnerLink(files, queryString));

                if ((outputs[1] == null) && (jsex != null))
                    outputs[1] = jsex.Output;

                if ((jsex != null) && (jsex.Exceptions.Length > 0)) {
                    Console.WriteLine("// JS exceptions begin //");
                    foreach (var exc in jsex.Exceptions)
                        Console.WriteLine(exc);
                }

                if (outputs[0] != null) {
                    Console.WriteLine("// C# output begins //");
                    if (outputs[0].Length > truncateThreshold) {
                        Console.WriteLine(outputs[0].Substring(0, truncateThreshold));
                        Console.WriteLine("(truncated)");
                    } else
                        Console.WriteLine(outputs[0]);
                }
                if (outputs[1] != null) {
                    writeJSOutput();
                }

                if (dumpJsOnFailure && (generateJs != null)) {
                    Console.WriteLine("// Generated javascript begins here //");
                    Console.WriteLine(generateJs());
                    Console.WriteLine("// Generated javascript ends here //");
                }

                throw;
            }
        }
    }

    public class JSEvaluationConfig
    {
        public bool ThrowOnUnimplementedExternals { get; set; }

        public string[] AdditionalFilesToLoad { get; set; }
    }

    public class CrossDomainHelper : MarshalByRefObject
    {
        private readonly AssemblyUtility _assemblyUtility;
        private readonly Metacomment[] _metacomments;
        private readonly bool _wasCached;

        public CrossDomainHelper(string[] absoluteFilenames, string assemblyName, string compilerOptions, string currentMetaRevision)
        {
            var compileResult = CompilerUtil.Compile(absoluteFilenames, assemblyName, compilerOptions, currentMetaRevision);
            _assemblyUtility = new AssemblyUtility(compileResult.Assembly);
            _metacomments = compileResult.Metacomments;
            _wasCached = compileResult.WasCached;
        }

        public CrossDomainHelper(string assemblyPath, bool reflectionOnly)
        {
            _assemblyUtility = new AssemblyUtility(assemblyPath, reflectionOnly);
        }

        public static CrossDomainHelper CreateFromAssemblyPathOnRemoteDomain(AppDomain domain, string assemblyPath,
            bool reflectionOnly)
        {
            return domain == AppDomain.CurrentDomain
                ? new CrossDomainHelper(assemblyPath, reflectionOnly)
                : (CrossDomainHelper) domain.CreateInstanceFromAndUnwrap(
                    typeof (CrossDomainHelper).Assembly.Location,
                    typeof (CrossDomainHelper).FullName,
                    false,
                    BindingFlags.CreateInstance,
                    null,
                    new object[] {assemblyPath, reflectionOnly},
                    CultureInfo.InvariantCulture,
                    null
                    );
        }

        public static CrossDomainHelper CreateFromCompileResultOnRemoteDomain(AppDomain domain, IEnumerable<string> absoluteFilenames, string assemblyName, string compilerOptions, string currentMetaRevision)
        {
            return domain == AppDomain.CurrentDomain
                ? new CrossDomainHelper(absoluteFilenames.ToArray(), assemblyName, compilerOptions, currentMetaRevision)
                : (CrossDomainHelper) domain.CreateInstanceFromAndUnwrap(
                    typeof (CrossDomainHelper).Assembly.Location,
                    typeof (CrossDomainHelper).FullName,
                    false,
                    BindingFlags.CreateInstance,
                    null,
                    new object[] {absoluteFilenames.ToArray(), assemblyName, compilerOptions, currentMetaRevision},
                    CultureInfo.InvariantCulture,
                    null
                    );
        }

        public AssemblyUtility AssemblyUtility
        {
            get { return _assemblyUtility; }
        }

        public Metacomment[] Metacomments
        {
            get { return _metacomments; }
        }

        public bool WasCached {
            get { return _wasCached; }
        }
    }

    public class AssemblyUtility : MarshalByRefObject
    {
        private readonly Assembly _assembly;
        private readonly MethodInfo _entryPoint;

        public AssemblyUtility(Assembly assembly)
        {
            _assembly = assembly;
            _entryPoint = GetTestMethod();
        }

        public AssemblyUtility(string assemblyPath, bool reflectionOnly)
            : this(reflectionOnly ? Assembly.ReflectionOnlyLoadFrom(assemblyPath) : Assembly.LoadFrom(assemblyPath))
        { }

        public string AssemblyLocation
        {
            get { return _assembly.Location; }
        }

        public string AssemblyFullName
        {
            get { return _assembly.FullName; }
        }

        public string EntryMethodName
        {
            get { return _entryPoint.Name; }
        }

        public string EntryMethodTypeFullName
        {
            get { return _entryPoint.DeclaringType.FullName; }
        }

        public bool EntryMethodAcceptsArguments
        {
            get { return _entryPoint != null ? _entryPoint.GetParameters().Length > 0 : false; }
        }

        public string Run(string[] args, out long elapsed)
        {
            TextWriter oldStdout = null;
            using (var sw = new StringWriter())
            {
                oldStdout = Console.Out;
                try
                {
                    oldStdout.Flush();
                    Console.SetOut(sw);

                    long startedCs = DateTime.UtcNow.Ticks;

                    if (EntryMethodAcceptsArguments)
                    {
                        _entryPoint.Invoke(null, new object[] { args });
                    }
                    else
                    {
                        if ((args != null) && (args.Length > 0))
                            throw new ArgumentException("Test case does not accept arguments");

                        _entryPoint.Invoke(null, new object[] { });
                    }

                    long endedCs = DateTime.UtcNow.Ticks;

                    elapsed = endedCs - startedCs;
                    sw.Flush();
                    return sw.ToString();
                }
                finally
                {
                    Console.SetOut(oldStdout);
                }
            }
        }

        private MethodInfo GetTestMethod()
        {
            var entryPoint = _assembly.EntryPoint;

            if (entryPoint == null)
            {
                var program = _assembly.GetType("Program");
                if (program == null)
                    throw new Exception("Test missing 'Program' main class");

                var testMethod = program.GetMethod("Main");
                if (testMethod == null)
                    throw new Exception("Test missing 'Main' method of 'Program' main class");

                entryPoint = testMethod;
            }

            return entryPoint;
        }
    }
}