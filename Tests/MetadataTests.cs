using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class MetadataTests {
        protected void GenericIgnoreTest (string fileName, string workingOutput, string jsErrorSubstring) {
            long elapsed;
            using (var test = new ComparisonTest(fileName)) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                Assert.AreEqual(workingOutput, csOutput.Trim());

                try {
                    string generatedJs;
                    test.RunJavascript(new string[0], out generatedJs, out elapsed);
                    Assert.Fail("Expected javascript to throw an exception containing the string \"" + jsErrorSubstring + "\".");
                } catch (JavaScriptException jse) {
                    if (!jse.ErrorText.Contains(jsErrorSubstring))
                        throw;
                }
            }
        }

        [Test]
        public void JSIgnorePreventsTranslationOfType () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreType.cs",
                "Test",
                "ReferenceError: Test is not defined"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfMethod () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreMethod.cs",
                "Foo",
                "attempt was made to reference the member 'Foo()'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfProperty () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreProperty.cs",
                "0",
                "attempt was made to reference the member 'Property'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfEvent () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreEvent.cs",
                "a",
                "attempt was made to reference the member 'Event'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfField () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreField.cs",
                "1",
                "attempt was made to reference the member 'Field'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfConstructor () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreConstructor.cs",
                "new Test(<int>)\r\nnew Test(<string>)",
                "attempt was made to reference the member '.ctor(s)'"
            );
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

        [Test]
        public void MethodsContainingActualUnsafeCodeIgnored () {
            long elapsed;
            string generatedJs;
            using (var test = new ComparisonTest(@"SpecialTestCases\IgnoreUnsafeCode.cs")) {
                var csOutput = test.RunCSharp(new string[0], out elapsed);
                var jsOutput = test.RunJavascript(new string[0], out generatedJs, out elapsed);

                Assert.AreEqual("Foo\r\nBar\r\nBaz", csOutput.Trim());
                Assert.AreEqual("Foo\r\nBar", jsOutput.Trim());
            }
        }
    }
}
