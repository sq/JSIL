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
        public void CastingFromNull()
        {
            using (var test = new ComparisonTest(@"TestCases\CastingFromNull.cs"))
            {
                test.Run();
            }
        }

        
        [Test]
        public void StaticInitializersInGenericTypesSettingStaticFields()
        {
            using (var test = new ComparisonTest(@"TestCases\StaticInitializersInGenericTypesSettingStaticFields.cs"))
            {
                test.Run();
            }
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
                    @"TestCases\InheritOpenGenericClass.cs",
                    @"TestCases\InheritGenericClass.cs",
                    @"TestCases\GenericMethods.cs",
                    @"TestCases\NestedGenericMethodCalls.cs",
                    @"TestCases\OverloadWithGeneric.cs",
                    @"TestCases\OverloadWithMultipleGeneric.cs",
                    @"TestCases\GenericClasses.cs",
                    @"TestCases\GenericStaticMethods.cs"
                }
            );
        }

        [Test]
        public void Structs () {
            var defaultProvider = MakeDefaultProvider();

            RunComparisonTests(
                new[] { 
                    @"TestCases\StructArrayLiteral.cs",
                    @"TestCases\StructAssignment.cs",
                    @"TestCases\StructDefaults.cs",
                    @"TestCases\StructFields.cs",
                    @"TestCases\StructInitializers.cs",
                    @"TestCases\StructProperties.cs",
                    @"TestCases\StructPropertyThis.cs",
                    @"TestCases\StructThisAssignment.cs",
                    @"TestCases\SingleDimStructArrays.cs",
                    @"TestCases\MultiDimStructArrays.cs"
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
            using (var test = new ComparisonTest(@"TestCases\RefStruct.cs"))
                test.Run();
            using (var test = new ComparisonTest(@"TestCases\StructPropertyThis.cs"))
                test.Run();
            using (var test = new ComparisonTest(@"TestCases\RefClass.cs"))
                test.Run();
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
            var failureList = new List<string>();

            foreach (var filename in simpleTests) {
                Console.Write("// {0} ... ", Path.GetFileName(filename));

                try {
                    // We reuse the same type info provider for all the tests in this folder so they run faster
                    using (var test = new ComparisonTest(filename, null, defaultProvider))
                        test.Run();
                } catch (Exception ex) {
                    failureList.Add(Path.GetFileNameWithoutExtension(filename));
                    if (ex.Message == "JS test failed")
                        Debug.WriteLine(ex.InnerException);
                    else
                        Debug.WriteLine(ex);
                }
            }

            Assert.AreEqual(0, failureList.Count, 
                String.Format("{0} test(s) failed:\r\n{1}", failureList.Count, String.Join("\r\n", failureList.ToArray()))
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
        public void IntegerArithmetic () {
            using (var test = new ComparisonTest(@"TestCases\IntegerArithmetic.cs"))
                test.Run();
        }

        [Test]
        public void Temporaries () {
            using (var test = new ComparisonTest(@"TestCases\InterleavedTemporaries.cs"))
                test.Run();
            using (var test = new ComparisonTest(@"TestCases\IndirectInterleavedTemporaries.cs"))
                test.Run();
            using (var test = new ComparisonTest(@"TestCases\DirectTemporaryAssignment.cs"))
                test.Run();
        }
    }
}
