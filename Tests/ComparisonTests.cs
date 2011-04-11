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
    public class ComparisonTests {
        [Test]
        public void HelloWorld () {
            var test = new ComparisonTest(@"TestCases\HelloWorld.cs");

            test.Run();
            test.Run("hello", "world");
        }

        [Test]
        public void BinaryTrees () {
            var test = new ComparisonTest(@"TestCases\BinaryTrees.cs");

            test.Run();
            test.Run("6");
        }

        [Test]
        public void NBody () {
            var test = new ComparisonTest(@"TestCases\NBody.cs");

            test.Run();
        }

        // Fails because we emit a goto.
        [Test]
        public void FannkuchRedux () {
            var test = new ComparisonTest(@"TestCases\FannkuchRedux.cs");

            test.Run();
        }

        [Test]
        public void AllSimpleTests () {
            var simpleTests = Directory.GetFiles(
                Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "SimpleTestCases")), 
                "*.cs"
            );
            int failureCount = 0;

            foreach (var filename in simpleTests) {
                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    var test = new ComparisonTest(filename);
                    test.Run();
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                    failureCount += 1;
                }
            }

            Assert.AreEqual(0, failureCount, "One or more tests failed");
        }
    }
}
