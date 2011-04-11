using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class VerbatimTests {
        [Test]
        public void EvalIsEmittedIntoBodyOfMethod () {
            long elapsed;
            string generatedJs;
            var test = new ComparisonTest(@"SpecialTestCases\Eval.cs");

            var csOutput = test.RunCSharp(new string[0], out elapsed);
            Assert.AreEqual("1", csOutput.Trim());
            var jsOutput = test.RunJavascript(new string[0], out generatedJs, out elapsed);
            Assert.AreEqual("2", jsOutput.Trim());
        }
    }
}
