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
        public void ReplaceMethodBodyWithProxyMethodBody () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\ReplaceMethodBody.cs",
                "ProxiedClass.ProxiedMethod",
                "ProxiedClassProxy.ProxiedMethod"
            );

            Assert.IsFalse(
                generatedJs.Contains("\"ProxiedClass.ProxiedMethod"),
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
        public void ProxiedMethodInheritance () {
            GenericTest(
                @"SpecialTestCases\ProxiedMethodInheritance.cs",
                "DerivedClass.Method1\r\nDerivedClass.Method2\r\nDerivedClass2.Method1\r\nDerivedClass2.Method2",
                "BaseClassProxy.Method1\r\nDerivedClassProxy.Method2\r\nBaseClassProxy.Method1\r\nDerivedClass2.Method2"
            );

            GenericTest(
                @"SpecialTestCases\ProxiedMethodInheritance2.cs",
                "DerivedClass.Method1\r\nBaseClass.Method2\r\nDerivedClass.Method1\r\nDerivedClass2.Method2",
                "BaseClassProxy.Method1\r\nBaseClass.Method2\r\nBaseClassProxy.Method1\r\nDerivedClass2.Method2"
            );
        }

        [Test]
        public void ProxiedOperators () {
            GenericTest(
                @"SpecialTestCases\ProxiedOperators.cs",
                "P 2 P 4", "2 4"
            );
        }

        [Test]
        public void StubbedAssembliesDoNotGenerateMethodBodies () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\StubbedMethodBodies.cs",
                "",
                "The external method 'Main' of type 'Program'",
                new [] { ".*" }
            );

            try {
                Assert.IsTrue(generatedJs.Contains("\"Main"));
                Assert.IsTrue(generatedJs.Contains("\"get_B"));

                // We still want to generate method bodies for auto properties, since they're compiler generated and only
                //  contain a single statement anyway
                Assert.IsTrue(generatedJs.ContainsRegex("MakeMethod(.*\"get_A\".*)"));
                Assert.IsTrue(generatedJs.ContainsRegex("MakeMethod(.*\"get_D\".*)"));

                Assert.IsTrue(generatedJs.Contains("\"set_E"));
                Assert.IsTrue(generatedJs.Contains("\"remove_F"));
                Assert.IsTrue(generatedJs.Contains("\"_ctor"));

                Assert.IsTrue(generatedJs.Contains("$.A$value = 0"));
                Assert.IsTrue(generatedJs.Contains("$.prototype.D$value = 0"));
            } catch {
                Console.WriteLine(generatedJs);
                throw;
            }
        }

        [Test]
        public void RenameMethod () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameMethod.cs",
                "Method",
                "Method"
            );

            Assert.IsFalse(generatedJs.Contains("Program.Method"));
            Assert.IsTrue(generatedJs.Contains("Program.RenamedMethod"));
        }

        [Test]
        public void RenameField () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameField.cs",
                "Field",
                "Field"
            );

            Assert.IsFalse(generatedJs.Contains("Program.Field"));
            Assert.IsTrue(generatedJs.Contains("Program.RenamedField"));
        }

        [Test]
        public void RenameProperty () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameProperty.cs",
                "Property",
                "Property"
            );

            Assert.IsFalse(generatedJs.Contains("Program.Property"));
            Assert.IsTrue(generatedJs.Contains("Program.RenamedProperty"));
        }

        [Test]
        public void RenameEnum () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameEnum.cs",
                "A B MyEnum",
                "A B RenamedEnum"
            );

            Assert.IsFalse(generatedJs.Contains("MyEnum"));
            Assert.IsTrue(generatedJs.Contains("RenamedEnum"));
        }

        [Test]
        public void RenameClass () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameClass.cs",
                "MyClass MyClass",
                "MyClass RenamedClass"
            );

            Assert.IsFalse(generatedJs.Contains(".MyClass()"));
            Assert.IsTrue(generatedJs.Contains(".RenamedClass()"));
        }

        [Test]
        public void RenameStruct () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameStruct.cs",
                "MyStruct MyStruct",
                "MyStruct RenamedStruct"
            );

            Assert.IsFalse(generatedJs.Contains(".MyStruct()"));
            Assert.IsTrue(generatedJs.Contains(".RenamedStruct()"));
        }

        [Test]
        public void RenameStaticClass () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\RenameStaticClass.cs",
                "MyClass MyClass",
                "MyClass RenamedClass"
            );

            Assert.IsFalse(generatedJs.Contains(".MyClass."));
            Assert.IsTrue(generatedJs.Contains(".RenamedClass."));
        }

        [Test]
        public void ExternalMethod () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\ExternalMethod.cs",
                "Method", "external method 'Method' of type 'Program' has not"
            );

            Assert.IsFalse(generatedJs.Contains("MakeMethod($, false, \"Method\""));
            Assert.IsTrue(generatedJs.Contains(".Program.Method("));
        }

        [Test]
        public void ExternalField () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\ExternalField.cs",
                "Field",
                "undefined"
            );

            Assert.IsFalse(generatedJs.Contains(".Field = "));
            Assert.IsTrue(generatedJs.Contains(".Program.Field"));
        }

        [Test]
        public void ExternalProperty () {
            var generatedJs = GenericTest(
                @"SpecialTestCases\ExternalProperty.cs",
                "Property",
                "undefined"
            );

            Assert.IsFalse(generatedJs.Contains("\"get_Property\""));
            Assert.IsFalse(generatedJs.Contains(".MakeProperty($, \"Property\""));
            Assert.IsTrue(generatedJs.Contains(".Program.Property"));
        }

        [Test]
        public void ExternalPropertyGetter () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\ExternalPropertyGetter.cs",
                "Property", "method 'get_Property' of type 'Program' has not"
            );

            Assert.IsFalse(generatedJs.Contains("MakeMethod($, false, \"get_Property\""));
            Assert.IsTrue(generatedJs.Contains(".MakeProperty($, \"Property\""));
            Assert.IsTrue(generatedJs.Contains(".Program.Property"));
        }

        [Test]
        public void ExternalClass () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\ExternalClass.cs",
                "MyClass", "external type 'MyClass' has not"
            );

            Assert.IsFalse(generatedJs.Contains("JSIL.MakeClass"));
            Assert.IsTrue(generatedJs.Contains("MakeExternalType(\"MyClass\""));
            Assert.IsTrue(generatedJs.Contains(".MyClass()"));
        }

        [Test]
        public void ExternalStaticClass () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\ExternalStaticClass.cs",
                "MyClass", "external type 'MyClass' has not"
            );

            Assert.IsTrue(generatedJs.Contains("MakeExternalType(\"MyClass\""));
            Assert.IsTrue(generatedJs.Contains("MyClass."));
        }

        [Test]
        public void ReplaceExternalClass () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\ReplaceExternalClass.cs",
                "MyClass", "ReferenceError: UnqualifiedTypeName is not defined"
            );

            Assert.IsTrue(generatedJs.Contains("UnqualifiedTypeName"));
            Assert.IsFalse(generatedJs.Contains("MyClass"));
        }

        [Test]
        public void ReplaceNonExternalClass () {
            var generatedJs = GenericIgnoreTest(
                @"SpecialTestCases\ReplaceNonExternalClass.cs",
                "MyClass", "ReferenceError: UnqualifiedTypeName is not defined"
            );

            Assert.IsTrue(generatedJs.Contains("UnqualifiedTypeName"));
            Assert.IsTrue(generatedJs.Contains("UnqualifiedTypeName.GetString"));
            Assert.IsTrue(generatedJs.Contains("UnqualifiedTypeName.get_StringProperty"));
            Assert.IsTrue(generatedJs.Contains("UnqualifiedTypeName.set_StringProperty"));
            Assert.IsTrue(generatedJs.Contains("UnqualifiedTypeName.StringField"));
            Assert.IsTrue(generatedJs.Contains("return \"MyClass\""));
            Assert.IsFalse(generatedJs.Contains("MyClass."));
        }
    }
}
