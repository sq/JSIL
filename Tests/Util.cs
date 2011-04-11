using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Threading;
using NUnit.Framework;

namespace JSIL.Tests {
    public class JavaScriptException : Exception {
        public readonly string ErrorText;

        public JavaScriptException (int exitCode, string errorText)
            : base(String.Format("JavaScript interpreter exited with code {0}\r\n{1}", exitCode, errorText)) 
        {
            ErrorText = errorText;
        }
    }

    public static class CSharpUtil {
        public static Assembly Compile (string sourceCode) {
            using (var csc = new CSharpCodeProvider(new Dictionary<string, string>() { 
                { "CompilerVersion", "v4.0" } 
            })) {
                var parameters = new CompilerParameters(new[] {
                    "mscorlib.dll", "System.Core.dll", "Microsoft.CSharp.dll",
                    typeof(JSIL.Meta.JSIgnore).Assembly.Location
                }) {
                    GenerateExecutable = true,
                    GenerateInMemory = false,
                    IncludeDebugInformation = true
                };

                var results = csc.CompileAssemblyFromSource(parameters, sourceCode);

                if (results.Errors.Count > 0) {
                    throw new Exception(
                        String.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select((ce) => ce.ErrorText).ToArray())
                    );
                }

                return results.CompiledAssembly;
            }
        }
    }

    public class ComparisonTest {
        public static readonly string TestSourceFolder;
        public static readonly string JSShellPath;
        public static readonly string BootstrapJS;

        public readonly string Filename;
        public readonly Assembly Assembly;
        public readonly MethodInfo TestMethod;

        static string GetPathOfAssembly (Assembly assembly) {
            return new Uri(assembly.CodeBase).AbsolutePath.Replace("/", "\\");
        }

        static ComparisonTest () {
            var testAssembly = typeof(ComparisonTest).Assembly;
            var assemblyPath = Path.GetDirectoryName(GetPathOfAssembly(testAssembly));

            TestSourceFolder = Path.GetFullPath(Path.Combine(assemblyPath, @"..\"));
            JSShellPath = Path.GetFullPath(Path.Combine(assemblyPath, @"..\..\Upstream\SpiderMonkey\js.exe"));

            using (var resourceStream = testAssembly.GetManifestResourceStream("JSIL.Tests.bootstrap.js"))
            using (var sr = new StreamReader(resourceStream))
                BootstrapJS = sr.ReadToEnd();
        }

        public ComparisonTest (string filename) {
            Filename = Path.Combine(TestSourceFolder, filename);

            var sourceCode = File.ReadAllText(Filename);
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

                    if (process.ExitCode != 0)
                        throw new JavaScriptException(process.ExitCode, (output[1] ?? "").Trim());
                }

                long endedJs = DateTime.UtcNow.Ticks;
                elapsed = endedJs - startedJs;

                return output[0] ?? "";
            } finally {
                var jsFile = Filename.Replace(".cs", ".js");
                if (File.Exists(jsFile))
                    File.Delete(jsFile);
                File.Copy(tempFilename, jsFile);

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
}
