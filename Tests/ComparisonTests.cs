using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class ComparisonTests : GenericTestFixture {
        [Test]
        public void HelloWorld () {
            using (var test = new ComparisonTest(@"TestCases\HelloWorld.cs")) {
                test.Run();
                test.Run("hello", "world");
            }
        }

        [Test]
        public void BigStringSwitch () {
            using (var test = new ComparisonTest(@"SpecialTestCases\BigStringSwitch.cs")) {
                test.Run();
                test.Run("howdy", "hello", "world", "what", "why", "who", "where", "when");
            }
        }

        [Test]
        public void CastingFromNull() {
            using (var test = new ComparisonTest(@"TestCases\CastingFromNull.cs"))
                test.Run();
        }

        [Test]
        public void BinaryTrees () {
            using (var test = new ComparisonTest(@"TestCases\BinaryTrees.cs")) {
                test.Run();
                test.Run("8");
            }
        }

        [Test]
        public void ForEach () {
            using (var test = new ComparisonTest(@"TestCases\ForEach.cs"))
                test.Run();
        }

        [Test]
        public void Events () {
            using (var test = new ComparisonTest(@"TestCases\Events.cs"))
                test.Run();
        }

        [Test]
        public void ValueTypeMethods () {
            using (var test = new ComparisonTest(@"TestCases\ValueTypeMethods.cs"))
                test.Run();
        }

        [Test]
        public void Generics () {
            RunComparisonTests(
                new[] { 
                    @"TestCases\GenericStaticProperties.cs",
                    @"TestCases\GenericInnerClasses.cs",
                    @"TestCases\GenericTypeCasts.cs",
                    @"TestCases\GenericArgumentFromTypeReturnedByMethod.cs",
                    @"TestCases\GenericArgumentFromTypePassedToMethod.cs",
                    @"TestCases\GenericStructs.cs",
                    @"TestCases\InheritGenericClass.cs",
                    @"TestCases\InheritOpenGenericClass.cs",
                    @"TestCases\GenericMethods.cs",
                    @"TestCases\NestedGenericMethodCalls.cs",
                    @"TestCases\OverloadWithGeneric.cs",
                    @"TestCases\OverloadWithMultipleGeneric.cs",
                    @"TestCases\GenericClasses.cs",
                    @"TestCases\GenericStaticMethods.cs",
                    @"TestCases\StaticInitializersInGenericTypesSettingStaticFields.cs",
                    @"TestCases\GenericStaticConstructorOrdering.cs"
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
        public void MulticastDelegates () {
            using (var test = new ComparisonTest(@"TestCases\MulticastDelegates.cs"))
                test.Run();
        }

        [Test]
        public void Chars () {
            using (var test = new ComparisonTest(@"TestCases\Chars.cs"))
                test.Run();
            using (var test = new ComparisonTest(@"TestCases\CharSwitch.cs"))
                test.Run();
        }

        [Test]
        public void Enums () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\EnumSwitch.cs",
                    @"TestCases\Enums.cs",
                    @"TestCases\EnumArrayLookup.cs",
                    @"TestCases\OverloadWithEnum.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        public void Refs () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\RefStruct.cs",
                    @"TestCases\StructPropertyThis.cs",
                    @"TestCases\RefClass.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        public void NBody () {
            using (var test = new ComparisonTest(@"TestCases\NBody.cs")) {
                test.Run();
                test.Run("300000");
            }
        }

        [Test]
        public void FannkuchRedux () {
            using (var test = new ComparisonTest(@"TestCases\FannkuchRedux.cs")) {
                test.Run();
                test.Run("10");
            }
        }

        [Test]
        public void AllSimpleTests () {
            var defaultProvider = MakeDefaultProvider();
            var simpleTests = Directory.GetFiles(
                Path.GetFullPath(Path.Combine(ComparisonTest.TestSourceFolder, "SimpleTestCases")), 
                "*.cs"
            );

            RunComparisonTests(
                simpleTests, null, defaultProvider
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

            RunComparisonTests(
                new[] { 
                    @"TestCases\GenericStaticConstructorOrdering.cs",
                    @"TestCases\StaticConstructorOrdering.cs",
                    @"TestCases\StaticInitializersInGenericTypesSettingStaticFields.cs"
                }, null, defaultProvider
            );
        }

        [Test]
        public void Goto () {
            using (var test = new ComparisonTest(@"TestCases\Goto.cs"))
                test.Run();
        }

        [Test]
        public void YieldReturn () {
            using (var test = new ComparisonTest(@"TestCases\YieldReturn.cs"))
                test.Run();
        }

        [Test]
        public void FaultBlock () {
            using (var test = new ComparisonTest(@"TestCases\FaultBlock.cs"))
                test.Run();
        }

        [Test]
        public void Switch () {
            using (var test = new ComparisonTest(@"TestCases\Switch.cs"))
                test.Run();
        }

        [Test]
        public void StaticArrays () {
            using (var test = new ComparisonTest(@"TestCases\StaticArrayInitializer.cs"))
                test.Run();
        }

        [Test]
        public void Arithmetic () {
            using (var test = new ComparisonTest(@"TestCases\IntegerArithmetic.cs"))
                test.Run();
            using (var test = new ComparisonTest(@"TestCases\TernaryArithmetic.cs"))
                test.Run();
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
