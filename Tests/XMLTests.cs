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
        public void SimpleXMLTests () {
            var typeInfo = MakeDefaultProvider();
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "XMLTestCases"));
            var testFiles = Directory.GetFiles(testPath, "*.cs").Concat(Directory.GetFiles(testPath, "*.vb")).ToArray();

            RunComparisonTests(
                testFiles, null, typeInfo
            );
        }

        [Test]
        public void BrowserXMLTests () {
            Console.WriteLine("// js.exe has no DOMParser so this test only generates the .js files.");
            Console.WriteLine("// To run the .js files, click a link below.");
            Console.WriteLine();

            var typeInfo = MakeDefaultProvider();
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "BrowserXMLTestCases"));
            var xmlTests = Directory.GetFiles(testPath, "*.cs").Concat(Directory.GetFiles(testPath, "*.vb")).ToArray();

            try {
                RunComparisonTests(
                    xmlTests, null, typeInfo,
                    (testFile) => {
                        Console.WriteLine(ComparisonTest.GetTestRunnerLink(testFile));

                        return false;
                    },
                    (csharpOutput, js) =>
                        Console.WriteLine(csharpOutput)
                );
            } catch (Exception exc) {
                Console.WriteLine(exc.Message);
            }
        }
    }
}
