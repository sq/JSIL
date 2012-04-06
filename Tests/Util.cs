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
using MethodInfo = System.Reflection.MethodInfo;

namespace JSIL.Tests {
    public class JavaScriptException : Exception {
        public readonly int ExitCode;
        public readonly string ErrorText;
        public readonly string Output;

        public JavaScriptException (int exitCode, string stdout, string stderr)
            : base(String.Format("JavaScript interpreter exited with code {0}\r\n{1}\r\n{2}", exitCode, stdout, stderr)) 
        {
            ExitCode = exitCode;
            ErrorText = stderr;
            Output = stdout;
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

        public static Assembly CompileCS (string[] sourceCode, out TempFileCollection temporaryFiles) {
            using (var csc = new CSharpCodeProvider(new Dictionary<string, string>() { 
                { "CompilerVersion", "v4.0" } 
            })) {
                return Compile(csc, sourceCode, out temporaryFiles);
            }
        }

        public static Assembly CompileVB (string[] sourceCode, out TempFileCollection temporaryFiles) {
            using (var vbc = new VBCodeProvider(new Dictionary<string, string>() { 
                { "CompilerVersion", "v4.0" } 
            })) {
                return Compile(vbc, sourceCode, out temporaryFiles);
            }
        }

        private static Assembly Compile (CodeDomProvider provider, string[] sourceCode, out TempFileCollection temporaryFiles) {            
            var parameters = new CompilerParameters(new[] {
                "mscorlib.dll", "System.dll", "System.Core.dll", "Microsoft.CSharp.dll",
                typeof(JSIL.Meta.JSIgnore).Assembly.Location
            }) {
                CompilerOptions = "/unsafe",
                GenerateExecutable = true,
                GenerateInMemory = false,
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(TempPath, true)
            };

            var results = provider.CompileAssemblyFromSource(parameters, sourceCode);

            if (results.Errors.Count > 0) {
                throw new Exception(
                    String.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select((ce) => ce.ToString()).ToArray())
                );
            }

            temporaryFiles = results.TempFiles;
            return results.CompiledAssembly;
        }
    }

    public class ComparisonTest : IDisposable {
        public float JavascriptExecutionTimeout = 30.0f;

