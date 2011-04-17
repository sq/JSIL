using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class MetadataTests {
        [Test]
        public void JSIgnorePreventsTranslationOfType () {
            long elapsed;
            using (var test = new ComparisonTest(@"SpecialTestCases\IgnoreType.cs")) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                Assert.AreEqual("Test", csOutput.Trim());

                try {
                    string generatedJs;
                    test.RunJavascript(new string[0], out generatedJs, out elapsed);
                    Assert.Fail("Expected javascript to throw");
                } catch (JavaScriptException jse) {
                    if (!jse.ErrorText.Contains("ReferenceError: Test is not defined"))
                        throw;
                }
            }
        }

        [Test]
        public void JSIgnorePreventsTranslationOfMethod () {
            long elapsed;
            using (var test = new ComparisonTest(@"SpecialTestCases\IgnoreMethod.cs")) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                Assert.AreEqual("Foo", csOutput.Trim());

                try {
                    string generatedJs;
                    test.RunJavascript(new string[0], out generatedJs, out elapsed);
                    Assert.Fail("Expected javascript to throw");
                } catch (JavaScriptException jse) {
                    if (!jse.ErrorText.Contains("TypeError: instance.Foo is not a function"))
                        throw;
                }
            }
        }

        [Test]
        public void JSIgnorePreventsTranslationOfProperty () {
            long elapsed;
            string generatedJs;
            using (var test = new ComparisonTest(@"SpecialTestCases\IgnoreProperty.cs")) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                var jsOutput = test.RunJavascript(new string[0], out generatedJs, out elapsed);

                Assert.AreEqual("0", csOutput.Trim());
                Assert.AreEqual("undefined", jsOutput.Trim());
            }
        }

        [Test]
        public void JSIgnorePreventsTranslationOfField () {
            long elapsed;
            string generatedJs;
            using (var test = new ComparisonTest(@"SpecialTestCases\IgnoreField.cs")) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                var jsOutput = test.RunJavascript(new string[0], out generatedJs, out elapsed);

                Assert.AreEqual("1", csOutput.Trim());
                Assert.AreEqual("undefined", jsOutput.Trim());
            }
        }

        [Test]
        public void JSReplacementReplacesMethods () {
            long elapsed;
            string generatedJs;
            using (var test = new ComparisonTest(@"SpecialTestCases\ReplaceMethod.cs")) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                var jsOutput = test.RunJavascript(new string[0], out generatedJs, out elapsed);

                Assert.AreEqual("none", csOutput.Trim());
                Assert.AreEqual("185", jsOutput.Trim());
            }
        }
    }
}
