using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace JSIL.Tests {
    public class ComparisonTest {
        public static readonly string TestSourceFolder;
        public static readonly string JSShellPath;
        public static readonly string BootstrapJS;

        public readonly Assembly Assembly;
        public readonly MethodInfo TestMethod;

        static string GetPathOfAssembly (Assembly assembly) {
            return new Uri(assembly.CodeBase).AbsolutePath.Replace("/", "\\");
        }

        static ComparisonTest () {
            var testAssembly = typeof(ComparisonTest).Assembly;
            var assemblyPath = Path.GetDirectoryName(GetPathOfAssembly(testAssembly));

            TestSourceFolder = Path.GetFullPath(Path.Combine(assemblyPath, @"..\TestCases"));
            JSShellPath = Path.GetFullPath(Path.Combine(assemblyPath, @"..\..\Upstream\SpiderMonkey\js.exe"));

            using (var resourceStream = testAssembly.GetManifestResourceStream("JSIL.Tests.bootstrap.js"))
            using (var sr = new StreamReader(resourceStream))
                BootstrapJS = sr.ReadToEnd();
        }

        public ComparisonTest (string filename) {
            var sourceCode = File.ReadAllText(Path.Combine(TestSourceFolder, filename));
            Assembly = CSharpUtil.Compile(sourceCode);

            TestMethod = Assembly.GetType("Program").GetMethod("Main");
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

        public string RunJavascript (string[] args, out string generatedJavascript, out long elapsed) {
            var tempFilename = Path.GetTempFileName();
            var translator = new JSIL.AssemblyTranslator();
            var translatedJs = translator.Translate(GetPathOfAssembly(Assembly));
            var declaringType = JSIL.Internal.Util.EscapeIdentifier(TestMethod.DeclaringType.FullName, false);

            string argsJson;
            var jsonSerializer = new DataContractJsonSerializer(typeof(string[]));
            using (var ms = new MemoryStream()) {
                jsonSerializer.WriteObject(ms, args);
                argsJson = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }

            var invocationJs = String.Format("{0}.Main({1});", declaringType, argsJson);

            generatedJavascript = translatedJs;
            translatedJs = BootstrapJS + Environment.NewLine + translatedJs + Environment.NewLine + invocationJs;

            File.WriteAllText(tempFilename, translatedJs);

            try {
                var psi = new ProcessStartInfo(JSShellPath, String.Format("-f {0}", tempFilename)) {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var output = new string[2];

                long startedJs = DateTime.UtcNow.Ticks;
                using (var process = Process.Start(psi)) {
                    ThreadPool.QueueUserWorkItem((_) => {
                        output[0] = process.StandardOutput.ReadToEnd();
                    });
                    ThreadPool.QueueUserWorkItem((_) => {
                        output[1] = process.StandardError.ReadToEnd();
                    });

                    process.WaitForExit();

                    if ((output[1] != null) && (output[1].Trim().Length > 0))
                        throw new Exception("Javascript shell produced error output:\r\n" + output[1]);
                    else if (process.ExitCode != 0)
                        throw new Exception("Javascript shell exited with error code " + process.ExitCode.ToString());
                }

                long endedJs = DateTime.UtcNow.Ticks;
                elapsed = endedJs - startedJs;

                return output[0] ?? "";
            } finally {
                File.Delete(tempFilename);
            }
        }

        public void Run (params string[] args) {
            bool failedInner = false;
            string generatedJs = null;
            long elapsedCs, elapsedJs;

            var csOutput = RunCSharp(args, out elapsedCs);
            try {
                var jsOutput = RunJavascript(args, out generatedJs, out elapsedJs);

                try {
                    Assert.AreEqual(csOutput, jsOutput);
                } catch {
                    failedInner = true;
                    Console.WriteLine("failed");
                    Console.WriteLine("// C# output begins //");
                    Console.WriteLine(csOutput);
                    Console.WriteLine("// JavaScript output begins //");
                    Console.WriteLine(jsOutput);
                    throw;
                }
            } catch {
                if (!failedInner)
                    Console.WriteLine("failed");

                if (generatedJs != null) {
                    Console.WriteLine("// Generated javascript begins here //");
                    Console.WriteLine(generatedJs);
                    Console.WriteLine("// Generated javascript ends here //");
                }
                throw;
            }

            Console.WriteLine(
                "passed: C# in {0:00.0000}s, JS in {1:00.0000}s", 
                TimeSpan.FromTicks(elapsedCs).TotalSeconds,
                TimeSpan.FromTicks(elapsedJs).TotalSeconds
            );
        }
    }

    [TestFixture]
    public class ComparisonTests {
        [Test]
        public void HelloWorld () {
            var test = new ComparisonTest("HelloWorld.cs");

            test.Run();
            test.Run("hello", "world");
        }

        [Test]
        public void BinaryTrees () {
            var test = new ComparisonTest("BinaryTrees.cs");

            test.Run();
            test.Run("6");
        }

        [Test]
        public void NBody () {
            var test = new ComparisonTest("NBody.cs");

            test.Run();
        }

        // Fails because we emit a goto.
        [Test]
        public void FannkuchRedux () {
            var test = new ComparisonTest("FannkuchRedux.cs");

            test.Run();
        }

        [Test]
        public void AllSimpleTests () {
            var simpleTests = Directory.GetFiles(
                Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, @"..\SimpleTestCases")), 
                "*.cs"
            );
            int failureCount = 0;

            foreach (var filename in simpleTests) {
                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    var test = new ComparisonTest(filename);
                    test.Run();
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                    failureCount += 1;
                }
            }

            Assert.AreEqual(0, failureCount, "One or more tests failed");
        }
    }
}
