using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class FormattingTests : GenericTestFixture {
        [Test]
        public void ChainedElseIfs () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\ChainedElseIf.cs",
                "Two"
            );
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

        [Test]
        public void StringConcat () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\StringConcat.cs",
                "abc\r\nde"
            );
            try {
                Assert.AreEqual(
                    2, 
                    generatedJs.Split(new string[] { "System.String.Concat" }, StringSplitOptions.RemoveEmptyEntries).Length
                );
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void PostIncrement () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\PostIncrement.cs",
                "2\r\n3\r\n1\r\n0\r\n0\r\n0\r\n1"
            );
            try {
                Assert.IsFalse(generatedJs.Contains("i + 1"));
                Assert.IsFalse(generatedJs.Contains("i - 1"));
                Assert.IsFalse(generatedJs.Contains("this.Value + value"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void EliminateSingleUseTemporaries () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\SingleUseTemporaries.cs",
                "a\r\nb\r\nc"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("array = objs"));
                Assert.IsFalse(generatedJs.Contains("obj = array[i]"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void EliminateSingleUseExceptionTemporaries () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\SingleUseExceptionTemporaries.cs",
                "a\r\nb"
            );

            try {
                Assert.IsTrue(generatedJs.Contains("ex = $exception"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void NestedInitialization () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\NestedInitialization.cs",
                "a = 5, b = 7\r\na = 5, b = 7"
            );

            try {
                Assert.IsTrue(generatedJs.Contains("var b ="));
                Assert.IsTrue(generatedJs.Contains(", (b = \"7"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void IfBooleanProperty () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\IfBooleanProperty.cs",
                "true\r\nfalse"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("!!Program.P"));
                Assert.IsTrue(generatedJs.Contains("!Program.P"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void PrivateNames () {
            using (var test = new ComparisonTest(@"SpecialTestCases\PrivateNames.cs"))
                test.Run();
        }
    }
}
