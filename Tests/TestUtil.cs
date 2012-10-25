using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JSIL.Internal;
using JSIL.Translator;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Threading;
using Microsoft.VisualBasic;
using NUnit.Framework;
using System.Globalization;
using Microsoft.Win32;
using System.Web.Script.Serialization;
using MethodInfo = System.Reflection.MethodInfo;

namespace JSIL.Tests {
    public class JavaScriptEvaluatorException : Exception {
        public readonly int ExitCode;
        public readonly string ErrorText;
        public readonly string Output;
        public readonly JavaScriptException[] Exceptions;

        public JavaScriptEvaluatorException (int exitCode, string stdout, string stderr, JavaScriptException[] exceptions)
            : base(String.Format("JavaScript interpreter exited with code {0} after throwing {3} exception(s):\r\n{1}\r\n{2}", exitCode, stdout, stderr, exceptions.Length)) 
        {
            ExitCode = exitCode;
            ErrorText = stderr;
            Output = stdout;
            Exceptions = exceptions;
        }
    }

    public class JavaScriptException : Exception {
        new public readonly string Message;
        public readonly string Stack;

        private static string FormatMessage (string message, string stack) {
            if (stack != null)
                return String.Format("JavaScript Exception: {0}\r\n{1}", message, stack);
            else
                return String.Format("JavaScript Exception: {0}", message);
        }

        public JavaScriptException (string message, string stack) 
            : base (FormatMessage(message, stack)) {
            Message = message;
            Stack = stack;
        }
    }

    public static class CompilerUtil {
        public static string TempPath;

        // Attempt to clean up stray assembly files from previous test runs
        //  since the assemblies would have remained locked and undeletable 
        //  due to being loaded
        static CompilerUtil () {
            TempPath = Path.Combine(Path.GetTempPath(), "JSIL Tests");
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            foreach (var filename in Directory.GetFiles(TempPath))
                try {
                    File.Delete(filename);
                } catch {
                }
        }

        public static Assembly CompileCS (
            IEnumerable<string> filenames, string assemblyName
        ) {
            return Compile(
                () => new CSharpCodeProvider(new Dictionary<string, string>() { 
                    { "CompilerVersion", "v4.0" } 
                }),
                filenames, assemblyName
            );
        }

        public static Assembly CompileVB (
            IEnumerable<string> filenames, string assemblyName
        ) {
            return Compile(
                () => new VBCodeProvider(new Dictionary<string, string>() { 
                    { "CompilerVersion", "v4.0" } 
                }), 
                filenames, assemblyName
            );
        }

        private static bool CheckCompileManifest (IEnumerable<string> inputs, string outputDirectory) {
            var manifestPath = Path.Combine(outputDirectory, "compileManifest.json");
            if (!File.Exists(manifestPath))
                return false;

            var jss = new JavaScriptSerializer();
            var manifest = jss.Deserialize<Dictionary<string, string>>(File.ReadAllText(manifestPath));

            foreach (var input in inputs) {
                var fi = new FileInfo(input);
                var key = Path.GetFileName(input);

                if (!manifest.ContainsKey(key))
                    return false;

                var previousTimestamp = DateTime.Parse(manifest[key]);

                var delta = fi.LastWriteTime - previousTimestamp;
                if (Math.Abs(delta.TotalSeconds) >= 1) {
                    return false;
                }
            }

            return true;
        }

        private static void WriteCompileManifest (IEnumerable<string> inputs, string outputDirectory) {
            var manifest = new Dictionary<string, string>();

            foreach (var input in inputs) {
                var fi = new FileInfo(input);
                var key = Path.GetFileName(input);
                manifest[key] = fi.LastWriteTime.ToString("O");
            }

            var manifestPath = Path.Combine(outputDirectory, "compileManifest.json");
            var jss = new JavaScriptSerializer();
            File.WriteAllText(manifestPath, jss.Serialize(manifest));
        }

