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
                @"a = (\$asm([0-9A-F])*).Program.A.MemberwiseClone\(\)"
            ));
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnArgument\((\$asm([0-9A-F])*).Program.B\).MemberwiseClone\(\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"c = (\$asm([0-9A-F])*).Program.B.MemberwiseClone\(\)"
            ));
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"d = (\$asm([0-9A-F])*).Program.A.MemberwiseClone\(\)"
            ));
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"e = (\$asm([0-9A-F])*).Program.A.MemberwiseClone\(\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"(\$asm([0-9A-F])*).Program.Field = e.MemberwiseClone\(\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"(\$asm([0-9A-F])*).Program.StoreArgument\(d.MemberwiseClone\(\)\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"f = new JSIL.Variable\((\$asm([0-9A-F])*).Program.A.MemberwiseClone\(\)\)"
            ));
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
                @"(\$asm([0-9A-F])*).Program.ReturnArgument\(a.MemberwiseClone\(\)\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnArgument\(a\).MemberwiseClone\(\);"
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
                @"(\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a.MemberwiseClone\(\), 0\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a, 0\).MemberwiseClone\(\);"
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
                @"(\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a.MemberwiseClone\(\), 0\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a, 0\).MemberwiseClone\(\);"
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
                @"b = (\$asm([0-9A-F])*).Program.ReturnArgument\((\$asm([0-9A-F])*)." +
                @"Program.ReturnIncrementedArgument\((\$asm([0-9A-F])*).Program.ReturnArgument\(a\)." +
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
        public void StructLoopInteraction () {
            var output = "0\r\n1\r\n2";

            var generatedJs = GenericTest(
                @"AnalysisTestCases\StructLoopInteraction.cs",
                output, output
            );

            Console.WriteLine(generatedJs);
        }
    }
}
