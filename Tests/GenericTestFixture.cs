using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using JSIL.Internal;
using JSIL.Translator;
using Microsoft.Win32;
using NUnit.Framework;

namespace JSIL.Tests {
    public class GenericTestFixture : IDisposable {
        protected TypeInfoProvider DefaultTypeInfoProvider;

        public EvaluatorPool EvaluatorPool {
            get;
            private set;
        }

        protected virtual bool UseDebugJSShell {
            get {
                return false;
            }
        }

        protected virtual Dictionary<string, string> SetupEvaluatorEnvironment () {
            return null;
        }

        protected virtual string JSShellOptions {
            get {
                return "";
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp () {
            EvaluatorPool = new EvaluatorPool(
                UseDebugJSShell 
                    ? ComparisonTest.DebugJSShellPath 
                    : ComparisonTest.JSShellPath, 
                JSShellOptions,
                (e) =>
                    e.WriteInput(ComparisonTest.EvaluatorSetupCode),
                SetupEvaluatorEnvironment()
            );
        }

        [TestFixtureTearDown]
        public void FixtureTearDown () {
            Dispose();
        }

        public void Dispose () {
            if (EvaluatorPool != null) {
                EvaluatorPool.Dispose();
                EvaluatorPool = null;
            }

            if (DefaultTypeInfoProvider != null) {
                DefaultTypeInfoProvider.Dispose();
                DefaultTypeInfoProvider = null;
            }
        }

        protected ComparisonTest MakeTest (
            string filename, string[] stubbedAssemblies = null,
            TypeInfoProvider typeInfo = null,
            AssemblyCache assemblyCache = null
        ) {
            return new ComparisonTest(
                EvaluatorPool,
                Portability.NormalizeDirectorySeparators(filename), stubbedAssemblies,
                typeInfo, assemblyCache
            );
        }

        protected virtual Configuration MakeConfiguration () {
            return ComparisonTest.MakeDefaultConfiguration();
        }

        protected TypeInfoProvider MakeDefaultProvider () {
            if (DefaultTypeInfoProvider == null)
                // Construct a type info provider with default proxies loaded (kind of a hack)
                DefaultTypeInfoProvider = (new AssemblyTranslator(MakeConfiguration())).GetTypeInfoProvider();

            return DefaultTypeInfoProvider.Clone();
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
            Action<string, string> errorCheckPredicate = null,
            Func<Configuration> getConfiguration = null
        ) {
            var started = DateTime.UtcNow.Ticks;

            string commonFile = null;
            for (var i = 0; i < filenames.Length; i++) {
                if (filenames[i].Contains(Path.Combine ("", "Common."))) {
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
                    getConfiguration ?? MakeConfiguration
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

        private CompileResult RunComparisonTest (
            string filename, string[] stubbedAssemblies = null, 
            TypeInfoProvider typeInfo = null, Action<string, string> errorCheckPredicate = null,
            List<string> failureList = null, string commonFile = null, 
            bool shouldRunJs = true, AssemblyCache asmCache = null,
            Func<Configuration> makeConfiguration = null, 
            Action<Exception> onTranslationFailure = null,
            JSEvaluationConfig evaluationConfig = null,
            string compilerOptions = "",
            Action<AssemblyTranslator> initializeTranslator = null,
            Func<string> getTestRunnerQueryString = null,
            bool? scanForProxies = null
        ) {
            CompileResult result = null;
            Console.WriteLine("// {0} ... ", Path.GetFileName(filename));
            filename = Portability.NormalizeDirectorySeparators(filename);

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
                    stubbedAssemblies, typeInfo, asmCache,
                    compilerOptions: compilerOptions
                )) {
                    test.GetTestRunnerQueryString = getTestRunnerQueryString ?? test.GetTestRunnerQueryString;
                    result = test.CompileResult;

                    if (shouldRunJs) {
                        test.Run(
                            makeConfiguration: makeConfiguration, 
                            evaluationConfig: evaluationConfig, 
                            onTranslationFailure: onTranslationFailure,
                            initializeTranslator: initializeTranslator,
                            scanForProxies: scanForProxies
                        );
                    } else {
                        string js;
                        long elapsed;
                        try {
                            var csOutput = test.RunCSharp(new string[0], out elapsed);
                            test.GenerateJavascript(
                                new string[0], out js, out elapsed, 
                                makeConfiguration, 
                                evaluationConfig == null || evaluationConfig.ThrowOnUnimplementedExternals, 
                                onTranslationFailure,
                                initializeTranslator
                            );

                            Console.WriteLine("generated");

                            if (errorCheckPredicate != null) {
                                errorCheckPredicate(csOutput, js);
                            }
                        } catch (Exception) {
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

            return result;
        }

        protected string GetJavascript (string fileName, string expectedText = null, Func<Configuration> makeConfiguration = null, bool dumpJsOnFailure = true) {
            long elapsed, temp;
            string generatedJs = null, output;

            using (var test = MakeTest(fileName)) {
                try {
                    output = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed, makeConfiguration ?? MakeConfiguration);
                } catch {
                    if (dumpJsOnFailure)
                        Console.Error.WriteLine("// Generated JS: \r\n{0}", generatedJs);
                    throw;
                }

                if (expectedText != null)
                    Assert.AreEqual(Portability.NormalizeNewLines(expectedText), output.Trim());
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

            using (var test = new ComparisonTest(EvaluatorPool, Portability.NormalizeDirectorySeparators(fileName), stubbedAssemblies, typeInfo)) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);

                try {
                    var jsOutput = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed, MakeConfiguration);

                    try {
                        Assert.AreEqual(Portability.NormalizeNewLines(csharpOutput), csOutput.Trim(), "Did not get expected output from C# test");
                    } catch {
                        var cso = csOutput;
                        if (cso.Length > 8192)
                            cso = cso.Substring(0, 8192);
                        Console.Error.WriteLine("// C# stdout: \r\n{0}", cso);
                        throw;
                    }

                    Assert.AreEqual(Portability.NormalizeNewLines(javascriptOutput), jsOutput.Trim(), "Did not get expected output from JavaScript test");
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

            using (var test = new ComparisonTest(EvaluatorPool, Portability.NormalizeDirectorySeparators(fileName), stubbedAssemblies)) {
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

        protected CompileResult RunSingleComparisonTestCase (
            object[] parameters, 
            Func<Configuration> makeConfiguration = null,
            JSEvaluationConfig evaluationConfig = null,
            Action<Exception> onTranslationFailure = null,
            string compilerOptions = "",
            Action<AssemblyTranslator> initializeTranslator = null,
            Func<string> getTestRunnerQueryString = null,
            bool? scanForProxies = null
        ) {
            if (parameters.Length != 5)
                throw new ArgumentException("Wrong number of test case data parameters.");

            var provider = (TypeInfoProvider)parameters[1];
            var cache = (AssemblyCache)parameters[2];
            try {
                return RunComparisonTest(
                    (string)parameters[0], null, provider, null, null, (string)parameters[3], true, cache,
                    makeConfiguration: makeConfiguration,
                    evaluationConfig: evaluationConfig,
                    onTranslationFailure: onTranslationFailure,
                    compilerOptions: compilerOptions,
                    initializeTranslator: initializeTranslator,
                    getTestRunnerQueryString: getTestRunnerQueryString,
                    scanForProxies: scanForProxies
                );
            } finally {
                if ((bool)parameters[4]) {
                    Console.WriteLine("Disposing cache and provider.");
                    if (provider != null)
                        provider.Dispose();
                    if (cache != null)
                        cache.Dispose();
                }
            }
        }

        protected IEnumerable<TestCaseData> FolderTestSource (string folderName, TypeInfoProvider typeInfo = null, AssemblyCache asmCache = null) {
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, folderName));
            if (!Directory.Exists(testPath)) {
                Console.WriteLine("WARNING: Folder {0} doesn't exist.", testPath);
                yield break;
            }

            var testNames = Directory.GetFiles(testPath, "*.cs")
                .Concat(Directory.GetFiles(testPath, "*.vb"))
                .Concat(Directory.GetFiles(testPath, "*.fs"))
                .Concat(Directory.GetFiles(testPath, "*.js"))
                .Concat(Directory.GetFiles(testPath, "*.il"))
                .Concat(Directory.GetFiles(testPath, "*.cpp"))
                .OrderBy((s) => s).ToArray();

            string commonFile = null;

            foreach (var testName in testNames) {
                if (Path.GetFileNameWithoutExtension(testName) == "Common") {
                    commonFile = testName;
                    break;
                }
            }

            for (int i = 0, l = testNames.Length; i < l; i++) {
                var testName = testNames[i];
                if (Path.GetFileNameWithoutExtension(testName) == "Common")
                    continue;

                yield return (new TestCaseData(new object[] { new object[] { testName, typeInfo, asmCache, commonFile, i == (l - 1) } }))
                    .SetName(PickTestNameForFilename(testName))
                    .SetDescription(String.Format("{0}\\{1}", folderName, Path.GetFileName(testName)))
                    .SetCategory(folderName);
            }
        }

        protected IEnumerable<TestCaseData> FilenameTestSource (string[] filenames, TypeInfoProvider typeInfo = null, AssemblyCache asmCache = null) {
            var testNames = filenames.OrderBy((s) => s).ToArray();

            for (int i = 0, l = testNames.Length; i < l; i++) {
                var testName = testNames[i];

                bool isIgnored = testName.StartsWith("ignored:", StringComparison.OrdinalIgnoreCase);
                var actualTestName = testName;

                if (isIgnored)
                    actualTestName = actualTestName.Substring(actualTestName.IndexOf(":") + 1);

                var item = (new TestCaseData(new object[] { new object[] { actualTestName, typeInfo, asmCache, null, i == (l - 1) } }))
                    .SetName(PickTestNameForFilename(actualTestName));

                if (isIgnored)
                    item.Ignore();

                yield return item;
            }
        }

        public static string PickTestNameForFilename (string filename) {
            var result = Path.GetFileNameWithoutExtension(filename);

            switch (Path.GetExtension(filename).ToLowerInvariant()) {
                case ".cs":
                    return result + " (C#)";
                case ".vb":
                    return result + " (VB)";
                case ".fs":
                    return result + " (F#)";
                case ".js":
                    return result + " (JavaScript)";
                case ".il":
                    return result + " (CIL)";
                case ".cpp":
                    return result + " (C++/CLI)";
                default:
                    return result;
            }
        }
    }
}
