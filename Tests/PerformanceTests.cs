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
            cfg.CodeGenerator.AggressivelyUseElementProxies = true;
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
        [FailsOnMono]
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
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                Console.WriteLine(test.RunJavascript(null, MakeUnsafeConfiguration));
            }
        }

        [Test]
        public void EnumCasts () {
            using (var test = MakeTest(
                @"PerformanceTestCases\EnumCasts.cs"
            )) {
                test.Run();
            }
        }

        [Test]
        public void Sieve () {
            using (var test = MakeTest(
                @"PerformanceTestCases\Sieve.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null, makeConfiguration: MakeUnsafeConfiguration));
            }
        }

        [Test]
        public void Vector3 () {
            using (var test = MakeTest(
                @"PerformanceTestCases\Vector3.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null, makeConfiguration: MakeUnsafeConfiguration));
            }
        }

        [Test]
        public void FuseePackedVertices () {
            using (var test = MakeTest(
                @"PerformanceTestCases\FuseePackedVertices.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null, makeConfiguration: MakeUnsafeConfiguration));
            }
        }

        [Test]
        public void OverloadedMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\OverloadedMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void OverloadedConstructors () {
            using (var test = MakeTest(
                @"PerformanceTestCases\OverloadedConstructorsPerfomance.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void UncachedOverloadedMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\UncachedOverloadedMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void InterfaceMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\InterfaceMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void GenericInterfaceMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\GenericInterfaceMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void VariantGenericInterfaceMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\VariantGenericInterfaceMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void RectangleIntersects () {
            using (var test = MakeTest(
                @"PerformanceTestCases\RectangleIntersects.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void PropertyVsField () {
            using (var test = MakeTest(
                @"PerformanceTestCases\PropertyVsField.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(
                    null,
                    makeConfiguration: () => {
                        var cfg = MakeConfiguration();
                        cfg.CodeGenerator.PreferAccessorMethods = true;
                        return cfg;
                    }
                ));
            }
        }

        [Test]
        public void BaseMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\BaseMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }

        [Test]
        public void GenericMethodCalls () {
            using (var test = MakeTest(
                @"PerformanceTestCases\GenericMethodCalls.cs"
            )) {
                Console.WriteLine("// {0}", ComparisonTest.GetTestRunnerLink(test.OutputPath));

                long elapsedcs;

                Console.WriteLine("C#:\r\n{0}", test.RunCSharp(null, out elapsedcs));
                Console.WriteLine("JS:\r\n{0}", test.RunJavascript(null));
            }
        }
    } 
}
