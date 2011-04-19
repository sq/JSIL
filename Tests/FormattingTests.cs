using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class FormattingTests {
        [Test]
        public void EvalIsEmittedIntoBodyOfMethod () {
            long elapsed;
            string generatedJs;
            using (var test = new ComparisonTest(@"SpecialTestCases\ChainedElseIf.cs")) {
                test.RunJavascript(new string[0], out generatedJs, out elapsed);

                try {
                    Assert.AreEqual(
                        4, generatedJs.Split(
                            new string[] { "else if" }, StringSplitOptions.RemoveEmptyEntries
                        ).Length
                    );
                } catch {
                    Console.WriteLine(generatedJs);

                    throw;
                }
            }
        }
    }
}
