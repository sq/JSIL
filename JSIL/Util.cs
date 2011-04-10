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

        public static Regex ValidIdentifier = new Regex("$[A-Za-z_$]([A-Za-z_$0-9]*)^");

        public static string EscapeIdentifier (string identifier, string declaringType = null) {
            bool isReservedWord = ReservedWords.Contains(identifier);

            if (ValidIdentifier.IsMatch(identifier) && !isReservedWord)
                return identifier;

            if (isReservedWord)
                return "cs$" + identifier;

            if (declaringType == null)
                declaringType = "";
            else
                declaringType = "_" + declaringType;

            switch (identifier) {
                case ".ctor":
                    return String.Format("{0}_ctor_", declaringType);
                case ".cctor":
                    return String.Format("{0}_static_ctor_", declaringType);
            }

            var result = new StringBuilder();
            for (int i = 0, l = identifier.Length; i < l; i++) {
                var ch = identifier[i];

                switch (ch) {
                    case '.':
                        result.Append("_");
                        break;
                    case '_':
                        result.Append("$_");
                        break;
                    default:
                        result.Append(ch);
                        break;
                }
            }

            return result.ToString();
        }
    }
}
