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
            var simpleTests = Directory.GetFiles(
                Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "FailingTestCases")),
                "*.cs"
            );
            int passCount = 0;

            foreach (var filename in simpleTests) {
                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    var test = new ComparisonTest(filename);
                    test.Run();
                    passCount += 1;
                } catch (Exception ex) {
                }
            }

            Assert.AreEqual(0, passCount, "One or more tests passed that should have failed");
        }
    }
}