using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class AnalysisTests : GenericTestFixture {
        [Test]
        public void ReturnStructArgument () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnStructArgument.cs",
                "a=2, b=1\r\na=2, b=2\r\na=3, b=2",
                "a=2, b=1\r\na=2, b=2\r\na=3, b=2"
            );

            try {
                Assert.IsFalse(generatedJs.Contains(
                    @"Program.ReturnArgument(a.MemberwiseClone())"
                ));
                Assert.IsTrue(generatedJs.Contains(
                    @"b = Program.ReturnArgument(a).MemberwiseClone();"
                ));
            } catch {
                Console.WriteLine(generatedJs);
                throw;
            }
        }
    }
}
