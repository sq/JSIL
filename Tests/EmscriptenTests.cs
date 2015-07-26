using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JSIL.Internal;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class EmscriptenTests : GenericTestFixture {
        public string DllPath;

        [TestFixtureSetUp]
        public void SetUp () {
            var testDir = Path.Combine(ComparisonTest.TestSourceFolder, "EmscriptenTestCases");

            string stdout, stderr;
            var exitCode = RunProcess(Path.Combine(testDir, "vcbuild.bat"), "", null, out stderr, out stdout);

            Console.WriteLine(stdout);
            if (exitCode != 0) {
                Console.WriteLine(stderr);
                throw new Exception("Failed to build common.dll.");
            }

            DllPath = Path.Combine(testDir, "common.dll");

            exitCode = RunProcess(Path.Combine(testDir, "embuild.bat"), "", null, out stderr, out stdout);

            Console.WriteLine(stdout);

            if (exitCode != 0) {
                Console.WriteLine(stderr);
                throw new Exception("Failed to build common.emjs.");
            }
        }

        private static byte[] ReadEntireStream (Stream stream) {
            var result = new List<byte>();
            var buffer = new byte[32767];

            while (true) {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead < buffer.Length) {

                    if (bytesRead > 0) {
                        result.Capacity = result.Count + bytesRead;
                        result.AddRange(buffer.Take(bytesRead));
                    }

                    if (bytesRead <= 0)
                        break;
                } else {
                    result.AddRange(buffer);
                }
            }

            return result.ToArray();
        }
        
        private static int RunProcess (string filename, string parameters, byte[] stdin, out string stderr, out string stdout) {
            var psi = new ProcessStartInfo(filename, parameters);

            psi.WorkingDirectory = Path.GetDirectoryName(filename);
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            using (var process = Process.Start(psi)) {
                var stdinStream = process.StandardInput.BaseStream;
                var stderrStream = process.StandardError.BaseStream;
                var stdoutStream = process.StandardOutput.BaseStream;

                if (stdin != null) {
                    ThreadPool.QueueUserWorkItem(
                        (_) => {
                            if (stdin != null) {
                                stdinStream.Write(
                                    stdin, 0, stdin.Length
                                );
                                stdinStream.Flush();
                            }

                            stdinStream.Close();
                        }, null
                    );
                }

                var temp = new string[2] { null, null };
                ThreadPool.QueueUserWorkItem(
                    (_) => {
                        temp[1] = Encoding.ASCII.GetString(ReadEntireStream(stderrStream));
                    }, null
                );

                temp[0] = Encoding.ASCII.GetString(ReadEntireStream(stdoutStream));

                process.WaitForExit();

                stdout = temp[0];
                stderr = temp[1];

                var exitCode = process.ExitCode;

                process.Close();

                return exitCode;
            }
        }

        protected override Translator.Configuration MakeConfiguration () {
            var result = base.MakeConfiguration();

            result.CodeGenerator.EnableUnsafeCode = true;

            return result;
        }

        [Test]
        [TestCaseSource("EmscriptenTestCasesSource")]
        public void EmscriptenTestCases (object[] parameters) {
            RunSingleComparisonTestCase(
                parameters,
                MakeConfiguration,
                compilerOptions: "/unsafe",
                getTestRunnerQueryString: (() =>
                    "dll=Tests/EmscriptenTestCases/common.emjs"),
                scanForProxies: true,
                extraDependencies: new [] { DllPath }
            );
        }

        protected IEnumerable<TestCaseData> EmscriptenTestCasesSource () {
            return FolderTestSource("EmscriptenTestCases", MakeDefaultProvider(), new AssemblyCache());
        }
    }
}
