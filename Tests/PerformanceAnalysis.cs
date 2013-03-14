using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JSIL.Translator;

namespace JSIL.Tests {
    public class ParsedTypeInformation {
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

        public ParsedTypeInformation (
            string name,
            UInt32 typeId, bool isSingleton
        ) {
            Name = name;
            TypeId = typeId;
            IsSingleton = isSingleton;
            ParentTypeId = null;
            ParentType = null;
            Traits = null;
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
                    Name ?? "?", TypeId, ParentType, String.Join(", ", Traits ?? new string[0]),
                    IsSingleton ? "<" : "[",
                    IsSingleton ? ">" : "]"
                );
            }

            return result;
        }
    }

    public class PerformanceAnalysisData {
        private static readonly Regex TaggedObjectIndexToNameMapping = new Regex(
            @"//(\W*)TAGGED_OBJECT_(?'index'[0-9]*)='(?'name'[^']*)'", RegexOptions.ExplicitCapture
        );
        private static readonly Regex FunctionPrologue = new Regex(
            @"Main(\W*)\#(?'function_index'[0-9]*)(\W*)TAGGED_OBJECT_(?'object_index'[0-9]*)(\W*)\(line", RegexOptions.ExplicitCapture
        );
        private static readonly Regex TaggedObjectType = new Regex(
            @"\#(?'function_index'[ 0-9]+):([ 0-9]+):([ 0-9]+)getgname(.*)" +
            "\"\\$\\$ObjectToTag\"" +
            @"(\W*)typeset(\W*)([ 0-9]+):(\W*)object\[([0-9]*)\](\W*)(?'class_token'[\[\<])0x(?'type_id'([0-9A-F]+))[\]\>]",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );
        private static readonly Regex TypeInfoPrologue = new Regex(
            @"(?'class_token'[\[\<])0x(?'type_id'[0-9A-F]+)[\]\>](\W*):(\W*)(\(null\)|((?'parent_class_token'[\[\<])0x(?'parent_type_id'[0-9A-F]+)[\]\>]))(\W*)(?'traits'[^{]+)\{",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );

        public readonly Dictionary<UInt32, String> TaggedObjectTypesByID;
        public readonly Dictionary<String, UInt32> TaggedObjectTypeIDsByName;
        public readonly Dictionary<UInt32, ParsedTypeInformation> TypeInformationByID;
        public readonly string Output;
        public readonly string RawStdErr;
        public readonly string RawTypeInformation;

        public PerformanceAnalysisData (ComparisonTest test, Func<Configuration> makeConfiguration = null) {
            string trailingOutput;
            string stderr;
            string tempS;
            long tempL;

            Output = test.RunJavascript(
                null, out tempS, out tempL, out tempL, out stderr, out trailingOutput,
                makeConfiguration: makeConfiguration
            );

            var singletonTypeIDs = new HashSet<UInt32>();
            TaggedObjectTypesByID = ParseTaggedObjectTypes(stderr, trailingOutput, singletonTypeIDs);
            TypeInformationByID = ParseTypeInformation(trailingOutput, TaggedObjectTypesByID, singletonTypeIDs);

            TaggedObjectTypeIDsByName = new Dictionary<string, uint>(TaggedObjectTypesByID.Count);
            foreach (var kvp in TaggedObjectTypesByID) {
                if (!TaggedObjectTypeIDsByName.ContainsKey(kvp.Value))
                    TaggedObjectTypeIDsByName.Add(kvp.Value, kvp.Key);
            }

            RawStdErr = stderr;
            RawTypeInformation = trailingOutput;
        }

        public ParsedTypeInformation this[string name] {
            get {
                return TypeInformationByID[TaggedObjectTypeIDsByName[name]];
            }
        }

        private Dictionary<UInt32, string> ParseTaggedObjectTypes (string stderr, string trailingOutput, HashSet<UInt32> singletonTypeIDs) {
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
                var typeId = UInt32.Parse(match.Groups["type_id"].Value, NumberStyles.HexNumber);

                if (!result.ContainsKey(typeId))
                    result.Add(
                        typeId, taggedObjectIndexesToNames[taggedObjectIndex]
                    );

                if (match.Groups["class_token"].Value == "<")
                    singletonTypeIDs.Add(typeId);
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

        private Dictionary<UInt32, ParsedTypeInformation> ParseTypeInformation (string trailingOutput, Dictionary<UInt32, string> taggedObjectTypes, HashSet<UInt32> singletonTypeIDs) {
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

            // If js.exe didn't dump type information for this type, infer the singleton status from the other data we got
            //  and add an object for it. Not sure why this happens.

            foreach (var kvp in taggedObjectTypes) {
                if (!result.ContainsKey(kvp.Key))
                    result.Add(kvp.Key, new ParsedTypeInformation(kvp.Value, kvp.Key, singletonTypeIDs.Contains(kvp.Key)));
            }

            return result;
        }

        public void Dump (TextWriter output, bool dumpRaw = false) {
            bool errors = false;

            foreach (var kvp in TaggedObjectTypesByID) {
                if (TypeInformationByID.ContainsKey(kvp.Key)) {
                    output.WriteLine(TypeInformationByID[kvp.Key].ToString(false));
                } else {
                    output.WriteLine("No type info for {0} ({1:X8})", kvp.Value, kvp.Key);
                    errors = true;
                }
            }

            if (errors || dumpRaw) {
                output.WriteLine("// Stderr follows:");
                output.Write(RawStdErr);
                output.WriteLine("// Raw type information follows:");
                output.Write(RawTypeInformation);
            }
        }
    }
}
