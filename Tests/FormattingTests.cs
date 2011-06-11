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
        public void StringSwitch () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\StringSwitch.cs",
                ""
            );
            try {
                Assert.IsFalse(generatedJs.Contains("(!text =="));
                Assert.IsTrue(generatedJs.Contains("(!(text =="));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void StringConcat () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\StringConcat.cs",
                "abc\r\nde\r\nab5d"
            );
            try {
                Assert.AreEqual(
                    3,
                    generatedJs.Split(new String[] { "JSIL.ConcatString" }, StringSplitOptions.RemoveEmptyEntries).Length
                );
                Assert.IsFalse(generatedJs.Contains("WriteLine([\"a\", \"b\", 5, \"d\"])"));
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
        public void DisplayClassFieldNames () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\DisplayClassFieldNames.cs",
                "a()=x=1, y=y"
            );

            try {
                Assert.IsTrue(generatedJs.Contains(".x ="));
                Assert.IsTrue(generatedJs.Contains(".y ="));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void EnumeratorClassLocalNames () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\EnumeratorClassLocalNames.cs",
                "0\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9"
            );

            try {
                Assert.IsTrue(generatedJs.Contains("this.i"));
                Assert.IsTrue(generatedJs.Contains("this.$state"));
                Assert.IsTrue(generatedJs.Contains("this.$current"));
                Assert.IsFalse(generatedJs.Contains(".$li$g"));
                Assert.IsFalse(generatedJs.Contains(".$l$g1__state"));
                Assert.IsFalse(generatedJs.Contains(".$l$g2__current"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void RefParametersOnInstanceMethods () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\RefParametersOnInstanceMethods.cs",
                ".B = 0, i = 0\r\n.B = 1, i = 1\r\n.B = 3, i = 2"
            );

            try {
                Assert.IsTrue(generatedJs.Contains("ref */ i"));
                Assert.IsFalse(generatedJs.ToLower().Contains("unmaterialized"));
                Assert.IsTrue(generatedJs.Contains("instance.Method("));
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

        [Test]
        public void ForLoops () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\ForLoops.cs",
                "0\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n5\r\n3\r\n1"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("while"));
                Assert.AreEqual(4, generatedJs.Split(new string[] { "for (" }, StringSplitOptions.RemoveEmptyEntries).Length);
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void FlagsEnumsWithZeroValues () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\FlagsEnumsWithZeroValues.cs",
                "B A\r\nB 0"
            );
            try {
                Assert.IsFalse(generatedJs.Contains("| $asm01.Program.SimpleEnum.E"));
                Assert.IsFalse(generatedJs.Contains("| $asm01.Program.SimpleEnum.A"));
                Assert.IsFalse(generatedJs.Contains("$asm01.Program.SimpleEnum.E |"));
                Assert.IsFalse(generatedJs.Contains("$asm01.Program.SimpleEnum.A |"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void DoLoops () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\DoLoops.cs",
                "0\r\n1\r\n2\r\n3\r\n4\r\n1\r\n16"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("for ("));
                Assert.AreEqual(3, generatedJs.Split(new string[] { "do {" }, StringSplitOptions.RemoveEmptyEntries).Length);
                Assert.AreEqual(3, generatedJs.Split(new string[] { "} while (" }, StringSplitOptions.RemoveEmptyEntries).Length);
                Assert.IsTrue(generatedJs.Contains("while (true)"));
                Assert.IsTrue(generatedJs.Contains("break __loop2__"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }
    }
}
