using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JSIL.Compiler {
    public class VariableSet {
        private readonly Dictionary<string, Func<string>> Values = new Dictionary<string, Func<string>>();
        private static readonly Regex VariableRegex = new Regex(
            "\\%(?'variablename'[^\\%]*)\\%", RegexOptions.Compiled
        );

        public VariableSet () {
        }

        private VariableSet (VariableSet other) {
            foreach (var kvp in other.Values)
                Values.Add(kvp.Key, kvp.Value);
        }

        public IEnumerable<string> DefinedVariables {
            get {
                foreach (var key in Values.Keys)
                    yield return String.Format("%{0}%", key);
            }
        }

        public void Add (string key, string value) {
            Values.Add(key.ToLower(), () => value);
        }

        public void Add (string key, Func<string> value) {
            Values.Add(key.ToLower(), value);
        }

        public Func<string> this[string key] {
            set {
                Values[key.ToLower()] = value;
            }
        }

        private string Expander (string text, Match match) {
            if (match.Groups["variablename"].Success) {
                var key = match.Groups["variablename"].Value.ToLower();

                Func<string> variableFn;
                string variableValue;
                if (Values.TryGetValue(key, out variableFn)) {
                    return variableFn();
                } else if ((variableValue = Environment.GetEnvironmentVariable(key)) != null) {
                    return variableValue;
                } else {
                    throw new VariableExpansionException(
                        text, this, key
                    );
                }
            }

            // Expansion failed
            return match.Value;
        }

        public string Expand (string text) {
            if (text == null)
                return null;

            var result = text;

            bool needAnotherPass = true;

            while (needAnotherPass) {
                needAnotherPass = false;

                var expanded = VariableRegex.Replace(
                    result, (m) => Expander(text, m)
                );
                if (expanded != result) {
                    needAnotherPass = true;
                    result = expanded;
                }
            }

            return result;
        }

        public string ExpandPath (string path, bool requireExists) {
            var result = Expand(path);

            if (result != null)
                result = result.Replace('/', System.IO.Path.DirectorySeparatorChar);

            if (!requireExists)
                return result;

            if (File.Exists(result) || Directory.Exists(result))
                return result;
            else
                throw new FileNotFoundException(result);
        }

        public VariableSet Clone () {
            return new VariableSet(this);
        }
    }

    public class VariableExpansionException : Exception {
        public VariableExpansionException (string text, VariableSet vs, string missingKey)
            : base (
                MakeExceptionMessage(text, vs, missingKey)
        ) {
        }

        private static string MakeExceptionMessage (string text, VariableSet vs, string missingKey) {
            return String.Format(
                "Failed to expand '{0}' because the variable '{1}' does not exist.\r\nValid variables at this location:\r\n{2}",
                text, missingKey,
                String.Join("\r\n", vs.DefinedVariables.OrderBy((v) => v))
            );
        }
    }
}