        private static Assembly Compile (
            Func<CodeDomProvider> getProvider, IEnumerable<string> filenames, string assemblyName
        ) {
            var tempPath = Path.Combine(TempPath, assemblyName);
            Directory.CreateDirectory(tempPath);

            var outputAssembly = Path.Combine(
                tempPath,
                Path.GetFileNameWithoutExtension(assemblyName) + ".dll"
            );

            if (
                File.Exists(outputAssembly) &&
                CheckCompileManifest(filenames, tempPath)
            ) {
                return Assembly.LoadFile(outputAssembly);
            }

            var files = Directory.GetFiles(tempPath);
            foreach (var file in files) {
                try {
                    File.Delete(file);
                } catch {
                }
            }

            var parameters = new CompilerParameters(new[] {
                "mscorlib.dll", "System.dll", 
                "System.Core.dll", "System.Xml.dll", 
                "Microsoft.CSharp.dll",
                typeof(JSIL.Meta.JSIgnore).Assembly.Location
            }) {
                CompilerOptions = "/unsafe",
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(tempPath, true),
                OutputAssembly = outputAssembly
            };

            CompilerResults results;
            using (var provider = getProvider()) {
                results = provider.CompileAssemblyFromFile(
                    parameters,
                    filenames.ToArray()
                );
            }

            if (results.Errors.Count > 0) {
                throw new Exception(
                    String.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select((ce) => ce.ToString()).ToArray())
                );
            }

            WriteCompileManifest(filenames, tempPath);

