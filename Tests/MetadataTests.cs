using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class MetadataTests : GenericTestFixture {
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
                "Foo\r\nBar\r\nCaught: Error: An attempt was made to reference the member 'Baz()'"
            );
        }
    }
}
