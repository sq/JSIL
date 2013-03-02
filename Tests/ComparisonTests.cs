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
                test.Run(new[] { "hello", "world" });
            }
        }

        [Test]
        public void BinaryTrees () {
            using (var test = MakeTest(@"TestCases\BinaryTrees.cs")) {
                test.Run();
                test.Run(new[] { "8" });
            }
        }

        [Test]
        public void NBody () {
            using (var test = MakeTest(@"TestCases\NBody.cs")) {
                test.Run();
                test.Run(new[] { "100000" });
            }
        }

        [Test]
        public void FannkuchRedux () {
            using (var test = MakeTest(@"TestCases\FannkuchRedux.cs")) {
                test.Run();
                test.Run(new[] { "8" });
            }
        }

        [Test]
        [Ignore]
        public void FSharpExecutable () {
            // FIXME: Doesn't work yet.
            var js = GetJavascript(
                @"BinaryTestCases\ConsoleApplication8.exe"
            );
            Console.WriteLine(js);
        }

        [Test]
        public void MonoFixedArray () {
            var js = GetJavascript(
                @"BinaryTestCases\MonoPinArray.exe",
                makeConfiguration: () => {
                    var cfg = MakeConfiguration();
                    cfg.CodeGenerator.EnableUnsafeCode = true;
                    return cfg;
                }
            );
            Console.WriteLine(js);
        }

        [Test]
        public void UnsafeIntPerformanceComparison () {
            using (var test = MakeTest(
                @"SpecialTestCases\UnsafeIntPerformanceComparison.cs"
            )) {
                Console.WriteLine("// setup code //");
                Console.WriteLine(ComparisonTest.EvaluatorSetupCode);

                string js;
                long elapsedJs, elapsedTranslation;

                var output = test.RunJavascript(
                    new string[0], out js, out elapsedTranslation, out elapsedJs, 
                    makeConfiguration: () => {
                        var cfg = MakeConfiguration();
                        cfg.CodeGenerator.EnableUnsafeCode = true;
                        return cfg;
                    }
                );

                Console.WriteLine("// startup prologue //");
                Console.WriteLine(test.StartupPrologue);
                Console.WriteLine("// output //");
                Console.WriteLine(output);
                Console.WriteLine("// generated js //");
                Console.WriteLine(js);
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
                    // FIXME: Needs working ToString for expressions
                    // @"TestCases\LinqExpressionSimple.cs",
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
                    @"TestCases\MethodGenericParameterAsTypeParameter.cs",
                    @"TestCases\MethodGenericParameterAsTypeParameter2.cs",
                    @"TestCases\GenericMethodWithSameSignatureAsNonGenericMethod.cs",
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
                    @"TestCases\NestedStructEquals.cs",
                    @"TestCases\ComplexStructEquals.cs",
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
                    @"TestCases\StructBoxing.cs",
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
        [TestCaseSource("DictionariesSource")]
        public void Dictionaries (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> DictionariesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\Dictionary.cs",
                    @"TestCases\DictionaryInitializer.cs",
                    @"TestCases\DictionaryEnumerator.cs",
                    @"TestCases\DictionaryKeyValuePairs.cs",
                    @"TestCases\DictionaryValueCollectionCount.cs"
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("EnumsSource")]
        public void Enums (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> EnumsSource () {
            return FilenameTestSource(
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
                    @"TestCases\CompareFlagsEnums.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("RefsSource")]
        public void Refs (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> RefsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\RefParameters.cs",
                    @"TestCases\RefParameterInitializedInConditional.cs",
                    @"TestCases\RefStruct.cs",
                    @"TestCases\StructPropertyThis.cs",
                    @"TestCases\RefClass.cs"
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("GotoTestCasesSource")]
        public void Goto (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> GotoTestCasesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\Goto.cs",
                    @"SpecialTestCases\AsyncStateMachineSwitchGoto.cs",
                    @"SpecialTestCases\ElaborateSwitchControlFlow.cs"
                }, MakeDefaultProvider(), new AssemblyCache()
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
                    @"TestCases\GetGenericTypeWithMultipleArgumentsByName.cs",
                    @"TestCases\ValueTypeMethods.cs",
                    @"TestCases\Events.cs",
                    @"TestCases\Events.vb",
                    @"TestCases\ForEach.cs",
                    @"TestCases\CastToBoolean.cs",
                    @"TestCases\CastingFromNull.cs",
                    @"TestCases\YieldReturn.cs",
                    @"TestCases\FaultBlock.cs",
                    @"TestCases\StaticArrayInitializer.cs",
                }, null, new AssemblyCache()
            );
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
        [TestCaseSource("StaticConstructorsSource")]
        public void StaticConstructors (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> StaticConstructorsSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\GenericStaticConstructorOrdering2.cs",
                    // This breaks all the time and has yet to cause actual breakage.
                    // Furthermore, I'm not sure it's meaningful to rely on the ordering. So, it's in failing tests now.
                    // @"TestCases\StaticConstructorOrdering.cs", 
                    @"TestCases\StaticInitializersInGenericTypesSettingStaticFields2.cs"
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        public void SwitchStatements () {
            var defaultProvider = MakeDefaultProvider();

            using (var test = MakeTest(@"SpecialTestCases\BigStringSwitch.cs")) {
                test.Run();
                test.Run(new[] { "howdy", "hello", "world", "what", "why", "who", "where", "when" });
            }

            using (var test = MakeTest(@"SpecialTestCases\AlternateSwitchForm.cs")) {
                test.Run();
                test.Run(new[] { "HP", "MP", "STK", "MAG" });
            }

            RunComparisonTests(
                new[] { 
                    @"TestCases\Switch.cs",
                    @"TestCases\ComplexSwitch.cs",
                    @"TestCases\CharSwitch.cs",
                    @"TestCases\ContinueInsideSwitch.cs",
                    @"SpecialTestCases\AsyncStateMachineSwitchGoto.cs",
                    @"SpecialTestCases\ElaborateSwitchControlFlow.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        [TestCaseSource("ArithmeticSource")]
        public void Arithmetic (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> ArithmeticSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\LongArithmetic.cs",
                    @"TestCases\IntegerArithmetic.cs",
                    @"TestCases\TernaryArithmetic.cs",
                    @"TestCases\NullableArithmetic.cs",
                    @"TestCases\EnumNullableArithmetic.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("NullablesSource")]
        public void Nullables (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> NullablesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\Nullables.cs",
                    @"TestCases\NullableArithmetic.cs",
                    @"TestCases\NullableComparison.cs",
                    @"TestCases\NullableComparisonWithCast.cs",
                    @"TestCases\NullableObjectCast.cs",
                    @"TestCases\CastEnumNullableToInt.cs",
                    @"TestCases\EnumNullableArithmetic.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("TernariesSource")]
        public void Ternaries (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> TernariesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\TernaryArithmetic.cs",
                    @"TestCases\TernaryTypeInference.cs",
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        [TestCaseSource("TemporariesSource")]
        public void Temporaries (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> TemporariesSource () {
            return FilenameTestSource(
                new[] { 
                    @"TestCases\InterleavedTemporaries.cs",
                    @"TestCases\IndirectInterleavedTemporaries.cs",
                    @"TestCases\DirectTemporaryAssignment.cs"
                }, MakeDefaultProvider(), new AssemblyCache()
            );
        }

        [Test]
        public void TypeReferences () {
            RunComparisonTests(
                new[] { 
                    @"SpecialTestCases\CyclicTypeReferences.cs",
                    @"SpecialTestCases\CyclicTypeInheritance.cs"
                }, null, MakeDefaultProvider()
            );
        }

        #region Folders

        [Test]
        [TestCaseSource("SimpleTestCasesSource")]
        public void SimpleTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> SimpleTestCasesSource () {
            return FolderTestSource("SimpleTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("Int64TestCasesSource")]
        public void Int64TestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> Int64TestCasesSource () {
            return FolderTestSource("Int64TestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("IOTestCasesSource")]
        public void IOTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> IOTestCasesSource () {
            return FolderTestSource("IOTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("DateTimeTestCasesSource")]
        public void DateTimeTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> DateTimeTestCasesSource () {
            return FolderTestSource("DateTimeTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("EncodingTestCasesSource")]
        public void EncodingTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> EncodingTestCasesSource () {
            return FolderTestSource("EncodingTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("ReflectionTestCasesSource")]
        public void ReflectionTestCases (object[] parameters) {
            RunSingleComparisonTestCase(parameters);
        }

        protected IEnumerable<TestCaseData> ReflectionTestCasesSource () {
            return FolderTestSource("ReflectionTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        [Test]
        [TestCaseSource("UnsafeTestCasesSource")]
        public void UnsafeTestCases (object[] parameters) {
            RunSingleComparisonTestCase(
                parameters,
                makeConfiguration: () => {
                    var cfg = MakeConfiguration();
                    cfg.CodeGenerator.EnableUnsafeCode = true;
                    return cfg;
                }
            );
        }

        protected IEnumerable<TestCaseData> UnsafeTestCasesSource () {
            return FolderTestSource("UnsafeTestCases", MakeDefaultProvider(), new AssemblyCache());
        }

        #endregion
    }
}
