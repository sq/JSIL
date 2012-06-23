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
        public void Casts() {
            using (var test = MakeTest(@"TestCases\CastToBoolean.cs"))
                test.Run();
            using (var test = MakeTest(@"TestCases\CastingFromNull.cs"))
                test.Run();
        }

        [Test]
        public void BinaryTrees () {
            using (var test = MakeTest(@"TestCases\BinaryTrees.cs")) {
                test.Run();
                test.Run("8");
            }
        }

        [Test]
        public void ForEach () {
            using (var test = MakeTest(@"TestCases\ForEach.cs"))
                test.Run();
        }

        [Test]
        public void Events () {
            using (var test = MakeTest(@"TestCases\Events.cs"))
                test.Run();
            using (var test = MakeTest(@"TestCases\Events.vb"))
                test.Run();
        }

        [Test]
        public void ValueTypeMethods () {
            using (var test = MakeTest(@"TestCases\ValueTypeMethods.cs"))
                test.Run();
        }

        [Test]
        public void Dynamics () {
            RunComparisonTests(
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
                }
            );
        }

        [Test]
        public void Linq () {
            RunComparisonTests(
                new[] { 
                    @"TestCases\LinqSelect.cs",
                    @"TestCases\LinqToArray.cs",
                }
            );
        }

        [Test]
        public void Generics () {
            RunComparisonTests(
                new[] { 
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
                    @"TestCases\GenericParameterNameShadowing.cs"
                }
            );
        }

        [Test]
        public void Structs () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
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
                    @"TestCases\StructLateDeclaration.cs" // This test demonstrates a bug in IntroduceVariableDeclarations
                }, null, defaultProvider
            );
        }

        [Test]
        public void GetTypeByName () {
            using (var test = MakeTest(@"TestCases\GetTypeByName.cs"))
                test.Run();
            using (var test = MakeTest(@"TestCases\GetGenericTypeByName.cs"))
                test.Run();
        }

        [Test]
        public void FieldSpecialCases () {
            using (var test = MakeTest(@"TestCases\FieldRecursiveInitialization.cs"))
                test.Run();
            using (var test = MakeTest(@"TestCases\StringEmpty.cs"))
                test.Run();
        }

        [Test]
        public void MulticastDelegates () {
            using (var test = MakeTest(@"TestCases\MulticastDelegates.cs"))
                test.Run();
        }

        [Test]
        public void Chars () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\CharSwitch.cs",
                    @"TestCases\Chars.cs",
                    @"TestCases\CharArrayLookup.cs",
                    @"TestCases\CharArithmetic.cs",
                    @"TestCases\CharConcat.cs",
                }, null, defaultProvider
            );
        }

        [Test]
        public void Dictionaries () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
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
        public void AllSimpleTests () {
            var typeInfo = MakeDefaultProvider();
            var testPath = Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "SimpleTestCases"));
            var simpleTests = Directory.GetFiles(testPath, "*.cs").Concat(Directory.GetFiles(testPath, "*.vb")).ToArray();

            RunComparisonTests(
                simpleTests, null, typeInfo
            );
        }

        [Test]
        public void LambdaTests () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\LambdasUsingThis.cs",
                    @"TestCases\Lambdas.cs",
                    @"TestCases\LambdasUsingLocals.cs",
                    @"TestCases\DelegatesReturningDelegates.cs",
                    @"TestCases\NestedGenericMethodCalls.cs",
                    @"TestCases\LambdaRefParameters.cs"
                }, null, defaultProvider
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
        public void Goto () {
            using (var test = MakeTest(@"TestCases\Goto.cs"))
                test.Run();
        }

        [Test]
        public void YieldReturn () {
            using (var test = MakeTest(@"TestCases\YieldReturn.cs"))
                test.Run();
        }

        [Test]
        public void FaultBlock () {
            using (var test = MakeTest(@"TestCases\FaultBlock.cs"))
                test.Run();
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
        public void StaticArrays () {
            using (var test = MakeTest(@"TestCases\StaticArrayInitializer.cs"))
                test.Run();
        }

        [Test]
        public void Arithmetic () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\LongArithmetic.cs",
                    @"TestCases\IntegerArithmetic.cs",
                    @"TestCases\TernaryArithmetic.cs",
                    @"TestCases\NullableArithmetic.cs"
                }
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
                    @"TestCases\CastEnumNullableToInt.cs"
                }
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
