using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JSIL.Internal;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class EmscriptenTests : GenericTestFixture {
        protected override Translator.Configuration MakeConfiguration () {
            var result = base.MakeConfiguration();

            return result;
        }

        [Test]
        [TestCaseSource("EmscriptenTestCasesSource")]
        public void EmscriptenTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> EmscriptenTestCasesSource () {
            return FolderTestSource("EmscriptenTestCases", MakeDefaultProvider(), new AssemblyCache());
        }
    }
}
