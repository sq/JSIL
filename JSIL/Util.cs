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

        public static Regex ValidIdentifier = new Regex("$[A-Za-z_$]([A-Za-z_$0-9]*)^", RegexOptions.Compiled);

        public static string EscapeIdentifier (string identifier, bool escapePeriods = true) {
            bool isReservedWord = ReservedWords.Contains(identifier);

            if (ValidIdentifier.IsMatch(identifier) && !isReservedWord)
                return identifier;

            if (isReservedWord)
                return "cs$" + identifier;

            var result = new StringBuilder();
            for (int i = 0, l = identifier.Length; i < l; i++) {
                var ch = identifier[i];

                switch (ch) {
                    case '.':
                        if (escapePeriods)
                            result.Append("_");
                        else
                            result.Append(".");
                        break;
                    case '_':
                        result.Append("$_");
                        break;
                    default:
                        if ((ch <= 32) || (ch >= 127))
                            result.AppendFormat("${0:x}", ch);
                        else
                            result.Append(ch);
                        break;
                }
            }

            return result.ToString();
        }
    }
}
