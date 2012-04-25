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
    public class ReflectionTests : GenericTestFixture {
        [Test]
        public void AllReflectionTests () {
            var defaultProvider = MakeDefaultProvider();
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "ReflectionTestCases"));
            var simpleTests = Directory.GetFiles(testPath, "*.cs")
                .Concat(Directory.GetFiles(testPath, "*.vb"))
                .ToArray();

            RunComparisonTests(
                simpleTests, null, defaultProvider
            );
        }
    }
}
