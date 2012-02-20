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

            int passCount = 0;

            foreach (var filename in simpleTests) {
                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    using (var test = new ComparisonTest(filename))
                        test.Run();

                    passCount += 1;
                } catch (Exception ex) {
                    Console.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
                }
            }

            Assert.AreEqual(0, passCount, "One or more tests passed that should have failed");
        }
    }
}