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
    public class FailingTests {
        [Test]
        public void AllFailingTests () {
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "FailingTestCases"));
            var simpleTests = Directory.GetFiles(testPath, "*.cs").Concat(Directory.GetFiles(testPath, "*.vb")).ToArray();

            List<string> passedTests = new List<string>();

            foreach (var filename in simpleTests) {
                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    using (var test = new ComparisonTest(filename)) {
                        test.JavascriptExecutionTimeout = 5.0f;
                        test.Run();
                    }

                    passedTests.Add(Path.GetFileName(filename));
                } catch (JavaScriptException jse) {
                    Console.WriteLine(jse.ToString());
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }

            if (passedTests.Count > 0) {
                Assert.Fail("One or more tests passed that should have failed:\r\n" + String.Join("\r\n", passedTests));
            }
        }
    }
}