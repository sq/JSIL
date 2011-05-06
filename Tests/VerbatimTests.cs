using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class VerbatimTests : GenericTestFixture {
        [Test]
        public void EvalIsReplacedInGeneratedJavascript () {
            GenericTest(
                @"SpecialTestCases\Eval.cs",
                "1", "2"
            );
        }

        [Test]
        public void CustomNamedBuiltins () {
            GenericTest(
                @"SpecialTestCases\IndexBuiltinByName.cs",
                "test", "print"
            );
        }

        [Test]
        public void VerbatimIsEmittedRawInGeneratedJavascript () {
            GenericTest(
                @"SpecialTestCases\Verbatim.cs",
                "1\r\n2", "1"
            );
        }
    }
}
