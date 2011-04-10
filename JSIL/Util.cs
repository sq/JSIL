using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JSIL.Internal {
    public static class Util {
        public static readonly HashSet<string> ReservedWords = new HashSet<string> {
            "break", "do", "instanceof", "typeof",
            "case", "else", "new", "var",
            "catch", "finally", "return", "void",
            "continue", "for", "switch", "while",
            "debugger", "function", "this", "with",
            "default", "if", "throw", "delete",
            "in", "try"
        };

        public static readonly Dictionary<string, string> IdentifierMappings = new Dictionary<string, string> {
            { "ToString", "toString" },
            { "Length", "length" }
        };

        public static Regex ValidIdentifier = new Regex("$[A-Za-z_$]([A-Za-z_$0-9]*)^", RegexOptions.Compiled);

        public static string EscapeIdentifier (string identifier, bool escapePeriods = true) {
            bool isReservedWord = ReservedWords.Contains(identifier);
            string result = identifier;

            if (!ValidIdentifier.IsMatch(identifier)) {
                var sb = new StringBuilder();
                for (int i = 0, l = identifier.Length; i < l; i++) {
                    var ch = identifier[i];

                    switch (ch) {
                        case '.':
                            if (escapePeriods)
                                sb.Append("_");
                            else
                                sb.Append(".");
                        break;
                        case '/':
                            if (escapePeriods)
                                sb.Append("_");
                            else
                                sb.Append(".");
                        break;
                        case '_':
                            sb.Append("$_");
                        break;
                        case '<':
                            sb.Append("$lt");
                        break;
                        case '>':
                            sb.Append("$gt");
                        break;
                        case '`':
                            sb.Append("$bt");
                        break;
                        default:
                            if ((ch <= 32) || (ch >= 127))
                                sb.AppendFormat("${0:x}", ch);
                            else
                                sb.Append(ch);
                        break;
                    }
                }

                result = sb.ToString();
            } else if (isReservedWord) {
                result = "cs$" + result;
            }

            string mapped;
            if (IdentifierMappings.TryGetValue(result, out mapped))
                result = mapped;

            return result;
        }
    }
}
