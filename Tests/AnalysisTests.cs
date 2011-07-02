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
            var generatedJs = GenericTest(
                @"AnalysisTestCases\FieldAssignmentDetection.cs",
                "ct=1, mc=(a=0 b=0)\r\nct=1, mc=(a=2 b=1)\r\nct=3, mc=(a=2 b=1)",
                "ct=1, mc=(a=0 b=0)\r\nct=1, mc=(a=2 b=1)\r\nct=3, mc=(a=2 b=1)"
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
        public void ReturnStructArgument () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnStructArgument.cs",
                "a=2, b=1\r\na=2, b=2\r\na=3, b=2",
                "a=2, b=1\r\na=2, b=2\r\na=3, b=2"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"Program.ReturnArgument(a.MemberwiseClone())"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @"b = Program.ReturnArgument(a).MemberwiseClone();"
            ));
        }

        [Test]
        public void IncrementArgumentField () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\IncrementArgumentField.cs",
                "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3",
                "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3"
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
            var generatedJs = GenericTest(
                @"AnalysisTestCases\PureStructOperator.cs",
                "a=1, b=2, c=3\r\na=1, b=2, c=3\r\n4",
                "a=1, b=2, c=3\r\na=1, b=2, c=3\r\n4"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".op_Addition(a.MemberwiseClone()"
            ));
            Assert.IsFalse(generatedJs.Contains(
                @"b.MemberwiseClone())"
            ));
            Assert.IsFalse(generatedJs.Contains(
                @"c.MemberwiseClone())"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @".op_Addition(a, b)"
            ));
        }

        [Test]
        public void NestedReturn () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\NestedReturn.cs",
                "a=1, b=2\r\na=3, b=2\r\na=3, b=3",
                "a=1, b=2\r\na=3, b=2\r\na=3, b=3"
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains(
                @"b = Program.ReturnArgument(Program.ReturnIncrementedArgument(Program.ReturnArgument(a).MemberwiseClone())).MemberwiseClone()"
            ));
        }
    }
}
