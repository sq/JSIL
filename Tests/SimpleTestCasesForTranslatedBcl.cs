namespace JSIL.SimpleTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using JSIL.Internal;
    using JSIL.Tests;
    using JSIL.Translator;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleTestCasesForTranslatedBcl : GenericTestFixture
    {
        public static readonly string BootsrapperFileName =
            Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "Bootstappers", "BclBootstrapTranslated.cs"));

        private static readonly AssemblyCache AssemblyCache = new AssemblyCache();
        private readonly TypeInfoProvider TypeInfoProvider;

        public SimpleTestCasesForTranslatedBcl()
        {
            TypeInfoProvider = MakeDefaultProvider();
        }

        protected override Dictionary<string, string> SetupEvaluatorEnvironment()
        {
            return new Dictionary<string, string> { { "bclMode", "translated" } };
        }

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            Func<Configuration> makeConfiguration = () =>
            {
                var c = new Configuration
                {
                    ApplyDefaults = false,
                    FrameworkVersion = 4.0
                };
                c.CodeGenerator.EnableUnsafeCode = true;
                c.Assemblies.Stubbed.Add("^System\\.(?!Core)(.+),");
                c.Assemblies.Stubbed.Add("^Microsoft\\.(.+),");
                c.Assemblies.Stubbed.Add("FSharp.Core,");

                c.Assemblies.Ignored.Add("Microsoft\\.VisualC,");
                c.Assemblies.Ignored.Add("Accessibility,");
                c.Assemblies.Ignored.Add("SMDiagnostics,");
                c.Assemblies.Ignored.Add("System\\.EnterpriseServices,");
                c.Assemblies.Ignored.Add("System\\.Security,");
                c.Assemblies.Ignored.Add("System\\.Runtime\\.Serialization\\.Formatters\\.Soap,");
                c.Assemblies.Ignored.Add("System\\.Runtime\\.DurableInstancing,");
                c.Assemblies.Ignored.Add("System\\.Data\\.SqlXml,");
                c.Assemblies.Ignored.Add("JSIL\\.Meta,");

                c.Assemblies.Proxies.Add(Path.Combine(ComparisonTest.JSILFolder, "JSIL.Proxies.Bcl.dll"));
                return c;
            };

            Action<AssemblyTranslator> initializeTranslator = (at) =>
            {
                // Suppress stdout spew
                at.IgnoredMethod += (_, _2) => { };
            };

            RunSingleComparisonTestCase(
                BootstrapArguments(),
                makeConfiguration,
                new JSEvaluationConfig { ThrowOnUnimplementedExternals = false },
                initializeTranslator: initializeTranslator,
                shouldRunJs: false
                );
        }

        [Test]
        [TestCaseSource("SimpleTestCasesSourceForTranslatedBcl")]
        public void TestCasesTranslatedBcl(object[] parameters)
        {
            RunTestCase(parameters, null, null);
        }

        [Test]
        [TestCaseSource("SimpleTestCasesSourceForStubbedBcl")]
        public void TestCasesForStubbedBcl(object[] parameters)
        {
            RunTestCase(parameters, null, null);
        }

        [Test]
        [TestCaseSource("SimpleTestCasesSource")]
        public void SimpleTestCases(object[] parameters)
        {
            RunTestCase(parameters, null, null);
        }

        [Test]
        [TestCaseSource("ExpressionTestCasesSourceForTranslatedBcl")]
        public void ExpressionTestCasesForTranslatedBcl(object[] parameters)
        {
            Func<Configuration> makeConfiguration = () =>
            {
                var c = ComparisonTest.MakeDefaultConfiguration();
                c.Assemblies.TranslateAdditional.Add(Path.Combine(ComparisonTest.JSILFolder, "JSIL.ExpressionInterpreter.dll"));
                return c;
            };

            RunTestCase(parameters, makeConfiguration,
                new[]
                {
                    Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "..", "Libraries",
                        "JSIL.ExpressionInterpreter.js"))
                });
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSourceForTranslatedBcl()
        {
            return FolderTestSource("SimpleTestCasesForTranslatedBcl", TypeInfoProvider, AssemblyCache);
        }

        protected IEnumerable<TestCaseData> ExpressionTestCasesSourceForTranslatedBcl()
        {
            // TODO: Why we cannot reuse TypeInfoProvider here?
            return FolderTestSource("ExpressionTestCases", null, AssemblyCache);
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSourceForStubbedBcl()
        {
            return FolderTestSource("SimpleTestCasesForStubbedBcl", TypeInfoProvider, new AssemblyCache());
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSource()
        {
            return FolderTestSource("SimpleTestCases", TypeInfoProvider, AssemblyCache)
                .Concat(FolderTestSource("SimpleTestCasesFailingOnMono", TypeInfoProvider, AssemblyCache));
        }

        protected object[] BootstrapArguments()
        {
            var testCaseData =
                FilenameTestSource(new[] { BootsrapperFileName }, null, AssemblyCache).First();
            return (object[])testCaseData.Arguments[0];
        }

        private void RunTestCase(object[] parameters, Func<Configuration> makeConfiguration, IEnumerable<string> additionalFilesToLoad)
        {
            RunSingleComparisonTestCase(
                parameters,
                makeConfiguration,
                new JSEvaluationConfig
                {
                    ThrowOnUnimplementedExternals = false,
                    AdditionalFilesToLoad =
                        Enumerable.Repeat(BootsrapperFileName + ".out", 1)
                            .Concat(additionalFilesToLoad ?? Enumerable.Empty<string>())
                            .ToArray()
                }
                );
        }
    }
}