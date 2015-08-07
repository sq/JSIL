using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using JSIL.Internal;
using JSIL.Tests;
using JSIL.Translator;
using NUnit.Framework;

namespace JSIL.SimpleTests {
    [TestFixture]
    public class ExpressionTests : GenericTestFixture {
        protected override Dictionary<string, string> SetupEvaluatorEnvironment()
        {
            return new Dictionary<string, string> { { "bclMode", "translated" } };
        }

        [Test]
        [TestCaseSource("ExpressionTestCasesSourceForTranslatedBcl")]
        public void ExpressionTestCasesForTranslatedBcl(object[] parameters)
        {
            Func<Configuration> makeConfiguration = () =>
            {
                var c = new Configuration
                {
                    ApplyDefaults = false,
                };
                c.Assemblies.Stubbed.Add("^System,");
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
                c.Assemblies.TranslateAdditional.Add("JSIL.ExpressionInterpreter.dll");
                
                c.Assemblies.Proxies.Add(Path.Combine(ComparisonTest.JSILFolder, "JSIL.Proxies.Bcl.dll"));

                return c;
            };

            Action<AssemblyTranslator> initializeTranslator = (at) =>
            {
                // Suppress stdout spew
                at.IgnoredMethod += (_, _2) => { };
            };

            RunSingleComparisonTestCase(
                parameters,
                makeConfiguration,
                new JSEvaluationConfig
                {
                    ThrowOnUnimplementedExternals = false,
                    AdditionalFilesToLoad = new[] { Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "..", "Libraries", "JSIL.ExpressionInterpreter.js")) },
                },
                initializeTranslator: initializeTranslator
            );
        }

        protected IEnumerable<TestCaseData> ExpressionTestCasesSourceForTranslatedBcl()
        {
            return FolderTestSource("ExpressionTestCases", null, new AssemblyCache());
        }
    }
}
