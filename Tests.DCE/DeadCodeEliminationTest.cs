namespace JSIL.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JSIL.Compiler.Extensibility.DeadCodeAnalyzer;
    using NUnit.Framework;


    [TestFixture]
    class DeadCodeEliminationTest : GenericTestFixture
    {
        protected string GetJavascriptWithDCE(string fileName, string expectedText = null)
        {
            var dce = new DeadCodeAnalyzer();
            var analyzerSettings = new Dictionary<string, object>
            {
                {"DeadCodeElimination", true},
                {"NonAggressiveVirtualMethodElimination", false},
                {"WhiteList", new List<string> {@"System\.Void Program::Main\(.*\)"}}
            };
            dce.SetConfiguration(analyzerSettings);

            var defaultConfiguration = MakeConfiguration();
            var generatedJs = GetJavascript(
                Path.Combine(@"..\Tests.DCE", fileName),
                expectedText,
                makeConfiguration: () => defaultConfiguration,
                analyzers: new[] { dce }
            );

            return generatedJs;
        }

        [Test]
        public void BasicDCEFunctions()
        {
            var output = GetJavascriptWithDCE(@"DCETests\BasicDCEFunctions.cs");

            // Check that we preserved starting point and members it reference is Program.
            DceAssert.Has(output, MemberType.Class, "Program", true);
            DceAssert.Has(output, MemberType.Method, "Main", true);
            DceAssert.Has(output, MemberType.Method, "UsedFunctionInProgram", true);
            DceAssert.Has(output, MemberType.Field, "UsedFieldInProgram", true);
            DceAssert.Has(output, MemberType.Property, "UsedPropertyInProgram", true);
            DceAssert.Has(output, MemberType.Event, "UsedEventInProgram", true);

            // Check that we eliminated unused Program members.
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFunctionInProgram");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFieldInProgram");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedPropertyInProgram");
            DceAssert.HasNo(output, MemberType.Event, "UnusedEventInProgram", true);

            // Check that we eliminated UnusedStaticClass and UnusedClass.
            DceAssert.HasNo(output, MemberType.Mention, "UnusedStaticClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFunctionInUnusedStaticClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFieldInUnusedStaticClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedPropertyInUnusedStaticClass");

            DceAssert.HasNo(output, MemberType.Mention, "UnusedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFunctionInUnusedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFieldInUnusedStaticClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedStaticPropertyInUnusedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedPropertyInUnusedClass");

            // Check that we preserved members in UsedClass and UsedStaticClass reachable from Program.Main.
            DceAssert.Has(output, MemberType.Class, "UsedStaticClass", true);
            DceAssert.Has(output, MemberType.Method, "UsedFunctionInUsedStaticClass", true);
            DceAssert.Has(output, MemberType.Field, "UsedFieldInUsedStaticClass", true);

            DceAssert.Has(output, MemberType.Class, "UsedClass", false);
            DceAssert.Has(output, MemberType.Method, "UsedStaticFunctionInUsedClass", true);
            DceAssert.Has(output, MemberType.Method, "UsedFunctionInUsedClass", false);
            DceAssert.Has(output, MemberType.Field, "UsedStaticFieldInUsedClass", true);
            DceAssert.Has(output, MemberType.Field, "UsedFieldInUsedClass", false);
            DceAssert.Has(output, MemberType.Property, "UsedStaticPropertyInUsedClass", true);
            DceAssert.Has(output, MemberType.Property, "UsedPropertyInUsedClass", false);
            DceAssert.Has(output, MemberType.Event, "UsedStaticEventInUsedClass", true);
            DceAssert.Has(output, MemberType.Event, "UsedEventInUsedClass", false);

            // Check that we eliminated members in UsedClass and UsedStaticClass non-reachable from Program.Main.
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFunctionInUsedStaticClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFieldInUsedStaticClass");

            DceAssert.HasNo(output, MemberType.Mention, "UnusedStaticFunctionInUsedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFunctionInUsedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedStaticFieldInUsedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedFieldInUsedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedPropertyInUsedClass");
            DceAssert.HasNo(output, MemberType.Mention, "UnusedStaticPropertyInUsedClass");
            DceAssert.HasNo(output, MemberType.Event, "UnusedStaticEventInUsedClass", true);
            DceAssert.HasNo(output, MemberType.Event, "UnusedEventInUsedClass", false);
        }

        [Test]
        public void PreserveTypesReferencedFromFieldDeclaration()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveTypesReferencedFromFieldDeclaration.cs");
            DceAssert.Has(output, MemberType.Class, "PreservedType", false);
            DceAssert.HasNo(output, MemberType.Mention, "StrippedType");
        }

        [Test]
        public void PreserveTypesReferencedFromMethodDeclaration()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveTypesReferencedFromMethodDeclaration.cs");
            DceAssert.Has(output, MemberType.Class, "PreservedMethodDeclarationType", false);
            DceAssert.Has(output, MemberType.Class, "PreservedMethodReturnType", false);
            DceAssert.Has(output, MemberType.Class, "PreservedMethodFirstArgumentType", false);
            DceAssert.Has(output, MemberType.Class, "PreservedMethodSecondArgumentType", false);
            DceAssert.HasNo(output, MemberType.Mention, "StrippedType");
        }

        [Test]
        public void PreserveTypesReferencedFromGeneric()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveTypesReferencedFromGeneric.cs");
            // Generic type constructor.
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericTypeConstructorGenericTypeGen1`1", false);
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericTypeConstructorGenericTypeGen2`1", false);
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericTypeConstructor", false);

            // Generic type static method.
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericTypeStaticMethodGenericTypeGen1`1", false);
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericTypeStaticMethodGenericTypeGen2`1", false);
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericTypeStaticMethod", false);

            // Generic type static method.
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericMethodMethodGenericTypeGen1`1", false);
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericMethodGenericTypeGen2`1", false);
            DceAssert.Has(output, MemberType.Class, "PreservedFromGenericMethod", false);

            DceAssert.HasNo(output, MemberType.Mention, "StrippedType");
        }

        [Test]
        public void PreserveTypesGenericBase()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveTypesGenericBase.cs");
            DceAssert.Has(output, MemberType.Class, "BaseGenericType`2", false);
            DceAssert.Has(output, MemberType.Class, "DerivedGenericType`1", false);
            DceAssert.Has(output, MemberType.Class, "NonGenericDerivedType", false);
            DceAssert.Has(output, MemberType.Class, "TypeForT", false);
            DceAssert.Has(output, MemberType.Class, "TypeForK", false);

            DceAssert.HasNo(output, MemberType.Mention, "StrippedType");
        }

        [Test]
        public void PreserveStaticConstructorAndReferences()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveStaticConstructorAndReferences.cs");
            DceAssert.Has(output, MemberType.Class, "PreservedFromTypeReference", false);
            StringAssert.Contains("Hello from .cctor", output, "Static constructor eliminated, should be preserved.");

            DceAssert.HasNo(output, MemberType.Mention, "StrippedType");
        }

        [Test]
        public void PreserveVirtualMethodImplementation()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveVirtualMethodImplementation.cs");

            // Stripped Method from UsedDerivedType that hides used method from BaseType.
            StringAssert.Contains("BaseType.Method - used", output, "BaseType.Method eliminated, should be preserved");
            StringAssert.DoesNotContain("UsedDerivedType.Method - used", output, "UsedDerivedType.Method preserved, should be eliminated");

            // Preserve virtual method from used type.
            StringAssert.Contains("BaseType.MethodFromBaseType - used", output, "BaseType.MethodFromBaseType, should be preserved");
            StringAssert.Contains("UsedDerivedType.MethodFromBaseType - used", output, "UsedDerivedType.MethodFromBaseType, should be preserved");

            // Preserve used method from used interface from used type.
            DceAssert.Has(output, MemberType.Interface, "IIterface", false);
            StringAssert.Contains("UsedDerivedType.MethodFromIIterface - used", output, "UsedDerivedType.MethodFromIIterface eliminated, should be preserved");

            // Stripped not-used method virtual method.
            StringAssert.DoesNotContain("BaseType.UnusedMethodFromBaseType - used", output, "BaseType.UnusedMethodFromBaseType preserved, should be eliminated");
            StringAssert.DoesNotContain("UsedDerivedType.UnusedMethodFromBaseType - used", output, "UsedDerivedType.UnusedMethodFromBaseType preserved, should be eliminated");

            // Stripped not-used interface and members.
            DceAssert.HasNo(output, MemberType.Interface, "IIterfaceNotUsed", false);
            DceAssert.HasNo(output, MemberType.Method, "UnusedMethodFromIIterfaceNotUsed", false);
            StringAssert.DoesNotContain("UsedDerivedType.UnusedMethodFromIIterfaceNotUsed - used", output, "UsedDerivedType.UnusedMethodFromIIterfaceNotUsed preserved, should be eliminated");

            // Preserved used members - implementation for not-used interface.
            StringAssert.DoesNotContain("UsedDerivedType.UsedMethodFromIIterfaceNotUsed - used", output, "UsedDerivedType.UsedMethodFromIIterfaceNotUsed preserved, should be eliminated");

            // Striped fully not-used type
            StringAssert.DoesNotContain("UnusedDerivedType.Method - used", output, "UnusedDerivedType.Method preserved, should be eliminated");
            StringAssert.DoesNotContain("UnusedDerivedType.MethodFromIIterface - used", output, "UnusedDerivedType.MethodFromIIterface, should be eliminated");
            StringAssert.DoesNotContain("UnusedDerivedType.MethodFromBaseType - used", output, "UnusedDerivedType.MethodFromBaseType, should be eliminated");
        }

        [Test]
        public void PreserveVirtualMethodFromReallyUsedRootOnly()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveVirtualMethodFromReallyUsedRootOnly.cs");

            DceAssert.Has(output, MemberType.Class, "BaseType", false);
            DceAssert.Has(output, MemberType.Class, "UsedMiddleDerivedType", false);
            DceAssert.Has(output, MemberType.Class, "UsedDerivedType", false);
            DceAssert.Has(output, MemberType.Class, "DerivedTypeWithoudMethodUsage", false);

            StringAssert.DoesNotContain("BaseType.Method - used", output, "BaseType.Method preserved, should be eliminated");
            StringAssert.Contains("UsedMiddleDerivedType.Method - used", output, "UsedMiddleDerivedType.Method eliminated, should be preserved");
            StringAssert.Contains("UsedDerivedType.Method - used", output, "UsedDerivedType.Method eliminated, should be preserved");
            StringAssert.DoesNotContain("DerivedTypeWithoudMethodUsage.Method - used", output, "DerivedTypeWithoudMethodUsage.Method preserved, should be eliminated");

            StringAssert.Contains("BaseClassWithPreservedMethod.Method - used", output, "BaseClassWithPreservedMethod.Method eliminated, should be preserved");
        }

        [Test]
        public void PreserveUsageThroughConstraint()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveUsageThroughConstraint.cs");

            DceAssert.Has(output, MemberType.Interface, "IIterfaceForGenericMethodTest", false);
            DceAssert.Has(output, MemberType.Interface, "IIterfaceForGenericClassTest", false);
        }

        [Test]
        public void PreserveTypesReferencedFromGenericField()
        {
            var output = GetJavascriptWithDCE(@"DCETests\PreserveTypesReferencedFromGenericField.cs");

            DceAssert.Has(output, MemberType.Class, "NonGenericType", false);
        }

        [Test]
        public void Attributes()
        {
            var output = GetJavascriptWithDCE(@"DCETests\Attributes.cs");
            DceAssert.Has(output, MemberType.Class, "TestAttribute", false);
            StringAssert.Contains("Test arg", output, "TestAttribute application eliminated, should be preserved");
            StringAssert.Contains("TestEnum.A", output, "TestAttribute application eliminated, should be preserved");
            StringAssert.Contains("D_Usage", output, "TestEnum eliminated, should be preserved");

            DceAssert.HasNo(output, MemberType.Mention, "UnusedAttribute", false);
            StringAssert.DoesNotContain("Unused arg", output, "UnusedAttribute application preserved, should be eliminated");
            StringAssert.DoesNotContain("C_Usage", output, "UnusedEnum preserved, should be eliminated");
        }

        [Test]
        public void DCEMetaAttributes()
        {
            var output = GetJavascriptWithDCE(@"DCETests\DCEMetaAttributes.cs");
            DceAssert.Has(output, MemberType.Field, "UnusedFieldWithAttribute", true);
            DceAssert.Has(output, MemberType.Method, "UnusedMethodWithAttribute", true);

            DceAssert.Has(output, MemberType.Class, "UnusedClassWithAttribute", false);
            DceAssert.HasNo(output, MemberType.Mention, "MethodInUnusedClassWithAttribute", false);

            DceAssert.Has(output, MemberType.Class, "UnusedClassWithClassEntryPointAttribute", false);
            DceAssert.Has(output, MemberType.Method, "MethodInUnusedClassWithClassEntryPointAttribute", false);

            DceAssert.Has(output, MemberType.Class, "UnusedClassWithHierarchyEntryPointAttribute", false);
            DceAssert.Has(output, MemberType.Method, "MethodInUnusedClassWithHierarchyEntryPointAttribute", false);

            DceAssert.HasNo(output, MemberType.Mention, "DerivedUnusedClassWithAttribute", false);
            DceAssert.HasNo(output, MemberType.Mention, "MethodInDerivedUnusedClassWithAttribute", false);

            DceAssert.HasNo(output, MemberType.Mention, "DerivedUnusedClassWithClassEntryPointAttribute", false);
            DceAssert.HasNo(output, MemberType.Mention, "MethodInDerivedUnusedClassWithClassEntryPointAttribute", false);

            DceAssert.Has(output, MemberType.Class, "DerivedUnusedClassWithHierarchyEntryPointAttribute", false);
            DceAssert.Has(output, MemberType.Method, "MethodInDerivedUnusedClassWithHierarchyEntryPointAttribute", false);
        }

        [Test]
        public void DCEMetaAttributesOnProxies()
        {
            var output = GetJavascriptWithDCE(@"DCETests\DCEMetaAttributesOnProxies.cs");
            DceAssert.Has(output, MemberType.Field, "UnusedFieldWithAttribute", true);
            DceAssert.Has(output, MemberType.Method, "UnusedMethodWithAttribute", true);

            DceAssert.Has(output, MemberType.Class, "UnusedClassWithAttribute", false);
            DceAssert.HasNo(output, MemberType.Mention, "MethodInUnusedClassWithAttribute", false);

            DceAssert.Has(output, MemberType.Class, "UnusedClassWithClassEntryPointAttribute", false);
            DceAssert.Has(output, MemberType.Method, "MethodInUnusedClassWithClassEntryPointAttribute", false);

            DceAssert.Has(output, MemberType.Class, "UnusedClassWithHierarchyEntryPointAttribute", false);
            DceAssert.Has(output, MemberType.Method, "MethodInUnusedClassWithHierarchyEntryPointAttribute", false);

            DceAssert.HasNo(output, MemberType.Mention, "DerivedUnusedClassWithAttribute", false);
            DceAssert.HasNo(output, MemberType.Mention, "MethodInDerivedUnusedClassWithAttribute", false);

            DceAssert.HasNo(output, MemberType.Mention, "DerivedUnusedClassWithClassEntryPointAttribute", false);
            DceAssert.HasNo(output, MemberType.Mention, "MethodInDerivedUnusedClassWithClassEntryPointAttribute", false);

            DceAssert.Has(output, MemberType.Class, "DerivedUnusedClassWithHierarchyEntryPointAttribute", false);
            DceAssert.Has(output, MemberType.Method, "MethodInDerivedUnusedClassWithHierarchyEntryPointAttribute", false);
        }

        [Test]
        public void StripOuterTypes()
        {
            var output = GetJavascriptWithDCE(@"DCETests\StripOuterTypes.cs");
            // Generic type constructor.
            DceAssert.Has(output, MemberType.Class, "OuterType+InnerType", false);
            DceAssert.HasNo(output, MemberType.Class, "OuterType", false);

            DceAssert.HasNo(output, MemberType.Mention, "StrippedType");
         }

        [Test]
        public void StripInterfaceImplementation()
        {
            var output = GetJavascriptWithDCE(@"DCETests\StripInterfaceImplementation.cs");
            // Generic type constructor.
            DceAssert.Has(output, MemberType.Interface, "IUsedInterface", false);
            DceAssert.Has(output, MemberType.Class, "UsedType", false);

            DceAssert.HasNo(output, MemberType.Mention, "IUnUsedInterface");
        }
    }

    public static class DceAssert
    {
        public static void Has(string input, MemberType memberType, string name, bool isStatic = true, bool isPublic = true)
        {
            var message = string.Format("No {0} ({1})", name, memberType);
            Assert.IsTrue(Contains(input, memberType, name, isStatic, isPublic), message);
        }

        public static void HasNo(string input, MemberType memberType, string name, bool isStatic = true, bool isPublic = true)
        {
            var message = string.Format("{0} ({1}) found, shoud be eliminated", name, memberType);
            Assert.IsFalse(Contains(input, memberType, name, isStatic, isPublic), message);
        }

        private static bool Contains(string input, MemberType memberType, string name, bool isStatic, bool isPublic)
        {
            bool contains = false;
            switch (memberType)
            {
                case MemberType.Mention:
                    contains = HasMention(input, name);
                    break;
                case MemberType.Class:
                    contains = HasClassDefenition(input, name, isStatic);
                    break;
                case MemberType.Interface:
                    contains = HasInterfaceDefenition(input, name, isPublic);
                    break;
                case MemberType.Method:
                    contains = HasMethodDefenition(input, name, isStatic, isPublic);
                    break;
                case MemberType.Field:
                    contains = HasFieldDefenition(input, name, isStatic, isPublic);
                    break;
                case MemberType.Property:
                    contains = HasPropertyDefenition(input, name, isStatic, isPublic);
                    break;
                case MemberType.Event:
                    contains = HasEventDefenition(input, name, isStatic, isPublic);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("memberType");
            }

            return contains;
        }


        private static bool HasMethodDefenition(string input, string name, bool isStatic, bool isPublic = true)
        {
            var searchPattern = string.Format("$.Method({{Static:{1}, Public:{2}}}, \"{0}\"", name, Padded(isStatic), Padded(isPublic));
            return input.Contains(searchPattern);
        }

        private static bool HasFieldDefenition(string input, string name, bool isStatic, bool isPublic = true)
        {
            var searchPattern = string.Format("$.Field({{Static:{1}, Public:{2}}}, \"{0}\"", name, Padded(isStatic), Padded(isPublic));
            return input.Contains(searchPattern);
        }

        private static bool HasPropertyDefenition(string input, string name, bool isStatic, bool isPublic = true)
        {
            var searchPattern = string.Format("$.Property({{Static:{1}, Public:{2}}}, \"{0}\"", name, Padded(isStatic), Padded(isPublic));
            return input.Contains(searchPattern);
        }

        private static bool HasEventDefenition(string input, string name, bool isStatic, bool isPublic = true)
        {
            var searchPattern = string.Format("$.Event({{Static:{1}, Public:{2}}}, \"{0}\"", name, Padded(isStatic), Padded(isPublic));
            return input.Contains(searchPattern);
        }

        private static bool HasClassDefenition(string input, string name, bool isStatic)
        {
            var searchPattern = isStatic
                ? string.Format("JSIL.MakeStaticClass(\"{0}\"", name)
                : string.Format("Name: \"{0}\"", name);
            return input.Contains(searchPattern);
        }

        private static bool HasInterfaceDefenition(string input, string name, bool isPublic)
        {
            var searchPattern = string.Format("\"{0}\", {1}, [", name, isPublic.ToString().ToLower());
            return input.Contains(searchPattern);
        }

        private static bool HasMention(string input, string name)
        {
            return input.Contains(name);
        }

        private static string Padded(bool value)
        {
            return value ? "true " : "false";
        }
    }

    public enum MemberType
    {
        Mention,
        Class,
        Interface,
        Method,
        Field,
        Property,
        Event
    }
}
