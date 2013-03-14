using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
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
                Console.WriteLine(test.RunJavascript(null, MakeUnsafeConfiguration));
            }
        }
    }

    [TestFixture]
    public class PerformanceAnalysisTests : GenericTestFixture {
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
                return "--ion-eager";
            }
        }

        protected override bool UseDebugJSShell {
            get {
                return true;
            }
        }

        private void AssertIsSingleton (PerformanceAnalysisData data, string expression) {
            Assert.IsTrue(data[expression].IsSingleton, expression + " is not a singleton");
        }

        [Test]
        public void PointerMethodsAreSingletons () {
            using (var test = MakeTest(@"PerformanceTestCases\PointerMethodsAreSingletons.cs")) {
                var data = new PerformanceAnalysisData(test, MakeUnsafeConfiguration);

                Console.WriteLine(data.Output);

                try {
                    AssertIsSingleton(data, "pBuffer.getElement");
                    AssertIsSingleton(data, "pBuffer.setElement");
                    // FIXME: Fails. Something about this function makes SpiderMonkey unhappy :-(
                    // AssertIsSingleton(data, "Program.TestInlineAccess");
                } catch (Exception exc) {
                    data.Dump(Console.Out, true);
                    throw;
                }
            }
        }
    }    
}
