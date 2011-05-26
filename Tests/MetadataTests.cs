using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class MetadataTests : GenericTestFixture {
        [Test]
        public void JSIgnorePreventsTranslationOfType () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreType.cs",
                "Test",
                "attempt was made to reference the member '.ctor()'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfDerivedType () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreDerivedType.cs",
                "DerivedClass",
                "attempt was made to reference the member '.ctor()'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfInterface () {
            GenericTest(
                @"SpecialTestCases\IgnoreInterface.cs",
                "Test.Foo\r\nTrue",
                "Test.Foo\r\nfalse"
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
                "attempt was made to reference the member 'get_Property()'"
            );
        }

        [Test]
        public void JSIgnorePreventsTranslationOfEvent () {
            GenericIgnoreTest(
                @"SpecialTestCases\IgnoreEvent.cs",
                "a",
                "attempt was made to reference the member 'add_Event(value)'"
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
        public void LocalsOfIgnoredTypesAreNotInitialized () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\IgnoreLocal.cs",
                "",
                "null"
            );

            Assert.IsFalse(
                generatedJs.Contains("var local"),
                "Locals of ignored types should not be declared"
            );

            // Not sure it's actually possible to implement this, but it's less important anyway.
            /*
            Assert.IsFalse(
                generatedJs.Contains("local ="),
                "Locals of ignored types should not be assigned"
            );
             */
        }

        [Test]
        public void JSReplacementReplacesMethods () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\ReplaceMethod.cs",
                "none",
                "185"
            );

            Assert.IsFalse(
                generatedJs.Contains("Program.GetJSVersion"), 
                "Replaced methods should not have their body emitted"
            );
        }

        [Test]
        public void MethodsContainingActualUnsafeCodeIgnored () {
            GenericTest(
                @"SpecialTestCases\IgnoreUnsafeCode.cs",
                "Foo\r\nBar\r\nBaz",
                "Foo\r\nBar\r\nCaught: Error: The function 'System.Void Test::Baz()' could not be translated."
            );
        }

        [Test]
        public void ProxiedMethodsWithJSReplacementWorkEvenIfArgumentNamesdoNotMatch () {
            GenericTest(
                @"SpecialTestCases\ReplacementArgumentNameMismatch.cs",
                "2 4", "2 4"
            );
        }

        [Test]
        public void StubbedAssembliesDoNotGenerateMethodBodies () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\StubbedMethodBodies.cs",
                "",
                "The external function 'Main' of namespace 'Program'",
                new [] { new Regex(".*") }
            );

            try {
                Assert.IsTrue(generatedJs.Contains("\"Main"));
                Assert.IsTrue(generatedJs.Contains("\"get_A"));

                Assert.IsTrue(generatedJs.Contains("\"B"));
                Assert.IsTrue(generatedJs.Contains("\"set_C"));
                Assert.IsTrue(generatedJs.Contains("\"remove_D"));
                Assert.IsTrue(generatedJs.Contains("\"_ctor"));

                Assert.IsTrue(generatedJs.Contains("Program.$lA$gk__BackingField = 0"));
                Assert.IsTrue(generatedJs.Contains("T.prototype.$lC$gk__BackingField = 0"));
            } catch {
                Console.WriteLine(generatedJs);
                throw;
            }
        }
    }
}
