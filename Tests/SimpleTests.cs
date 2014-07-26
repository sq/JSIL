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
    public class SimpleTests : GenericTestFixture {
        [Test]
        [TestCaseSource("SimpleTestCasesSource")]
        public void SimpleTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSource()
        {
            return FolderTestSource("SimpleTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("SimpleTestCasesSourceForStubbedBcl")]
        public void SimpleTestCasesForStubbedBcl (object[] parameters) {
            Func<Configuration> makeConfiguration = () => {
                var c = new Configuration {
                    ApplyDefaults = false,
                };
                c.Assemblies.Stubbed.Add("mscorlib,");
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

                c.Assemblies.Proxies.Add("JSIL.Proxies.Bcl.dll");
                return c;
            };

            Action<AssemblyTranslator> initializeTranslator = (at) => {
                // Suppress stdout spew
                at.IgnoredMethod += (_, _2) => { };
            };

            RunSingleComparisonTestCase(
                parameters,
                makeConfiguration,
                false,
                initializeTranslator: initializeTranslator
            );
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSourceForStubbedBcl () {
            return FolderTestSource("SimpleTestCasesForStubbedBcl", null, new AssemblyCache());
        }

        [Test]
        [TestCaseSource("SimpleTestCasesSourceForTranslatedBcl")]
        public void SimpleTestCasesForTranslatedBcl(object[] parameters)
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

                    c.Assemblies.Proxies.Add("JSIL.Proxies.Bcl.dll");
                    return c;
                };

            Action<AssemblyTranslator> initializeTranslator = (at) => {
                // Suppress stdout spew
                at.IgnoredMethod += (_, _2) => { };
            };

            RunSingleComparisonTestCase(
                parameters, 
                makeConfiguration,
                false,
                initializeTranslator: initializeTranslator
            );
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSourceForTranslatedBcl()
        {
            return FolderTestSource("SimpleTestCasesForTranslatedBcl", null, new AssemblyCache());
        }
    }
}
