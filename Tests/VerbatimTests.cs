using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class VerbatimTests : GenericTestFixture {
        [Test]
        public void EvalIsReplacedInGeneratedJavascript () {
            GenericTest(
                @"SpecialTestCases\Eval.cs",
                "1", "2"
            );
        }

        [Test]
        public void JSReplacementTypeOfThisStatic () {
            GenericTest(
                @"SpecialTestCases\JSReplacementTypeOfThisStatic.cs",
                "Program", "Program"
            );
        }

        [Test]
        public void CustomNamedBuiltins () {
            GenericTest(
                @"SpecialTestCases\IndexBuiltinByName.cs",
                "test", "printed\r\nprinted again"
            );
        }

        [Test]
        public void VerbatimIsEmittedRawInGeneratedJavascript () {
            GenericTest(
                @"SpecialTestCases\Verbatim.cs",
                "1\r\n2", "1"
            );
        }

        [Test]
        public void BuiltinsThisEvaluatesToJSThis () {
            GenericTest(
                @"SpecialTestCases\VerbatimThis.cs",
                "", "Program+CustomType"
            );
        }

        [Test]
        public void GetHostService () {
            GenericTest(
                @"SpecialTestCases\GetHostService.cs",
                "threw\r\nnull", "threw\r\nnot null"
            );
        }

        [Test]
        public void CreateNamedFunction () {
            var js = GetJavascript(
                @"SpecialTestCases\CreateNamedFunction.cs",
                "3"
            );
        }

        [Test]
        public void VerbatimVariables () {
            var js = GetJavascript(
                @"SpecialTestCases\VerbatimVariables.cs",
                "hello\r\n7"
            );
        }

        [Test]
        public void VerbatimVariablesExistingArray () {
            var js = GetJavascript(
                @"SpecialTestCases\VerbatimVariablesExistingArray.cs",
                "hello\r\n7"
            );
        }

        [Test]
        public void VerbatimDynamic () {
            var js = GetJavascript(
                @"SpecialTestCases\Issue548.cs",
                "{\"obj1\":\"{}\"}"
            );
        }
    }
}
