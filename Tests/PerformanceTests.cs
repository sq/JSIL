using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using JSIL.Internal;
using JSIL.Translator;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class PerformanceTests : GenericTestFixture {
        Configuration MakeUnsafeConfiguration () {
            var cfg = MakeConfiguration();
            cfg.CodeGenerator.EnableUnsafeCode = true;
            return cfg;
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
        public void UnsafeIntPerformanceComparison () {
            using (var test = MakeTest(
                @"PerformanceTestCases\UnsafeIntPerformanceComparison.cs"
            )) {
                Console.WriteLine(test.RunJavascript(null, MakeUnsafeConfiguration));
            }
        }
    }

    [TestFixture]
    public class PerformanceAnalysisTests : GenericTestFixture {
        Configuration MakeUnsafeConfiguration () {
            var cfg = MakeConfiguration();
            cfg.CodeGenerator.EnableUnsafeCode = true;
            return cfg;
        }

        protected override Dictionary<string, string> SetupEvaluatorEnvironment () {
            return new Dictionary<string, string> {
                { "INFERFLAGS", "result" }
            };
        }

        protected override string JSShellOptions {
            get {
                return "--ion-eager --thread-count=0";
            }
        }

        protected override bool UseDebugJSShell {
            get {
                return true;
            }
        }

        private static readonly Regex TaggedObjectIndexToNameMapping = new Regex(
            @"//(\W*)TAGGED_OBJECT_(?'index'[0-9]*)='(?'name'[^']*)'", RegexOptions.ExplicitCapture
        );
        private static readonly Regex FunctionPrologue = new Regex(
            @"Main(\W*)\#(?'function_index'[0-9]*)(\W*)TAGGED_OBJECT_(?'object_index'[0-9]*)(\W*)\(line", RegexOptions.ExplicitCapture
        );
        private static readonly Regex TaggedObjectType = new Regex(
            @"\#(?'function_index'[ 0-9]+):([ 0-9]+):([ 0-9]+)getgname(.*)" +
            "\"\\$\\$ObjectToTag\"" +
            @"(\W*)typeset(\W*)([ 0-9]+):(\W*)object\[([0-9]*)\](\W*)[\[\<]0x(?'type_id'([0-9A-F]+))[\]\>]",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );
        private static readonly Regex TypeInfoPrologue = new Regex(
            @"(?'class_token'[\[\<])0x(?'type_id'[0-9A-F]+)[\]\>](\W*):(\W*)(\(null\)|((?'parent_class_token'[\[\<])0x(?'parent_type_id'[0-9A-F]+)[\]\>]))(\W*)(?'traits'[^{]+)\{",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );

        private Dictionary<UInt32, string> ParseTaggedObjectTypes (string stderr, string trailingOutput) {
            var taggedObjectIndexesToNames = new Dictionary<int, string>();
            foreach (Match match in TaggedObjectIndexToNameMapping.Matches(stderr))
                taggedObjectIndexesToNames.Add(int.Parse(match.Groups["index"].Value), match.Groups["name"].Value);

            var functionIndicesToTaggedObjectIndexes = new Dictionary<int, int>();
            foreach (Match match in FunctionPrologue.Matches(trailingOutput))
                functionIndicesToTaggedObjectIndexes.Add(int.Parse(match.Groups["function_index"].Value), int.Parse(match.Groups["object_index"].Value));

            var result = new Dictionary<UInt32, string>();
            foreach (Match match in TaggedObjectType.Matches(trailingOutput)) {
                var functionIndex = int.Parse(match.Groups["function_index"].Value);
                var taggedObjectIndex = functionIndicesToTaggedObjectIndexes[functionIndex];
                result.Add(
                    UInt32.Parse(match.Groups["type_id"].Value, NumberStyles.HexNumber),
                    taggedObjectIndexesToNames[taggedObjectIndex]
                );
            }

            if (taggedObjectIndexesToNames.Count != result.Count) {
                foreach (var kvp in taggedObjectIndexesToNames) {
                    if (!functionIndicesToTaggedObjectIndexes.Any(
                            (kvp2) => kvp2.Value == kvp.Key
                        ))
                        Console.Error.WriteLine("Never got function index for '{0}'", kvp.Value);

                    if (!result.Any(
                            (kvp2) => kvp2.Value == kvp.Value
                        ))
                        Console.Error.WriteLine("Never got type id for '{0}'", kvp.Value);
                }
            }

            return result;
        }

        private class ParsedTypeInformation {
            public readonly string Name;
            public readonly UInt32 TypeId;
            public readonly bool IsSingleton;
            public readonly UInt32? ParentTypeId;
            public ParsedTypeInformation ParentType;
            public readonly string[] Traits;

            public ParsedTypeInformation (
                string name,
                UInt32 typeId, bool isSingleton, UInt32? parentTypeId,
                string[] traits
            ) {
                Name = name;
                TypeId = typeId;
                IsSingleton = isSingleton;
                ParentTypeId = parentTypeId;
                Traits = traits;
            }

            public override string ToString () {
                return ToString(true);
            }

            public string ToString (bool shortForm) {
                string result;

                if (shortForm) {
                    result = String.Format(
                        "{0} {2}0x{1:X8}{3}",
                        Name ?? "?", TypeId,
                        IsSingleton ? "<" : "[",
                        IsSingleton ? ">" : "]"
                    );
                } else {
                    result = String.Format(
                        "{0} {4}0x{1:X8}{5} Parent=({2}) Traits={{{3}}}",
                        Name ?? "?", TypeId, ParentType, String.Join(", ", Traits),
                        IsSingleton ? "<" : "[",
                        IsSingleton ? ">" : "]"
                    );
                }

                return result;
            }
        }

        private Dictionary<UInt32, ParsedTypeInformation> ParseTypeInformation (string trailingOutput, Dictionary<UInt32, string> taggedObjectTypes) {
            var result = new Dictionary<UInt32, ParsedTypeInformation>();

            foreach (Match match in TypeInfoPrologue.Matches(trailingOutput)) {
                UInt32? parentTypeId = null;

                var typeId = UInt32.Parse(match.Groups["type_id"].Value, NumberStyles.HexNumber);
                var typeIsSingleton = match.Groups["class_token"].Value == "<";

                if (match.Groups["parent_class_token"].Success)
                    parentTypeId = UInt32.Parse(match.Groups["parent_type_id"].Value, NumberStyles.HexNumber);

                var traits = match.Groups["traits"].Value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string typeName;
                if (!taggedObjectTypes.TryGetValue(typeId, out typeName))
                    typeName = null;

                result.Add(typeId, new ParsedTypeInformation(
                    typeName, typeId, typeIsSingleton, parentTypeId, traits
                ));
            }

            foreach (var kvp in result) {
                if (!kvp.Value.ParentTypeId.HasValue)
                    continue;

                ParsedTypeInformation parentType;
                if (!result.TryGetValue(kvp.Value.ParentTypeId.Value, out parentType))
                    parentType = null;

                kvp.Value.ParentType = parentType;
            }

            return result;
        }

        private void RunPerformanceAnalysis (ComparisonTest test) {
            string trailingOutput;
            string stderr;
            string tempS;
            long tempL;

            var output = test.RunJavascript(
                null, out tempS, out tempL, out tempL, out stderr, out trailingOutput,
                makeConfiguration: MakeUnsafeConfiguration
            );

            var taggedObjectTypes = ParseTaggedObjectTypes(stderr, trailingOutput);
            var typeInformation = ParseTypeInformation(trailingOutput, taggedObjectTypes);

            foreach (var kvp in taggedObjectTypes)
                Console.WriteLine(typeInformation[kvp.Key].ToString(false));
        }

        [Test]
        public void PointerMethodsAreSingletons () {
            using (var test = MakeTest(@"PerformanceTestCases\PointerMethodsAreSingletons.cs")) {
                RunPerformanceAnalysis(test);
            }
        }
    }    
}
