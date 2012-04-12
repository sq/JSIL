using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class XMLTests : GenericTestFixture {
        [Test]
        public void AllXMLTests () {
            Console.WriteLine("// Spidermonkey has no DOMParser so this test only generates the .js files.");
            Console.WriteLine("// To run the .js files, click a link below.");
            Console.WriteLine();

            var typeInfo = MakeDefaultProvider();
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "XMLTestCases"));
            var xmlTests = Directory.GetFiles(testPath, "*.cs").Concat(Directory.GetFiles(testPath, "*.vb")).ToArray();

            var rootPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(ComparisonTest.CoreJSPath),
                @"..\"
            ));

            try {
                RunComparisonTests(
                    xmlTests, null, typeInfo,
                    (testFile) => {
                        var uri = new Uri(
                            Path.Combine(rootPath, "test_runner.html"), UriKind.Absolute
                        );

                        Console.WriteLine(
                            "{0}#{1}", uri,
                            Path.GetFullPath(testFile)
                                .Replace(".cs", ".js")
                                .Replace(".vb", ".vb.js")
                                .Replace(rootPath, "")
                                .Replace("\\", "/")
                        );

                        return false;
                    }
                );
            } catch (Exception exc) {
                Console.WriteLine(exc.Message);
            }
        }
    }
}
