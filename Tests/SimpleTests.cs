using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using JSIL.Internal;
using JSIL.Tests;
using NUnit.Framework;

namespace JSIL.SimpleTests {
    using System.Linq;

    [TestFixture]
    public class SimpleTests : GenericTestFixture {
        [Test]
        [TestCaseSource("SimpleTestCasesSource")]
        public void SimpleTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSource ()
        {
            return FolderTestSource("SimpleTestCases", MakeDefaultProvider(), new AssemblyCache());
        }
    }
}

