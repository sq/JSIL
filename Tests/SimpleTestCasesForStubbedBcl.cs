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
    public class SimpleTestCasesForStubbedBcl : GenericTestFixture
    {
        public static readonly string BootsrapperFileName =
            Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "Bootstappers", "BclBootstrapStubbed.cs"));

        private static readonly AssemblyCache AssemblyCache = new AssemblyCache();
        private readonly TypeInfoProvider TypeInfoProvider;
        public SimpleTestCasesForStubbedBcl()
        {
            TypeInfoProvider = MakeDefaultProvider();
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
                c.Assemblies.Stubbed.Add("mscorlib,");
                c.Assemblies.Stubbed.Add("^System,");
                c.Assemblies.Stubbed.Add("^System\\.(.+),");
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

        protected override Dictionary<string, string> SetupEvaluatorEnvironment()
        {
            return new Dictionary<string, string> {{"bclMode", "stubbed"}};
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


        protected IEnumerable<TestCaseData> SimpleTestCasesSourceForStubbedBcl()
        {
            return FolderTestSource("SimpleTestCasesForStubbedBcl", TypeInfoProvider, AssemblyCache);
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