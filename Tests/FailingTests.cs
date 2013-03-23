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
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class FailingTests : GenericTestFixture {
        [Test]
        [TestCaseSource("FailingTestCasesSource")]
        public void FailingTestCases (object[] parameters) {
            var passed = false;

            try {
                RunSingleComparisonTestCase(parameters);

                passed = true;
            } catch (JavaScriptEvaluatorException jse) {
                Console.WriteLine(jse.ToString());
            } catch (AssertionException ex) {
                Console.WriteLine(ex.ToString());
            }

            Assert.IsFalse(passed, "Test passed when it should have failed");
        }

        protected IEnumerable<TestCaseData> FailingTestCasesSource () {
            return FolderTestSource("FailingTestCases", MakeDefaultProvider(), new AssemblyCache());
        }
    }
}