using System;
using System.Collections.Generic;
using JSIL.Internal;
using JSIL.Translator;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class PerformanceTests : GenericTestFixture {
        Configuration MakeUnsafeConfiguration () {
            var cfg = MakeConfiguration();
            cfg.CodeGenerator.EnableUnsafeCode = true;
            return cfg;
        }

        protected override Dictionary<string, string> SetupEvaluatorEnvironment () {
            return new Dictionary<string, string> {
                { "INFERFLAGS", "result" }
            };
        }

        protected override string JSShellOptions {
            get {
                return "--ion-eager --thread-count=0";
            }
        }

        protected override bool UseDebugJSShell {
            get {
                return true;
            }
        }

        [Test]
        public void BinaryTrees () {
            using (var test = MakeTest(@"TestCases\BinaryTrees.cs")) {
                test.Run();
                test.Run(new[] { "8" });
            }
        }

        [Test]
        public void NBody () {
            using (var test = MakeTest(@"TestCases\NBody.cs")) {
                test.Run();
                test.Run(new[] { "100000" });
            }
        }

        [Test]
        public void FannkuchRedux () {
            using (var test = MakeTest(@"TestCases\FannkuchRedux.cs")) {
                test.Run();
                test.Run(new[] { "8" });
            }
        }

        [Test]
        public void UnsafeIntPerformanceComparison () {
            using (var test = MakeTest(
                @"PerformanceTestCases\UnsafeIntPerformanceComparison.cs"
            )) {
                Console.WriteLine("// setup code //");
                Console.WriteLine(ComparisonTest.EvaluatorSetupCode);

                string js;
                long elapsedJs, elapsedTranslation;

                var output = test.RunJavascript(
                    new string[0], out js, out elapsedTranslation, out elapsedJs,
                    makeConfiguration: MakeUnsafeConfiguration
                );

                Console.WriteLine("// startup prologue //");
                Console.WriteLine(test.StartupPrologue);
                Console.WriteLine("// output //");
                Console.WriteLine(output);
                Console.WriteLine("// generated js //");
                Console.WriteLine(js);
            }
        }

        private void RunPerformanceAnalysis (ComparisonTest test) {
            string inferData;
            string stderr;
            string tempS;
            long tempL;

            var output = test.RunJavascript(
                null, out tempS, out tempL, out tempL, out stderr, out inferData,
                makeConfiguration: MakeUnsafeConfiguration
            );

            Console.WriteLine("Object information:");
            Console.WriteLine(stderr);
            Console.WriteLine("TI data:");
            Console.WriteLine(inferData);
        }

        [Test]
        public void PointerMethodsAreSingletons () {
            using (var test = MakeTest(@"PerformanceTestCases\PointerMethodsAreSingletons.cs")) {
                RunPerformanceAnalysis(test);
            }
        }
    }
}