            return results.CompiledAssembly;
        }
    }

    public class ComparisonTest : IDisposable {
        public float JavascriptExecutionTimeout = 30.0f;

        public static readonly Regex ElapsedRegex = new Regex(
            @"// elapsed: (?'elapsed'[0-9]+(\.[0-9]*)?)", 
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
        public static readonly Regex ExceptionRegex = new Regex(
            @"(// EXCEPTION:)(?'errorText'.*)(// STACK:(?'stack'.*))(// ENDEXCEPTION)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline
        );

        public static readonly string TestSourceFolder;
        public static readonly string JSShellPath;
        public static readonly string LoaderJSPath;
        public static readonly string EvaluatorSetupCode;

        public readonly TypeInfoProvider TypeInfo;
        public readonly AssemblyCache AssemblyCache;
        public readonly string[] StubbedAssemblies;
        public readonly string OutputPath;
        public readonly Assembly Assembly;
        public readonly TimeSpan CompilationElapsed;
        public readonly EvaluatorPool EvaluatorPool;

        protected bool? MainAcceptsArguments;

        static ComparisonTest () {
            var testAssembly = typeof(ComparisonTest).Assembly;
            var assemblyPath = Path.GetDirectoryName(Util.GetPathOfAssembly(testAssembly));

            TestSourceFolder = Path.GetFullPath(Path.Combine(assemblyPath, @"..\Tests\"));
            JSShellPath = Path.GetFullPath(Path.Combine(assemblyPath, @"..\Upstream\SpiderMonkey\js.exe"));

            var librarySourceFolder = Path.GetFullPath(Path.Combine(TestSourceFolder, @"..\Libraries\"));

            LoaderJSPath = Path.Combine(librarySourceFolder, @"JSIL.js");

            EvaluatorSetupCode = String.Format(
@"var jsilConfig = {{
    libraryRoot: {1},
    environment: 'spidermonkey_shell'
}}; load({0});",
             Util.EscapeString(LoaderJSPath),
             Util.EscapeString(librarySourceFolder)
           );
        }

        public static string MapSourceFileToTestFile (string sourceFile) {
            return Regex.Replace(
                sourceFile, "(\\.cs|\\.vb|\\.exe|\\.dll)$", "$0.js"
            );
        }

        public ComparisonTest (
            EvaluatorPool pool, 
            string filename, string[] stubbedAssemblies = null, 
            TypeInfoProvider typeInfo = null, AssemblyCache assemblyCache = null
        ) : this (
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
            AssemblyCache assemblyCache = null
        ) {
            var started = DateTime.UtcNow.Ticks;
            OutputPath = outputPath;
            EvaluatorPool = pool;

            var extensions = (from f in filenames select Path.GetExtension(f).ToLower()).Distinct().ToArray();
            var absoluteFilenames = (from f in filenames select Path.Combine(TestSourceFolder, f));

            if (extensions.Length != 1)
                throw new InvalidOperationException("Mixture of different source languages provided.");

            var assemblyNamePrefix = Path.GetDirectoryName(outputPath).Split(new char[] { '\\', '/' }).Last();
            var assemblyName = Path.Combine(
                assemblyNamePrefix,
                Path.GetFileName(outputPath).Replace(".js", "")
            );

            switch (extensions[0]) {
                case ".cs":
                    Assembly = CompilerUtil.CompileCS(absoluteFilenames, assemblyName);
                    break;
                case ".vb":
                    Assembly = CompilerUtil.CompileVB(absoluteFilenames, assemblyName);
                    break;
                case ".exe":
                case ".dll":
                    var fns = absoluteFilenames.ToArray();
                    if (fns.Length > 1)
                        throw new InvalidOperationException("Multiple binary assemblies provided.");

                    Assembly = Assembly.LoadFile(fns[0]);
                    break;
                default:
                    throw new ArgumentException("Unsupported source file type for test");
            }

            if (typeInfo != null)
                typeInfo.ClearCaches();

            StubbedAssemblies = stubbedAssemblies;
            TypeInfo = typeInfo;
            AssemblyCache = assemblyCache;

            var ended = DateTime.UtcNow.Ticks;
            CompilationElapsed = TimeSpan.FromTicks(ended - started);
        }

        public static string GetTestRunnerLink (string testFile) {
            var rootPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(LoaderJSPath),
                @"..\"
            ));

            var uri = new Uri(
                Path.Combine(rootPath, "test_runner.html"), UriKind.Absolute
            );

            return String.Format(
                "{0}#{1}", uri,
                MapSourceFileToTestFile(Path.GetFullPath(testFile))
                    .Replace(rootPath, "")
                    .Replace("\\", "/")
            );
        }

        public void Dispose () {
        }

        protected MethodInfo GetTestMethod () {
            var entryPoint = Assembly.EntryPoint;
            
            if (entryPoint == null) {
                var program = Assembly.GetType("Program");
                if (program == null)
                    throw new Exception("Test missing 'Program' main class");

                var testMethod = program.GetMethod("Main");
                if (testMethod == null)
                    throw new Exception("Test missing 'Main' method of 'Program' main class");

                entryPoint = testMethod;
            }

            MainAcceptsArguments = entryPoint.GetParameters().Length > 0;

            return entryPoint;
        }

        public string RunCSharp (string[] args, out long elapsed) {
            TextWriter oldStdout = null;

            using (var sw = new StringWriter()) {
                oldStdout = Console.Out;
                try {
                    oldStdout.Flush();
                    Console.SetOut(sw);

                    var testMethod = GetTestMethod();
                    long startedCs = DateTime.UtcNow.Ticks;

                    if (MainAcceptsArguments.Value) {
                        testMethod.Invoke(null, new object[] { args });
                    } else {
                        if ((args != null) && (args.Length > 0))
                            throw new ArgumentException("Test case does not accept arguments");

                        testMethod.Invoke(null, new object[] { });
                    }

                    long endedCs = DateTime.UtcNow.Ticks;

                    elapsed = endedCs - startedCs;
                    sw.Flush();
                    return sw.ToString();
                } finally {
                    Console.SetOut(oldStdout);
                }
            }
        }

        public static Configuration MakeDefaultConfiguration () {
            return new Configuration {
                FrameworkVersion = 4.0,
                IncludeDependencies = false,
                ApplyDefaults = false
            };
        }

        public string GenerateJavascript (
            string[] args, out string generatedJavascript, out long elapsedTranslation,
            Func<Configuration> makeConfiguration = null
        ) {
            var tempFilename = Path.GetTempFileName();
            Configuration configuration;

            if (makeConfiguration != null)
                configuration = makeConfiguration();
            else
                configuration = MakeDefaultConfiguration();

            if (StubbedAssemblies != null)
                configuration.Assemblies.Stubbed.AddRange(StubbedAssemblies);

            var translator = new JSIL.AssemblyTranslator(configuration, TypeInfo, null, AssemblyCache);

            string translatedJs;
            var translationStarted = DateTime.UtcNow.Ticks;
            var assemblyPath = Util.GetPathOfAssembly(Assembly);
            translatedJs = null;
            try {
                var result = translator.Translate(
                    assemblyPath, TypeInfo == null
                );

                AssemblyTranslator.GenerateManifest(translator.Manifest, assemblyPath, result);
                translatedJs = result.WriteToString();

                // If we're using a preconstructed type information provider, we need to remove the type information
                //  from the assembly we just translated
                if (TypeInfo != null) {
                    Assert.AreEqual(1, result.Assemblies.Count);
                    TypeInfo.Remove(result.Assemblies.ToArray());
                }

                // If we're using a preconstructed assembly cache, make sure the test case assembly didn't get into
                //  the cache, since that would leak memory.
                if (AssemblyCache != null) {
                    AssemblyCache.TryRemove(Assembly.FullName);
                }
            } finally {
                translator.Dispose();
            }

            elapsedTranslation = DateTime.UtcNow.Ticks - translationStarted;

            var testMethod = GetTestMethod();
            var declaringType = JSIL.Internal.Util.EscapeIdentifier(
                testMethod.DeclaringType.FullName, Internal.EscapingMode.TypeIdentifier
            );

            string argsJson;

            if (MainAcceptsArguments.Value) {
                var jsonSerializer = new DataContractJsonSerializer(typeof(string[]));
                using (var ms2 = new MemoryStream()) {
                    jsonSerializer.WriteObject(ms2, args);
                    argsJson = Encoding.UTF8.GetString(ms2.GetBuffer(), 0, (int)ms2.Length);
                }
            } else {
                if ((args != null) && (args.Length > 0))
                    throw new ArgumentException("Test case does not accept arguments");

                argsJson = "";
            }

            var invocationJs = String.Format(
                "function runTestCase (timeout, elapsed) {{\r\n" +
                "  timeout({0});\r\n" +
                "  var started = elapsed();\r\n" +
                "  var testAssembly = JSIL.GetAssembly({1}, true);\r\n" +
                "  testAssembly.{2}.{3}({4});\r\n" +
                "  var ended = elapsed();\r\n" +
                "  return ended - started;\r\n" +
                "}}",
                JavascriptExecutionTimeout, 
                Util.EscapeString(testMethod.Module.Assembly.FullName),
                declaringType, Util.EscapeIdentifier(testMethod.Name), 
                argsJson
            );

            generatedJavascript = translatedJs;

            File.WriteAllText(tempFilename, translatedJs + Environment.NewLine + invocationJs);

            var jsFile = OutputPath;
            if (File.Exists(jsFile))
                File.Delete(jsFile);
            File.Copy(tempFilename, jsFile);

            File.Delete(tempFilename);

            return OutputPath;
        }

        public string RunJavascript (
            string[] args, out string generatedJavascript, out long elapsedTranslation, out long elapsedJs,
            Func<Configuration> makeConfiguration = null
        ) {
            var tempFilename = GenerateJavascript(args, out generatedJavascript, out elapsedTranslation, makeConfiguration);

            using (var evaluator = EvaluatorPool.Get()) {
                var startedJs = DateTime.UtcNow.Ticks;
                var sentinelStart = "// Test output begins here //";
                var sentinelEnd = "// Test output ends here //";
                var elapsedPrefix = "// elapsed: ";

                evaluator.WriteInput(
                    "contentManifest['Test'] = [['Script', {0}]]; " +
                    "function runMain () {{ " +
                    "print({1}); try {{ var elapsedTime = runTestCase(timeout, elapsed); }} catch (exc) {{ reportException(exc); }} print({2}); print({3} + elapsedTime);" +
                    "}}; shellStartup();",
                    Util.EscapeString(tempFilename),
                    Util.EscapeString(sentinelStart),
                    Util.EscapeString(sentinelEnd),
                    Util.EscapeString(elapsedPrefix)
                );

                evaluator.Join();

                long endedJs = DateTime.UtcNow.Ticks;
                elapsedJs = endedJs - startedJs;

                if (evaluator.ExitCode != 0) {
                    var _stdout = (evaluator.StandardOutput ?? "").Trim();
                    var _stderr = (evaluator.StandardError ?? "").Trim();

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
                        evaluator.ExitCode, _stdout, _stderr, exceptions.ToArray()
                    );
                }

                var stdout = evaluator.StandardOutput;

                if (stdout != null) {
                    var m = ElapsedRegex.Match(stdout);
                    if (m.Success) {
                        elapsedJs = TimeSpan.FromMilliseconds(
                            double.Parse(m.Groups["elapsed"].Value)
                        ).Ticks;
                        stdout = stdout.Replace(m.Value, "");
                    }
                }

                // Strip spurious output from the JS.exe REPL and from the standard libraries
                if (stdout != null) {
                    var startOffset = stdout.IndexOf(sentinelStart);

                    if (startOffset >= 0) {
                        startOffset += sentinelStart.Length;

                        // End sentinel might not be there if the test case calls quit().
                        var endOffset = stdout.IndexOf(sentinelEnd, startOffset);
                        if (endOffset >= 0) {
                            stdout = stdout.Substring(startOffset, endOffset - startOffset);
                        } else {
                            stdout = stdout.Substring(startOffset);
                        }
                    }
                }

                return stdout ?? "";
            }
        }

        public void Run (params string[] args) {
            var signals = new[] {
                new ManualResetEventSlim(false), new ManualResetEventSlim(false)
            };
            var generatedJs = new string[1];
            var errors = new Exception[2];
            var outputs = new string[2];
            var elapsed = new long[3];

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

            ThreadPool.QueueUserWorkItem((_) => {
                try {
                    outputs[1] = RunJavascript(args, out generatedJs[0], out elapsed[1], out elapsed[2]).Replace("\r", "").Trim();
                } catch (Exception ex) {
                    errors[1] = ex;
                }
                signals[1].Set();
            });

            signals[0].Wait();
            signals[1].Wait();

            try {
                if (errors[0] != null)
                    throw new Exception("C# test failed", errors[0]);
                else if (errors[1] != null)
                    throw errors[1];
                else
                    Assert.AreEqual(outputs[0], outputs[1]);

                Console.WriteLine(
                    "passed: CL:{0:0000}ms TR:{2:0000}ms C#:{1:0000}ms JS:{3:0000}ms",
                    CompilationElapsed.TotalMilliseconds,
                    TimeSpan.FromTicks(elapsed[0]).TotalMilliseconds,
                    TimeSpan.FromTicks(elapsed[1]).TotalMilliseconds,
                    TimeSpan.FromTicks(elapsed[2]).TotalMilliseconds
                );
            } catch (Exception ex){
                Console.WriteLine("failed: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message));

                Console.WriteLine("// {0}", GetTestRunnerLink(OutputPath));

                if (outputs[0] != null) {
                    Console.WriteLine("// C# output begins //");
                    Console.WriteLine(outputs[0]);
                }
                if (outputs[1] != null) {
                    Console.WriteLine("// JavaScript output begins //");
                    Console.WriteLine(outputs[1]);
                }
                if (generatedJs[0] != null) {
                    Console.WriteLine("// Generated javascript begins here //");
                    Console.WriteLine(generatedJs[0]);
                    Console.WriteLine("// Generated javascript ends here //");
                }

                throw;
            }
        }
    }

    public class GenericTestFixture {
        public EvaluatorPool EvaluatorPool {
            get;
            private set;
        }

        [TestFixtureSetUp]
        public void FixtureSetUp () {
            EvaluatorPool = new EvaluatorPool(
                ComparisonTest.JSShellPath, "",
                (e) =>
                    e.WriteInput(ComparisonTest.EvaluatorSetupCode)
            );
        }

        [TestFixtureTearDown]
        public void FixtureTearDown () {
            EvaluatorPool.Dispose();
        }

        protected ComparisonTest MakeTest (
            string filename, string[] stubbedAssemblies = null, 
            TypeInfoProvider typeInfo = null,
            AssemblyCache assemblyCache = null
        ) {
            return new ComparisonTest(
                EvaluatorPool, 
                filename, stubbedAssemblies, 
                typeInfo, assemblyCache
            );
        }

        protected virtual Configuration MakeConfiguration () {
            return ComparisonTest.MakeDefaultConfiguration();
        }

        protected TypeInfoProvider MakeDefaultProvider () {
            // Construct a type info provider with default proxies loaded (kind of a hack)
            return (new AssemblyTranslator(MakeConfiguration())).GetTypeInfoProvider();
        }

        /// <summary>
        /// Runs one or more comparison tests by compiling the source C# or VB.net file,
        ///     running the compiled test method, translating the compiled test method to JS,
        ///     then running the translated JS and comparing the outputs.
        /// </summary>
        /// <param name="filenames">The path to one or more test files. If a test file is named 'Common.cs' it will be linked into all tests.</param>
        /// <param name="stubbedAssemblies">The paths of assemblies to stub during translation, if any.</param>
        /// <param name="typeInfo">A TypeInfoProvider to use for type info. Using this parameter is not advised if you use proxies or JSIL.Meta attributes in your tests.</param>
        /// <param name="testPredicate">A predicate to invoke before running each test. If the predicate returns false, the JS version of the test will not be run (though it will be translated).</param>
        protected void RunComparisonTests (
            string[] filenames, string[] stubbedAssemblies = null, 
            TypeInfoProvider typeInfo = null, 
            Func<string, bool> testPredicate = null,
            Action<string, string> errorCheckPredicate = null
        ) {
            var started = DateTime.UtcNow.Ticks;

            string commonFile = null;
            for (var i = 0; i < filenames.Length; i++) {
                if (filenames[i].Contains("\\Common.")) {
                    commonFile = filenames[i];
                    break;
                }
            }

            const string keyName = @"Software\Squared\JSIL\Tests\PreviousFailures";

            StackFrame callingTest = null;
            for (int i = 1; i < 10; i++) {
                callingTest = new StackFrame(i);
                var method = callingTest.GetMethod();
                if ((method != null) && method.GetCustomAttributes(true).Any(
                    (ca) => ca.GetType().FullName == "NUnit.Framework.TestAttribute"
                )) {
                    break;
                } else {
                    callingTest = null;
                }
            }

            var previousFailures = new HashSet<string>();
            MethodBase callingMethod = null;
            if ((callingTest != null) && ((callingMethod = callingTest.GetMethod()) != null)) {
                try {
                    using (var rk = Registry.CurrentUser.CreateSubKey(keyName)) {
                        var names = rk.GetValue(callingMethod.Name) as string;
                        if (names != null) {
                            foreach (var name in names.Split(',')) {
                                previousFailures.Add(name);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine("Warning: Could not open registry key: {0}", ex);
                }
            }

            var failureList = new List<string>();
            var sortedFilenames = new List<string>(filenames);
            sortedFilenames.Sort(
                (lhs, rhs) => {
                    var lhsShort = Path.GetFileNameWithoutExtension(lhs);
                    var rhsShort = Path.GetFileNameWithoutExtension(rhs);

                    int result =
                        (previousFailures.Contains(lhsShort) ? 0 : 1).CompareTo(
                            previousFailures.Contains(rhsShort) ? 0 : 1
                        );

                    if (result == 0)
                        result = lhsShort.CompareTo(rhsShort);

                    return result;
                }
            );

            var asmCache = new AssemblyCache();

            foreach (var filename in sortedFilenames) {
                if (filename == commonFile)
                    continue;

                bool shouldRunJs = true;
                if (testPredicate != null)
                    shouldRunJs = testPredicate(filename);

                RunComparisonTest(
                    filename, stubbedAssemblies, typeInfo, 
                    errorCheckPredicate, failureList, 
                    commonFile, shouldRunJs, asmCache, 
                    MakeConfiguration
                );
            }

            if (callingMethod != null) {
                try {
                    using (var rk = Registry.CurrentUser.CreateSubKey(keyName))
                        rk.SetValue(callingMethod.Name, String.Join(",", failureList.ToArray()));
                } catch (Exception ex) {
                    Console.WriteLine("Warning: Could not open registry key: {0}", ex);
                }
            }

            var ended = DateTime.UtcNow.Ticks;
            var elapsedTotalSeconds = TimeSpan.FromTicks(ended - started).TotalSeconds;
            Console.WriteLine("// Ran {0} test(s) in {1:000.00}s.", sortedFilenames.Count, elapsedTotalSeconds);

            Assert.AreEqual(0, failureList.Count,
                String.Format("{0} test(s) failed:\r\n{1}", failureList.Count, String.Join("\r\n", failureList.ToArray()))
            );
        }

        private void RunComparisonTest(
            string filename, string[] stubbedAssemblies = null, TypeInfoProvider typeInfo = null, Action<string, string> errorCheckPredicate = null,
            List<string> failureList = null, string commonFile = null,  bool shouldRunJs = true, AssemblyCache asmCache = null,
            Func<Configuration> makeConfiguration = null
        ) {
            Console.WriteLine("// {0} ... ", Path.GetFileName(filename));

            try {
                var testFilenames = new List<string>() { filename };
                if (commonFile != null)
                    testFilenames.Add(commonFile);

                using (var test = new ComparisonTest(
                    EvaluatorPool,
                    testFilenames,
                    Path.Combine(
                        ComparisonTest.TestSourceFolder,
                        ComparisonTest.MapSourceFileToTestFile(filename)
                    ),
                    stubbedAssemblies, typeInfo, asmCache)
                    )
                {
                    if (shouldRunJs) {
                        test.Run();
                    } else {
                        string js;
                        long elapsed;
                        try
                        {
                            var csOutput = test.RunCSharp(new string[0], out elapsed);
                            test.GenerateJavascript(new string[0], out js, out elapsed, makeConfiguration);

                            Console.WriteLine("generated");

                            if (errorCheckPredicate != null)
                            {
                                errorCheckPredicate(csOutput, js);
                            }
                        }
                        catch (Exception _exc)
                        {
                            Console.WriteLine("error");
                            throw;
                        }
                    }
                }
            } catch (Exception ex) {
                if (ex.Message == "JS test failed")
                    Debug.WriteLine(ex.InnerException);
                else
                    Debug.WriteLine(ex);

                if (failureList != null) {
                    failureList.Add(Path.GetFileNameWithoutExtension(filename));
                } else
                    throw;
            }
        }

        protected string GetJavascript (string fileName, string expectedText = null) {
            long elapsed, temp;
            string generatedJs = null, output;

            using (var test = MakeTest(fileName)) {
                try {
                    output = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed, MakeConfiguration);
                } catch {
                    Console.Error.WriteLine("// Generated JS: \r\n{0}", generatedJs);
                    throw;
                }

                if (expectedText != null)
                    Assert.AreEqual(expectedText, output.Trim());
            }

            return generatedJs;
        }

        protected string GenericTest (
            string fileName, string csharpOutput, 
            string javascriptOutput, string[] stubbedAssemblies = null,
            TypeInfoProvider typeInfo = null
        ) {
            long elapsed, temp;
            string generatedJs = null;

            using (var test = new ComparisonTest(EvaluatorPool, fileName, stubbedAssemblies, typeInfo)) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);

                try {
                    var jsOutput = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed, MakeConfiguration);

                    Assert.AreEqual(csharpOutput, csOutput.Trim(), "Did not get expected output from C# test");
                    Assert.AreEqual(javascriptOutput, jsOutput.Trim(), "Did not get expected output from JavaScript test");
                } catch {
                    Console.Error.WriteLine("// Generated JS: \r\n{0}", generatedJs);
                    throw;
                }
            }

            return generatedJs;
        }

        protected string GenericIgnoreTest (string fileName, string workingOutput, string jsErrorSubstring, string[] stubbedAssemblies = null) {
            long elapsed, temp;
            string generatedJs = null, jsOutput = null;

            using (var test = new ComparisonTest(EvaluatorPool, fileName, stubbedAssemblies)) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                Assert.AreEqual(workingOutput, csOutput.Trim());

                try {
                    jsOutput = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed, MakeConfiguration);
                    Assert.Fail("Expected javascript to throw an exception containing the string \"" + jsErrorSubstring + "\".");
                } catch (JavaScriptEvaluatorException jse) {
                    bool foundMatch = false;

                    foreach (var exc in jse.Exceptions) {
                        if (exc.Message.Contains(jsErrorSubstring)) {
                            foundMatch = true;
                            break;
                        }
                    }

                    if (!foundMatch) {
                        Console.Error.WriteLine("// Was looking for a JS exception containing the string '{0}' but didn't find it.", jsErrorSubstring);
                        Console.Error.WriteLine("// Generated JS: \r\n{0}", generatedJs);
                        if (jsOutput != null)
                            Console.Error.WriteLine("// JS output: \r\n{0}", jsOutput);
                        throw;
                    }
                } catch {
                    Console.Error.WriteLine("// Generated JS: \r\n{0}", generatedJs);
                    if (jsOutput != null)
                        Console.Error.WriteLine("// JS output: \r\n{0}", jsOutput);
                    throw;
                }

            }

            return generatedJs;
        }

        protected void RunSingleComparisonTestCase (object[] parameters) {
            if (parameters.Length != 4)
                throw new ArgumentException("Wrong number of test case data parameters.");

            RunComparisonTest(
                (string)parameters[0], null, (TypeInfoProvider)parameters[1], null, null, (string)parameters[3], true, (AssemblyCache)parameters[2]
            );
        }

        protected IEnumerable<TestCaseData> FolderTestSource (string folderName, TypeInfoProvider typeInfo = null, AssemblyCache asmCache = null) {
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, folderName));
            var testNames = Directory.GetFiles(testPath, "*.cs").Concat(Directory.GetFiles(testPath, "*.vb")).OrderBy((s) => s);

            string commonFile = null;

            foreach (var testName in testNames) {
                if (Path.GetFileNameWithoutExtension(testName) == "Common") {
                    commonFile = testName;
                    break;
                }
            }

            foreach (var testName in testNames) {
                if (Path.GetFileNameWithoutExtension(testName) == "Common")
                    continue;

                yield return (new TestCaseData(new object[] { new object[] { testName, typeInfo, asmCache, commonFile } }))
                    .SetName(Path.GetFileName(testName))
                    .SetDescription(String.Format("{0}\\{1}", folderName, Path.GetFileName(testName)))
                    .SetCategory(folderName);
            }
        }

        protected IEnumerable<TestCaseData> FilenameTestSource (string[] filenames, TypeInfoProvider typeInfo = null, AssemblyCache asmCache = null) {
            foreach (var testName in filenames.OrderBy((s) => s)) {
                bool isIgnored = testName.StartsWith("ignored:", StringComparison.OrdinalIgnoreCase);
                var actualTestName = testName;

                if (isIgnored)
                    actualTestName = actualTestName.Substring(actualTestName.IndexOf(":") + 1);

                var item = (new TestCaseData(new object[] { new object[] { actualTestName, typeInfo, asmCache, null } }))
                    .SetName(Path.GetFileName(actualTestName));

                if (isIgnored)
                    item.Ignore();

                yield return item;
            }
        }
    }

    public static class TestExtensions {
        public static bool ContainsRegex (this string text, string regex) {
            var m = Regex.Matches(text, regex);
            return (m.Count > 0);
        }
    }
}
