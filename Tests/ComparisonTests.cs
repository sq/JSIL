using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using JSIL.Internal;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class ComparisonTests : GenericTestFixture {
        [Test]
        public void HelloWorld () {
            using (var test = MakeTest(@"TestCases\HelloWorld.cs")) {
                test.Run();
                test.Run("hello", "world");
            }
        }

        [Test]
        public void BinaryTrees () {
            using (var test = MakeTest(@"TestCases\BinaryTrees.cs")) {
                test.Run();
                test.Run("8");
            }
        }

        [Test]
        public void NBody () {
            using (var test = MakeTest(@"TestCases\NBody.cs")) {
                test.Run();
                test.Run("100000");
            }
        }

        [Test]
        public void FannkuchRedux () {
            using (var test = MakeTest(@"TestCases\FannkuchRedux.cs")) {
                test.Run();
                test.Run("8");
            }
        }
        [Test]
        [TestCaseSource("DynamicsSource")]
        public void Dynamics (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> DynamicsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\DynamicBinaryOperators.cs",
                    @"TestCases\DynamicConversion.cs",
                    @"TestCases\DynamicGetIndex.cs",
                    @"TestCases\DynamicInvoke.cs",
                    @"TestCases\DynamicMethods.cs",
                    @"TestCases\DynamicGenericMethods.cs",
                    @"TestCases\DynamicOverloadedMethods.cs",
                    @"TestCases\DynamicPropertyGet.cs",
                    @"TestCases\DynamicPropertyGetAndCall.cs",
                    @"TestCases\DynamicPropertySet.cs",
                    @"TestCases\DynamicReturnTypes.cs",
                    @"TestCases\DynamicSetIndex.cs",
                    @"TestCases\DynamicStaticOverloadedMethods.cs",
                    @"TestCases\DynamicUnaryOperators.cs",
                }, null, new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("LinqSource")]
        public void Linq (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> LinqSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\LinqSelect.cs",
                    @"TestCases\LinqToArray.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("GenericsSource")]
        public void Generics (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> GenericsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\HiddenMethodFromGenericClass.cs",
                    @"TestCases\MultipleGenericInterfaces.cs",
                    @"TestCases\NestedGenericInheritance.cs",
                    @"TestCases\DelegateResultWithConstraints.cs",
                    @"TestCases\GenericStaticProperties.cs",
                    @"TestCases\GenericInnerClasses.cs",
                    @"TestCases\GenericTypeCasts.cs",
                    @"TestCases\GenericArgumentFromTypeReturnedByMethod.cs",
                    @"TestCases\GenericArgumentFromTypePassedToMethod.cs",
                    @"TestCases\GenericInstanceCallGenericMethod.cs",
                    @"TestCases\GenericStructs.cs",
                    @"TestCases\InheritGenericClass.cs",
                    @"TestCases\InheritOpenGenericClass.cs",
                    @"TestCases\InheritOpenGenericInterface.cs",
                    @"TestCases\GenericMethods.cs",
                    @"TestCases\GenericMethodThisReference.cs",
                    @"TestCases\NestedGenericMethodCalls.cs",
                    @"TestCases\OverloadWithGeneric.cs",
                    @"TestCases\OverloadWithGenericArgCount.cs",
                    @"TestCases\OverloadWithMultipleGeneric.cs",
                    @"TestCases\OverloadWithMultipleGenericThis.cs",
                    @"TestCases\OverloadWithMultipleGenericThisRecursive.cs",
                    @"TestCases\GenericClasses.cs",
                    @"TestCases\GenericStaticMethods.cs",
                    @"TestCases\StaticInitializersInGenericTypesSettingStaticFields.cs",
                    @"TestCases\GenericStaticConstructorOrdering.cs",
                    @"TestCases\MethodOfGenericTypeAsGenericDelegate.cs",
                    @"TestCases\GenericMethodAsGenericDelegate.cs",
                    @"TestCases\GenericNestedTypeConstructedInParentStaticConstructor.cs",
                    @"TestCases\GenericParameterNameShadowing.cs",
                    @"TestCases\MutatedStructGenericParameter.cs",
                    @"TestCases\RefStructThisWithConstrainedInterface.cs",
                    @"TestCases\RefStructThisWithInterface.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("StructsSource")]
        public void Structs (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> StructsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\ReturnStruct.cs",
                    @"TestCases\StructArrayLiteral.cs",
                    @"TestCases\StructAssignment.cs",
                    @"TestCases\StructCompoundAssignment.cs",
                    @"TestCases\StructDefaults.cs",
                    @"TestCases\StructEquals.cs",
                    @"TestCases\StructFields.cs",
                    @"TestCases\InheritStructFields.cs",
                    @"TestCases\StructInitializers.cs",
                    @"TestCases\StructProperties.cs",
                    @"TestCases\StructPropertyThis.cs",
                    @"TestCases\StructThisAssignment.cs",
                    @"TestCases\SingleDimStructArrays.cs",
                    @"TestCases\MultiDimStructArrays.cs",
                    @"TestCases\StructLateDeclaration.cs", // This test demonstrates a bug in IntroduceVariableDeclarations
                    @"TestCases\RefStructThisWithConstrainedInterface.cs",
                    @"TestCases\RefStructThisWithInterface.cs",
                    @"TestCases\MutatedStructGenericParameter.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("FieldSpecialCasesSource")]
        public void FieldSpecialCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> FieldSpecialCasesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\FieldRecursiveInitialization.cs",
                    @"TestCases\StringEmpty.cs",
                    @"TestCases\ArrayFieldWithSelfReference.cs",
                    @"TestCases\CharField.cs",
                    @"TestCases\ArrayFieldOfThisType.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("CharsSource")]
        public void Chars (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> CharsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\CharSwitch.cs",
                    @"TestCases\Chars.cs",
                    @"TestCases\CharArrayLookup.cs",
                    @"TestCases\CharArithmetic.cs",
                    @"TestCases\CharConcat.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        public void Dictionaries () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\Dictionary.cs",
                    @"TestCases\DictionaryInitializer.cs",
                    @"TestCases\DictionaryEnumerator.cs",
                }, null, defaultProvider
            );
        }

        [Test]
        public void Enums () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\EnumComplexArithmetic.cs",
                    @"TestCases\EnumSwitch.cs",
                    @"TestCases\EnumCasts.cs",
                    @"TestCases\Enums.cs",
                    @"TestCases\EnumNegation.cs",
                    @"TestCases\EnumFieldDefaults.cs",
                    @"TestCases\EnumFieldAssignment.cs",
                    @"TestCases\EnumBooleanLogic.cs",
                    @"TestCases\EnumArrayLookup.cs",
                    @"TestCases\OverloadWithEnum.cs",
                    @"TestCases\EnumIfStatement.cs",
                    @"TestCases\CastEnumNullableToInt.cs",
                    @"TestCases\EnumCheckType.cs",
                    @"TestCases\EnumNullableArithmetic.cs",
                    @"TestCases\EnumAnonymousMethod.cs",
                }, null, defaultProvider
            );
        }

        [Test]
        public void Refs () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\RefParameters.cs",
                    @"TestCases\RefParameterInitializedInConditional.cs",
                    @"TestCases\RefStruct.cs",
                    @"TestCases\StructPropertyThis.cs",
                    @"TestCases\RefClass.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        [TestCaseSource("UncategorizedTestCasesSource")]
        public void UncategorizedTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> UncategorizedTestCasesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\HashSetCount.cs",
                    @"TestCases\MulticastDelegates.cs",
                    @"TestCases\GetTypeByName.cs",
                    @"TestCases\GetGenericTypeByName.cs",
                    @"TestCases\ValueTypeMethods.cs",
                    @"TestCases\Events.cs",
                    @"TestCases\Events.vb",
                    @"TestCases\ForEach.cs",
                    @"TestCases\CastToBoolean.cs",
                    @"TestCases\CastingFromNull.cs",
                    @"TestCases\Goto.cs",
                    @"TestCases\YieldReturn.cs",
                    @"TestCases\FaultBlock.cs",
                    @"TestCases\StaticArrayInitializer.cs",
                }, null, new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("SimpleTestCasesSource")]
        public void SimpleTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSource () {
            return FolderTestSource("SimpleTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("LambdaTestsSource")]
        public void Lambdas (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> LambdaTestsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\LambdasUsingThis.cs",
                    @"TestCases\Lambdas.cs",
                    @"TestCases\LambdasUsingLocals.cs",
                    @"TestCases\DelegatesReturningDelegates.cs",
                    @"TestCases\NestedGenericMethodCalls.cs",
                    @"TestCases\LambdaRefParameters.cs"
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }
        
        [Test]
        public void StaticConstructors () {
            var defaultProvider = MakeDefaultProvider();

            // We need a separate copy of these tests otherwise they may share a loaded assembly with the other test fixture.
            RunComparisonTests(
                new[] { 
                    @"TestCases\GenericStaticConstructorOrdering2.cs",
                    // This breaks all the time and has yet to cause actual breakage.
                    // Furthermore, I'm not sure it's meaningful to rely on the ordering. So, it's in failing tests now.
                    // @"TestCases\StaticConstructorOrdering.cs", 
                    @"TestCases\StaticInitializersInGenericTypesSettingStaticFields2.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        public void SwitchStatements () {
            var defaultProvider = MakeDefaultProvider();

            using (var test = MakeTest(@"SpecialTestCases\BigStringSwitch.cs")) {
                test.Run();
                test.Run("howdy", "hello", "world", "what", "why", "who", "where", "when");
            }

            using (var test = MakeTest(@"SpecialTestCases\AlternateSwitchForm.cs")) {
                test.Run();
                test.Run("HP", "MP", "STK", "MAG");
            }

            RunComparisonTests(
                new[] { 
                    @"TestCases\Switch.cs",
                    @"TestCases\ComplexSwitch.cs",
                    @"TestCases\CharSwitch.cs",
                    @"TestCases\ContinueInsideSwitch.cs",
                }, null, defaultProvider
            );
        }

        [Test]
        public void Arithmetic () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\IntegerArithmetic.cs",
                    @"TestCases\TernaryArithmetic.cs",
                    @"TestCases\NullableArithmetic.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        public void Nullables () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\Nullables.cs",
                    @"TestCases\NullableArithmetic.cs",
                    @"TestCases\NullableComparison.cs",
                    @"TestCases\NullableComparisonWithCast.cs",
                    @"TestCases\NullableObjectCast.cs",
                    @"TestCases\CastEnumNullableToInt.cs",
                    @"TestCases\EnumNullableArithmetic.cs",
                }, null, defaultProvider
            );
        }

        [Test]
        public void Ternaries () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\TernaryArithmetic.cs",
                    @"TestCases\TernaryTypeInference.cs",
                }, null, defaultProvider
            );
        }

        [Test]
        public void Temporaries () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\InterleavedTemporaries.cs",
                    @"TestCases\IndirectInterleavedTemporaries.cs",
                    @"TestCases\DirectTemporaryAssignment.cs"
                }, null, defaultProvider
            );
        }
    }
}
