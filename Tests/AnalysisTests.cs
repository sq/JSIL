using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class AnalysisTests : GenericTestFixture {
        [Test]
        public void FieldAssignmentDetection () {
            var output = "ct=1, mc=(a=0 b=0)\r\nct=1, mc=(a=2 b=1)\r\nct=3, mc=(a=2 b=1)";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\FieldAssignmentDetection.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"mc.UpdateWithNewState(2, ct);"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @"mc.UpdateWithNewState(2, ct.MemberwiseClone());"
            ));
        }

        [Test]
        public void LocalCopyOfGlobal () {
            var output = "1\r\n2\r\n2\r\n2\r\n1\r\n2\r\n3\r\n2\r\n2";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\LocalCopyOfGlobal.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"a = \$thisType.A.MemberwiseClone\(\)"
            ));
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"b = \$thisType.ReturnArgument\($thisType.B\).MemberwiseClone\(\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"c = \$thisType.B.MemberwiseClone\(\)"
            ));
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"d = \$thisType.A.MemberwiseClone\(\)"
            ));
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"e = \$thisType.A.MemberwiseClone\(\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"\$thisType.Field = e.MemberwiseClone\(\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"\$thisType.StoreArgument\(d.MemberwiseClone\(\)\)"
            ));

            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"f = new JSIL.BoxedVariable\(\$thisType\.A\.MemberwiseClone\(\)\)"
            ), "Struct values should be copied when creating a boxed variable from them");
        }

        [Test]
        public void ReturnStructArgument () {
            var output = "a=2, b=1\r\na=2, b=2\r\na=3, b=2";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnStructArgument.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"\$thisType.ReturnArgument\(a.MemberwiseClone\(\)\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = \$thisType.ReturnArgument\(a\).MemberwiseClone\(\);"
            ));
        }

        [Test]
        public void ReturnMutatedStructArgument () {
            var output = "a=2, b=1\r\na=2, b=4\r\na=3, b=4";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnMutatedStructArgument.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"\$thisType.ReturnMutatedArgument\(a.MemberwiseClone\(\), 0\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = \$thisType.ReturnMutatedArgument\(a, 0\).MemberwiseClone\(\);"
            ));
        }

        [Test]
        public void ReturnMutatedNestedStruct () {
            var output = "a=2, b=1\r\na=2, b=4\r\na=3, b=4";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnMutatedNestedStruct.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"\$thisType.ReturnMutatedArgument\(a.MemberwiseClone\(\), 0\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = \$thisType.ReturnMutatedArgument\(a, 0\).MemberwiseClone\(\);"
            ));
        }

        [Test]
        public void IncrementArgumentField () {
            var output = "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\IncrementArgumentField.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".IncrementArgumentValue(a)"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @".IncrementArgumentValue(a.MemberwiseClone())"
            ));
        }

        [Test]
        public void MutateNestedStruct () {
            var output = "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\MutateNestedStruct.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".IncrementArgumentValue(a)"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @".IncrementArgumentValue(a.MemberwiseClone())"
            ));
        }

        [Test]
        public void PureStructOperator () {
            var output = "a=1, b=2, c=3\r\na=1, b=2, c=3\r\n4";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\PureStructOperator.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".op_Addition(a.MemberwiseClone()"
            ), "Argument to op_Addition was cloned");
            Assert.IsFalse(generatedJs.ContainsRegex(
                @"\.op_Addition\([^\)]*\)\.MemberwiseClone\("
            ), "Return value of op_Addition was cloned");
            Assert.IsFalse(generatedJs.Contains(
                @"b.MemberwiseClone())"
            ), "Argument to op_Addition was cloned");
            Assert.IsFalse(generatedJs.Contains(
                @"c.MemberwiseClone())"
            ), "Argument to op_Addition was cloned");
            Assert.IsTrue(generatedJs.Contains(
                @".op_Addition(a, b)"
            ));
        }

        [Test]
        public void RecursivePureStructOperator () {
            var output = "1\r\n0\r\n1\r\n1\r\n0\r\n1";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\RecursivePureStructOperator.cs",
                output, output
            );

            Console.WriteLine(generatedJs);

            // FIXME: Static analyzer too terrible.
            /*
            Assert.IsFalse(generatedJs.Contains(
                @".MemberwiseClone()"
            ), "a value was cloned");
             */
        }

        [Test]
        public void CopyGetEnumerator () {
            var output = "1\r\n2\r\n3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\CopyGetEnumerator.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"GetEnumerator().MemberwiseClone()"
            ), "The enumerator was cloned");
        }

        [Test]
        public void NestedReturnNew () {
            var output = "a=1, b=2, c=3, d=6\r\na=1, b=2, c=3, d=6";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\NestedReturnNew.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".MemberwiseClone()"
            ), "A struct was cloned");
        }

        [Test]
        public void NestedReturn () {
            var output = "a=1, b=2\r\na=3, b=2\r\na=3, b=3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\NestedReturn.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = \$thisType.ReturnArgument\(" +
                @"\$thisType.ReturnIncrementedArgument\(\$thisType.ReturnArgument\(a\)." +
                @"MemberwiseClone\(\)\)\).MemberwiseClone\(\)"
            ));
        }

        [Test]
        public void StructTemporaries () {
            var output = "a = 1";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\StructTemporaries.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains("a.Value"));
            Assert.IsTrue(generatedJs.Contains("b.Value"));
        }

        [Test]
        public void InitTemporaryArray () {
            var output = "";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\InitTemporaryArray.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains("lineListIndices ="));
            Assert.IsTrue(generatedJs.Contains("lineListIndices["));
        }

        [Test]
        public void MakeCopyBeforeMutation () {
            var output = "copy=1, arg=1\r\na=2, b=1\r\ncopy=2, arg=4\r\na=2, b=4\r\na=3, b=4";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\MakeCopyBeforeMutation.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains(
                @"copy = arg.MemberwiseClone()"
            ), "Copy was not cloned");
        }

        [Test]
        public void PointlessFinallyBlocks () {
            var output = "1 4\r\n1 5\r\n1 6\r\n2 4\r\n2 5\r\n2 6\r\n3 4\r\n3 5\r\n3 6";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\PointlessFinallyBlocks.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"Dispose()"
            ), "Enumerator(s) were disposed");
            Assert.IsFalse(generatedJs.Contains(
                @" finally "
            ), "Finally block(s) were generated");
        }

        [Test]
        public void OptimizeArrayEnumerators () {
            var output = "1\r\n2\r\n3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\OptimizeArrayEnumerators.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".GetEnumerator"
            ), "GetEnumerator was called");
            Assert.IsFalse(generatedJs.Contains(
                @".MoveNext()"
            ), "MoveNext was called");
            Assert.IsFalse(generatedJs.Contains(
                @".Current"
            ), "Current was used");
            Assert.IsFalse(generatedJs.Contains(
                @"Dispose()"
            ), "Dispose was called");
        }

        [Test]
        public void ReuseEnumeratorLocal () {
            var output = "1\r\n2\r\n3";

            output += "\r\n" + output + "\r\n" + output;

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReuseEnumeratorLocal.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".GetEnumerator"
            ), "GetEnumerator was called");
            Assert.IsFalse(generatedJs.Contains(
                @".MoveNext()"
            ), "MoveNext was called");
            Assert.IsFalse(generatedJs.Contains(
                @".Current"
            ), "Current was used");
            Assert.IsFalse(generatedJs.Contains(
                @"Dispose()"
            ), "Dispose was called");
        }

        [Test]
        public void StructLoopInteraction () {
            var output = "0\r\n1\r\n2";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\StructLoopInteraction.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void PropertyTemporaries () {
            var output = 
@"Shockwave.WarpTo(72, 80)
Shockwave.TryMove(Right, 384)
Shockwave.WarpTo(72, 80)
Shockwave.TryMove(Up, 384)
Shockwave.WarpTo(72, 80)
Shockwave.TryMove(Left, 384)
Shockwave.WarpTo(72, 80)
Shockwave.TryMove(Down, 384)";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\PropertyTemporaries.cs",
                output, output
            );

            Assert.IsTrue(generatedJs.Contains("var x = "));
            Assert.IsTrue(generatedJs.Contains("var y = "));
            Assert.IsTrue(generatedJs.Contains("/ 8) | 0) * 8"));

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void TemporaryArraySize () {
            var output = "tempArray.Length=64";
            var generatedJs = GenericTest(
                @"AnalysisTestCases\TemporaryArraySize.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void AddToStructProperty () {
            var output = "0\r\n1\r\n2";
            var generatedJs = GenericTest(
                @"AnalysisTestCases\AddToStructProperty.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void VerbatimVariableMutation () {
            var output = "a=1, b=2\r\na=1, b=1\r\na=1, b=3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\VerbatimVariableMutation.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains(
                @"b = a.MemberwiseClone()"
            ), "Copy was not cloned");
        }

        [Test]
        public void StructImmutabilityDetection () {
            var output = "1 1 2 2\r\n1 2 2 5\r\n2 2 5 5";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\StructImmutabilityDetection.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"ICT.MemberwiseClone"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @"CT.MemberwiseClone"
            ));
        }

        [Test]
        public void ImmutableStructThisAssignment () {
            var output = "2 2\r\n1 2";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ImmutableStructThisAssignment.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains(
                @"ict = ict.MemberwiseClone(),"
            ));
        }

        [Test]
        public void SwitchConstructorFolding () {
            var output = "(1, 0, 0)";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\SwitchConstructorFolding.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void ReadStructFromReadonlyFieldThenMutate () {
            var output = "1, 1\r\n1, 3";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReadStructFromReadonlyFieldThenMutate.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void UnfoldableStructConstructor () {
            var output = "(1, 2)";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\UnfoldableStructConstructor.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void ImmutableStructReinitialization () {
            var output = "2 0 0 2\r\n2 3 0 3\r\n2 3 4 4\r\n2 3 4 5";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ImmutableStructReinitialization.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void ImmutableStructReinitializationInControlFlow () {
            var output = "2 0 0 2\r\n2 3 0 3\r\n2 3 0 3\r\n2 3 0 5";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ImmutableStructReinitializationInControlFlow.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void ImmutableStructReinitializationInLoop () {
            var output = "0 1 2 3 4 5 6 7 8";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\ImmutableStructReinitializationInLoop.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void Issue184 () {
            var output = "001.000";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\Issue184.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void AffectedFieldThroughCall () {
            var output = "Expected 100, actual 100.\r\nExpected 10, actual 10.";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\AffectedFieldThroughCall.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void AffectedFieldThroughRecursiveCall () {
            var output = "Expected 100, actual 100.\r\nExpected 10, actual 10.";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\AffectedFieldThroughRecursiveCall.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void TemporaryFunctionCallPurity () {
            var output = "";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\TemporaryFunctionCallPurity.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void StructAllocationHoisting () {
            var output = "0\r\n2000\r\n4000\r\n6000\r\n8000\r\n10000";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\StructAllocationHoisting.cs",
                output, output
            );

            Console.WriteLine(generatedJs);

            Assert.IsFalse(
                generatedJs.Contains("PrintValue(new ($T"),
                "A temporary instance is allocated per loop iteration"
            );

            Assert.IsTrue(
                generatedJs.Contains("new ($T"),
                "An instance was never allocated"
            );
        }

        [Test]
        public void Issue199 () {
            var generatedJs = GetJavascript(@"SpecialTestCases\Issue199.fs");

            Console.WriteLine(generatedJs);
        }

        [Test]
        public void PackedArrayInitializationHoisting () {
            var output = "0\r\n1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\PackedArrayInitializationHoisting.cs",
                output, output
            );

            Console.WriteLine(generatedJs);

            Assert.IsFalse(
                generatedJs.Contains("result.set_Item(i, new ("),
                "A temporary instance is allocated per loop iteration"
            );

            Assert.IsTrue(
                generatedJs.Contains("new ("),
                "An instance was never allocated"
            );
        }

        [Test]
        public void CopyForTemporaryStructInLoop () {
            string output = "0, 0 w=64 h=64\r\n64, 0 w=64 h=64\r\n128, 0 w=64 h=64";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\CopyForTemporaryStructInLoop.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }
    }
}
