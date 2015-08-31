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
            var exitCode = ProcessUtil.Run(Path.Combine(testDir, "vcbuild.bat"), "", null, out stderr, out stdout);

            Console.WriteLine(stdout);
            if (exitCode != 0) {
                Console.WriteLine(stderr);
                throw new Exception("Failed to build common.dll.");
            }

            DllPath = Path.Combine(testDir, "common.dll");

            exitCode = ProcessUtil.Run(Path.Combine(testDir, "embuild.bat"), "", null, out stderr, out stdout);

            Console.WriteLine(stdout);

            if (exitCode != 0) {
                Console.WriteLine(stderr);
                throw new Exception("Failed to build common.emjs.");
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