        public static readonly Regex ElapsedRegex = new Regex(
            @"// elapsed: (?'elapsed'[0-9]*(\.[0-9]*)?)", RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        protected TempFileCollection TemporaryFiles;

        public static readonly string TestSourceFolder;
        public static readonly string JSShellPath;
        public static readonly string CoreJSPath, BootstrapJSPath;

        public readonly TypeInfoProvider TypeInfo;
        public readonly string[] StubbedAssemblies;
        public readonly string OutputPath;
        public readonly Assembly Assembly;
        public readonly MethodInfo TestMethod;

        static ComparisonTest () {
            var testAssembly = typeof(ComparisonTest).Assembly;
            var assemblyPath = Path.GetDirectoryName(Util.GetPathOfAssembly(testAssembly));

            TestSourceFolder = Path.GetFullPath(Path.Combine(assemblyPath, @"..\"));
            JSShellPath = Path.GetFullPath(Path.Combine(assemblyPath, @"..\..\Upstream\SpiderMonkey\js.exe"));
            CoreJSPath = Path.GetFullPath(Path.Combine(TestSourceFolder, @"..\Libraries\JSIL.Core.js"));
            BootstrapJSPath = Path.GetFullPath(Path.Combine(TestSourceFolder, @"..\Libraries\JSIL.Bootstrap.js"));
        }

        public ComparisonTest (string filename, string[] stubbedAssemblies = null, TypeInfoProvider typeInfo = null)
            : this (
                new[] { filename }, 
                Path.Combine(
                    TestSourceFolder, 
                    filename.Replace(".cs", ".js").Replace(".vb", "_vb.js")
                ), 
                stubbedAssemblies, typeInfo
            ) {
        }

        public ComparisonTest (IEnumerable<string> filenames, string outputPath, string[] stubbedAssemblies = null, TypeInfoProvider typeInfo = null) {
            OutputPath = outputPath;

            var sourceCode = (from f in filenames
                              let fullPath = Path.Combine(TestSourceFolder, f)
                              select File.ReadAllText(fullPath)).ToArray();
            var extensions = (from f in filenames select Path.GetExtension(f).ToLower()).Distinct().ToArray();

            if (extensions.Length != 1)
                throw new InvalidOperationException("Mixture of different source languages provided.");

            switch (extensions[0]) {
                case ".cs":
                    Assembly = CompilerUtil.CompileCS(sourceCode, out TemporaryFiles);
                    break;
                case ".vb":
                    Assembly = CompilerUtil.CompileVB(sourceCode, out TemporaryFiles);
                    break;
                default:
                    throw new ArgumentException("Unsupported source file type for test");
            }

            var program = Assembly.GetType("Program");
            if (program == null)
                throw new Exception("Test missing 'Program' main class");

            TestMethod = program.GetMethod("Main");
            if (TestMethod == null)
                throw new Exception("Test missing 'Main' method of 'Program' main class");

            StubbedAssemblies = stubbedAssemblies;
            TypeInfo = typeInfo;
        }

        public void Dispose () {
            foreach (string filename in TemporaryFiles)
                try {
                    File.Delete(filename);
                } catch {
                }
        }

        public string RunCSharp (string[] args, out long elapsed) {
            var oldStdout = Console.Out;
            using (var sw = new StringWriter())
                try {
                    Console.SetOut(sw);
                    long startedCs = DateTime.UtcNow.Ticks;
                    TestMethod.Invoke(null, new object[] { args });
                    long endedCs = DateTime.UtcNow.Ticks;
                    elapsed = endedCs - startedCs;
                    return sw.ToString();
                } finally {
                    Console.SetOut(oldStdout);
                }
        }

        public static Configuration MakeDefaultConfiguration () {
            return new Configuration {
                FrameworkVersion = 4.0,
                IncludeDependencies = false,
                ApplyDefaults = false
            };
        }

        public string RunJavascript (string[] args, out string generatedJavascript, out long elapsedTranslation, out long elapsedJs) {
            var tempFilename = Path.GetTempFileName();
            var configuration = MakeDefaultConfiguration();

            if (StubbedAssemblies != null)
                configuration.Assemblies.Stubbed.AddRange(StubbedAssemblies);

            var translator = new JSIL.AssemblyTranslator(configuration, TypeInfo);

            string translatedJs;
            var translationStarted = DateTime.UtcNow.Ticks;
            var assemblyPath = Util.GetPathOfAssembly(Assembly);
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

            elapsedTranslation = DateTime.UtcNow.Ticks - translationStarted;

            var declaringType = JSIL.Internal.Util.EscapeIdentifier(TestMethod.DeclaringType.FullName, Internal.EscapingMode.TypeIdentifier);

            string argsJson;
            var jsonSerializer = new DataContractJsonSerializer(typeof(string[]));
            using (var ms2 = new MemoryStream()) {
                jsonSerializer.WriteObject(ms2, args);
                argsJson = Encoding.UTF8.GetString(ms2.GetBuffer(), 0, (int)ms2.Length);
            }

            var invocationJs = String.Format(
                @"timeout({0}); JSIL.Initialize(); var started = elapsed(); {1}.Main({2}); var ended = elapsed(); print('// elapsed: ' + (ended - started));", 
                JavascriptExecutionTimeout, declaringType, argsJson
            );

            generatedJavascript = translatedJs;

            File.WriteAllText(tempFilename, translatedJs + Environment.NewLine + invocationJs);

            try {
                // throw new Exception();

                var psi = new ProcessStartInfo(JSShellPath, String.Format("-j -m -n -f \"{0}\" -f \"{1}\" -f \"{2}\"", CoreJSPath, BootstrapJSPath, tempFilename)) {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                ManualResetEventSlim stdoutSignal, stderrSignal;
                stdoutSignal = new ManualResetEventSlim(false);
                stderrSignal = new ManualResetEventSlim(false);
                var output = new string[2];

                long startedJs = DateTime.UtcNow.Ticks;
                using (var process = Process.Start(psi)) {
                    ThreadPool.QueueUserWorkItem((_) => {
                        output[0] = process.StandardOutput.ReadToEnd();
                        stdoutSignal.Set();
                    });
                    ThreadPool.QueueUserWorkItem((_) => {
                        output[1] = process.StandardError.ReadToEnd();
                        stderrSignal.Set();
                    });

                    stdoutSignal.Wait();
                    stderrSignal.Wait();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        throw new JavaScriptException(
                            process.ExitCode,
                            (output[0] ?? "").Trim(),
                            (output[1] ?? "").Trim()
                        );
                }

                long endedJs = DateTime.UtcNow.Ticks;
                elapsedJs = endedJs - startedJs;

                if (output[0] != null) {
                    var m = ElapsedRegex.Match(output[0]);
                    if (m.Success) {
                        elapsedJs = TimeSpan.FromMilliseconds(
                            double.Parse(m.Groups["elapsed"].Value)
                        ).Ticks;
                        output[0] = output[0].Replace(m.Value, "");
                    }
                }

                return output[0] ?? "";
            } finally {
                translator.Dispose();

                var jsFile = OutputPath;
                if (File.Exists(jsFile))
                    File.Delete(jsFile);
                File.Copy(tempFilename, jsFile);

                File.Delete(tempFilename);
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
                    "passed: C#:{0:00.00}s JSIL:{1:00.00}s JS:{2:00.00}s",
                    TimeSpan.FromTicks(elapsed[0]).TotalSeconds,
                    TimeSpan.FromTicks(elapsed[1]).TotalSeconds,
                    TimeSpan.FromTicks(elapsed[2]).TotalSeconds
                );
            } catch {
                Console.WriteLine("failed");
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
        protected TypeInfoProvider MakeDefaultProvider () {
            // Construct a type info provider with default proxies loaded (kind of a hack)
            return (new AssemblyTranslator(ComparisonTest.MakeDefaultConfiguration())).GetTypeInfoProvider();
        }

        protected void RunComparisonTests (
            string[] filenames, string[] stubbedAssemblies = null, TypeInfoProvider typeInfo = null
        ) {
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

            foreach (var filename in sortedFilenames) {
                if (filename == commonFile)
                    continue;

                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    var testFilenames = new List<string>() { filename };
                    if (commonFile != null)
                        testFilenames.Add(commonFile);

                    using (var test = new ComparisonTest(
                        testFilenames, 
                        Path.Combine(
                            ComparisonTest.TestSourceFolder,
                            filename.Replace(".cs", ".js").Replace(".vb", "_vb.js")
                        ),
                        stubbedAssemblies, typeInfo)
                    )
                        test.Run();
                } catch (Exception ex) {
                    failureList.Add(Path.GetFileNameWithoutExtension(filename));
                    if (ex.Message == "JS test failed")
                        Debug.WriteLine(ex.InnerException);
                    else
                        Debug.WriteLine(ex);
                }
            }

            if (callingMethod != null) {
                try {
                    using (var rk = Registry.CurrentUser.CreateSubKey(keyName))
                        rk.SetValue(callingMethod.Name, String.Join(",", failureList.ToArray()));
                } catch (Exception ex) {
                    Console.WriteLine("Warning: Could not open registry key: {0}", ex);
                }
            }

            Assert.AreEqual(0, failureList.Count,
                String.Format("{0} test(s) failed:\r\n{1}", failureList.Count, String.Join("\r\n", failureList.ToArray()))
            );
        }

        protected string GetJavascript (string fileName, string expectedText = null) {
            long elapsed, temp;
            string generatedJs = null, output;

            using (var test = new ComparisonTest(fileName)) {
                try {
                    output = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed);
                } catch {
                    Console.Error.WriteLine("// Generated JS: \r\n{0}", generatedJs);
                    throw;
                }

                if (expectedText != null)
                    Assert.AreEqual(expectedText, output.Trim());
            }

            return generatedJs;
        }

        protected string GenericTest (string fileName, string csharpOutput, string javascriptOutput, string[] stubbedAssemblies = null) {
            long elapsed, temp;
            string generatedJs = null;

            using (var test = new ComparisonTest(fileName, stubbedAssemblies)) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);

                try {
                    var jsOutput = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed);

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

            using (var test = new ComparisonTest(fileName, stubbedAssemblies)) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                Assert.AreEqual(workingOutput, csOutput.Trim());

                try {
                    jsOutput = test.RunJavascript(new string[0], out generatedJs, out temp, out elapsed);
                    Assert.Fail("Expected javascript to throw an exception containing the string \"" + jsErrorSubstring + "\".");
                } catch (JavaScriptException jse) {
                    if (!jse.ErrorText.Contains(jsErrorSubstring)) {
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
    }

    public static class TestExtensions {
        public static bool ContainsRegex (this string text, string regex) {
            var m = Regex.Matches(text, regex);
            return (m.Count > 0);
        }
    }
}
