using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public void SwitchWithMultipleDefaults () {
            var generatedJs = GetJavascript(
                @"TestCases\ComplexSwitch.cs",
                "zero\r\none\r\ntwo or three\r\ntwo or three"
            );
            try {
                // TODO: The following will only work if switch statements with multiple default cases are collapsed into a single default case.

                /*
                Assert.IsFalse(generatedJs.Contains("__ = \"IL_"));
                Assert.IsFalse(generatedJs.Contains("case 1:"));
                 */

                Assert.IsTrue(generatedJs.Contains("default:"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void BigStringSwitch () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\BigStringSwitch.cs",
                ""
            );

            try {
                Assert.IsFalse(generatedJs.Contains(".TryGetValue"));

                // TODO: The following will only work if optimized switches are fully deoptimized back into a normal switch.
                // At present this isn't possible because JSIL cannot fully untangle the flow control graph produced by the optimized switch.

                /*
                Assert.IsFalse(generatedJs.Contains("break "));
                Assert.IsFalse(generatedJs.Contains("continue "));
                 */
                // Assert.IsTrue(generatedJs.Contains("for (var i = 0; i < args.length; ++i)"));

                Assert.IsTrue(generatedJs.Contains("for (var i = 0; i < args.length;"));
                Assert.IsTrue(generatedJs.Contains("switch (text)"));
                Assert.IsTrue(generatedJs.Contains("case \"howdy\""));
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

        // FIXME: We can't treat arrays as constant expressions, so this fails now.
        // [Test]
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
                Assert.IsFalse(Regex.IsMatch(
                    generatedJs,
                    @"!!(\$asm([0-9A-F])*).Program.P"
                ));
                Assert.IsTrue(Regex.IsMatch(
                    generatedJs, 
                    @"!(\$asm([0-9A-F])*).Program.P"
                ));
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
        public void NewParentheses () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\NewParentheses.cs",
                "CustomType"
            );

            try {
                Assert.IsFalse(Regex.IsMatch(
                    generatedJs, 
                    @"\(new (\$asm([0-9A-F])*).CustomType"
                ));
                Assert.IsTrue(Regex.IsMatch(
                    generatedJs, 
                    @"new (\$asm([0-9A-F])*).CustomType\(\)"
                ));
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
            using (var test = MakeTest(@"SpecialTestCases\PrivateNames.cs"))
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
        public void OuterThisNotUsedForDelegateNew () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\OuterThisDelegateNew.cs",
                "PrintNumber(1)\r\nMyClass.PrintNumber(2)"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("outer_this"));
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
                Assert.IsFalse(generatedJs.Contains("for ("), "A for loop failed conversion to a do-loop");
                Assert.AreEqual(3, generatedJs.Split(new string[] { "do {" }, StringSplitOptions.RemoveEmptyEntries).Length);
                Assert.AreEqual(3, generatedJs.Split(new string[] { "} while (" }, StringSplitOptions.RemoveEmptyEntries).Length);
                Assert.IsTrue(generatedJs.Contains("while (true)"));
                Assert.IsTrue(generatedJs.Contains("break $loop2"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void UntranslatableGotos () {
            var generatedJs = GetJavascript(
                @"TestCases\UntranslatableGotoOutParameters.cs",
                ": null"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("JSIL.UntranslatableInstruction"), "A goto failed translation");
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void UntranslatableGotos2 () {
            var generatedJs = GetJavascript(
                @"TestCases\RepeatIterator.cs",
                "a\r\na\r\na\r\na\r\na"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("JSIL.UntranslatableInstruction"), "A goto failed translation");

                var m = Regex.Match(
                    generatedJs,
                    @"if \(this.i \>\= this.count\) \{[^}]*\} else \{"
                );
                bool foundElse = (m != null) && m.Success;

                m = Regex.Match(
                    generatedJs,
                    @"if \(this.i \< this.count\) \{[^}]*\}"
                );
                bool foundIf = (m != null) && m.Success;

                Assert.IsTrue(foundElse || foundIf);

                if (foundElse) {
                    Assert.IsTrue(m.Value.Contains("continue $labelgroup0;"), "If block true clause left empty when hoisting out label");
                } else {
                    Assert.IsTrue(m.Value.Contains("return "), "Return statement not in true clause");
                }
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void SealedMethods () {
            var generatedJs = GetJavascript(
                @"TestCases\SealedMethods.cs",
                "Foo.Func1\r\nFoo.Func2\r\nFoo.Func1"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("Foo.prototype.Func1.call"), "Func1 was called through the prototype with an explicit this");
                Assert.IsTrue(generatedJs.Contains("this.Func1"), "Func1 was not called on this");
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void SealedMethods2 () {
            var output = "F1 F2 F1 F1 B1 B2 F2 F1 B3 B2 F2 F1";

            var generatedJs = GenericTest(
                @"TestCases\SealedMethods2.cs",
                output, output
            );

            try {
                Assert.IsTrue(generatedJs.Contains("Foo.prototype.Func1.call"), "Func1 was not called through the Foo prototype");
                Assert.IsTrue(generatedJs.Contains("Foo.prototype.Func2.call"), "Func2 was not called through the Foo prototype");
                Assert.IsTrue(generatedJs.Contains("this.Func2()"), "Func2 was not called through this");
                Assert.IsTrue(generatedJs.Contains("this.Func2()"), "Func2 was not called through this");

                Assert.IsTrue(generatedJs.Contains("test.Func1()"), "Func1 was not called directly on test");
                Assert.IsTrue(generatedJs.Contains("test.Func2()"), "Func2 was not called directly on test");

                Assert.IsTrue(generatedJs.Contains("test2.Func1()"), "Func1 was not called directly on test");
                Assert.IsTrue(generatedJs.Contains("test2.Func2()"), "Func2 was not called directly on test");
                Assert.IsTrue(generatedJs.Contains("test2.Func3()"), "Func3 was not called directly on test");
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void UnderivedMethods () {
            var generatedJs = GetJavascript(
                @"TestCases\UnderivedMethods.cs",
                "Foo.Func1\r\nFoo.Func2\r\nFoo.Func1"
            );

            try {
                Assert.IsFalse(generatedJs.Contains("Foo.prototype.Func1.call"), "Func1 was called through the prototype with an explicit this");
                Assert.IsTrue(generatedJs.Contains("this.Func1"), "Func1 was not called on this");
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void GenericMethodSignatures () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\GenericMethodSignatures.cs",
                "1"
            );

            try {
                Assert.IsTrue(generatedJs.Contains("\"!!0\", [\"!!0\"], [\"T\"]"));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void CorlibTypeRefs () {
            var generatedJs = GetJavascript(
                @"SpecialTestCases\CorlibTypeRefs.cs",
                "Method(hello)\r\nMethod(MyType(world))"
            );

            try {
                Assert.IsFalse(generatedJs.Contains(".TypeRef(\"System.String\")"), "Long-form string typeref");
                Assert.IsTrue(generatedJs.Contains(".TypeRef(\"MyType\")"), "Long-form custom typeref");

                // Maybe this can be improved?
                Assert.IsTrue(generatedJs.Contains(".TypeRef(\"System.Array\""), "Long-form array typeref");
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void FastOverloadDispatch () {
            var output = "A()\r\nA(1)\r\nA(1, str)\r\nB()\r\nB(int 1)\r\nB(string str)";

            var generatedJs = GenericTest(
                @"SpecialTestCases\FastOverloadDispatch.cs",
                output, output
            );

            try {
                Assert.IsFalse(generatedJs.Contains("CallStatic($asm00.Program, \"A\", "));
                Assert.IsTrue(generatedJs.Contains("$asm00.Program.B();"));
                Assert.IsTrue(generatedJs.Contains("CallStatic($asm00.Program, \"B\", "));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void FastConstructorOverloadDispatch () {
            var output = "A()\r\nA(1)\r\nB()\r\nB(int 1)\r\nB(string s)";

            var generatedJs = GenericTest(
                @"SpecialTestCases\FastConstructorOverloadDispatch.cs",
                output, output
            );

            try {
                Assert.IsFalse(generatedJs.Contains(".Construct($asm00.A"));
                Assert.IsFalse(generatedJs.Contains(".Construct($asm00.B)"));
                Assert.IsTrue(generatedJs.Contains(".Construct($asm00.B, "));
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void OverloadedGenericMethodSignatures () {
            var output = "IsNullOrEmpty with 1 parameters\r\nAny with one argument\r\nfalse";

            var typeInfo = MakeDefaultProvider();

            Action check = () => {
                var generatedJs = GenericTest(
                    @"SpecialTestCases\OverloadedGenericMethodSignatures.cs",
                    output, output, null, typeInfo
                );

                try {
                    Assert.IsTrue(generatedJs.Contains("function Any$b1 (TSource, source)"));
                    Assert.IsTrue(generatedJs.Contains("function Any$b1 (TSource, source, predicate)"));
                } catch {
                    Console.WriteLine(generatedJs);

                    throw;
                }
            };

            for (var i = 0; i < 3; i++)
                check();
        }

        [Test]
        public void OverloadedGenericMethodSignatures2 () {
            var output = "A2\r\nB";

            var typeInfo = MakeDefaultProvider();

            Action check = () => {
                var generatedJs = GenericTest(
                    @"SpecialTestCases\OverloadedGenericMethodSignatures2.cs",
                    output, output, null, typeInfo
                );

                try {
                    Assert.IsTrue(generatedJs.Contains("this.Test("), "this.Test was not direct-dispatched");
                    Assert.IsTrue(generatedJs.Contains("test.Interface_Test2("), "test.Interface_Test2 was not direct-dispatched");
                } catch {
                    Console.WriteLine(generatedJs);

                    throw;
                }
            };

            for (var i = 0; i < 3; i++)
                check();
        }

        [Test]
        public void CustomObjectMethods () {
            var output = "";

            var generatedJs = GenericTest(
                @"SpecialTestCases\CustomObjectMethods.cs",
                output, output
            );

            try {
                Assert.IsFalse(generatedJs.Contains("JSIL.ObjectEquals("), "Base Object.Equals was used");
                Assert.IsFalse(generatedJs.Contains("System.ValueType.$Cast("), "Cast to struct was generated");
            } catch {
                Console.WriteLine(generatedJs);

                throw;
            }
        }

        [Test]
        public void NoUnnecessaryCasts () {
            var testNames = new string[] {
                @"FailingTestCases\ArrayToString.cs",
                @"SimpleTestCases\CollectionInitializers.cs",
                @"TestCases\DictionaryInitializer.cs",
            };

            RunComparisonTests(testNames, null, null, 
                (test) => false, 
                (csharp, js) => {
                    Assert.IsFalse(
                        js.Contains("JSIL.Cast("),
                        "JS output should not contain any casts"
                    );

                    Assert.IsFalse(
                        js.Contains("JSIL.TryCast("),
                        "JS output should not contain any casts"
                    );

                    Assert.IsFalse(
                        js.Contains(".$Cast"),
                        "JS output should not contain any casts"
                    );

                    Assert.IsFalse(
                        js.Contains(".$TryCast"),
                        "JS output should not contain any casts"
                    );
                }
            );
        }
    }
}
