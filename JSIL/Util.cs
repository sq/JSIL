using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace JSIL.Internal {
    public enum EscapingMode {
        MemberIdentifier,
        TypeIdentifier,
        String
    }

    public static class Util {
        public static readonly HashSet<string> ReservedWords = new HashSet<string> {
            "break", "do", "instanceof", "typeof",
            "case", "else", "new", "var",
            "catch", "finally", "return", "void",
            "continue", "for", "switch", "while",
            "debugger", "function", "this", "with",
            "default", "if", "throw", "delete",
            "in", "try", "import", "class", "enum",
            "export", "extends", "super", "let",
            "package", "interface", "implements", "private",
            "protected", "public", "static", "yield",
            "const", "true", "false", "null"
        };

        public static Regex ValidIdentifier = new Regex("$[A-Za-z_$]([A-Za-z_$0-9]*)^", RegexOptions.Compiled);

        public static string EscapeIdentifier (string identifier, EscapingMode escapingMode = EscapingMode.MemberIdentifier) {
            string result = identifier;

            if (!ValidIdentifier.IsMatch(identifier)) {
                var sb = new StringBuilder();
                for (int i = 0, l = identifier.Length; i < l; i++) {
                    var ch = identifier[i];

                    switch (ch) {
                        case '.':
                            if (escapingMode != EscapingMode.MemberIdentifier)
                                sb.Append(".");
                            else
                                sb.Append("_");
                        break;
                        case '/':
                            if (escapingMode == EscapingMode.MemberIdentifier)
                                sb.Append("_");
                            else if (escapingMode == EscapingMode.TypeIdentifier)
                                sb.Append(".");
                            else
                                sb.Append("/");
                        break;
                        case '+':
                            if (escapingMode == EscapingMode.MemberIdentifier)
                                sb.Append("_");
                            else if (escapingMode == EscapingMode.TypeIdentifier)
                                sb.Append(".");
                            else
                                sb.Append("+");
                        break;
                        case '`':
                            sb.Append("$b");
                        break;
                        case '~':
                            sb.Append("$t");
                        break;
                        case ':':
                            sb.Append("$c");
                        break;
                        case '<':
                            sb.Append("$l");
                        break;
                        case '>':
                            sb.Append("$g");
                        break;
                        case '(':
                            sb.Append("$lp");
                        break;
                        case ')':
                            sb.Append("$rp");
                        break;
                        case '{':
                            sb.Append("$lc");
                        break;
                        case '}':
                            sb.Append("$rc");
                        break;
                        case '[':
                            sb.Append("$lb");
                        break;
                        case ']':
                            sb.Append("$rb");
                        break;
                        case '@':
                            sb.Append("$at");
                        break;
                        case '-':
                            sb.Append("$da");
                        break;
                        case '=':
                            sb.Append("$eq");
                        break;
                        case ' ':
                            sb.Append("$sp");
                        break;
                        case '?':
                            sb.Append("$qu");
                        break;
                        case '!':
                            sb.Append("$ex");
                        break;
                        case '*':
                            sb.Append("$as");
                        break;
                        case '&':
                            sb.Append("$am");
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
            }

            bool isReservedWord = ReservedWords.Contains(result);
            if (isReservedWord)
                result = "$" + result;

            return result;
        }

        public static string EscapeCharacter (char character) {
            switch (character) {
                case '\0':
                    return @"\0";
                case '\'':
                    return @"\'";
                case '\\':
                    return @"\\";
                case '"':
                    return "\\\"";
                case '\t':
                    return @"\t";
                case '\r':
                    return @"\r";
                case '\n':
                    return @"\n";
                default: {
                    if (character > 255)
                        return String.Format(@"\x{0:x4}", (int)character);
                    else
                        return String.Format(@"\x{0:x2}", (int)character);
                }
            }
        }

        public static string EscapeString (string text, char? quoteCharacter = null) {
            if (text == null)
                return "null";

            bool containsSingle = text.Contains('\'');
            bool containsDouble = text.Contains('"');

            if (quoteCharacter == null) {
                if (containsDouble && !containsSingle)
                    quoteCharacter = '\'';
                else
                    quoteCharacter = '"';
            }

            var sb = new StringBuilder();

            sb.Append(quoteCharacter.Value);

            foreach (var ch in text) {
                if (ch == quoteCharacter.Value)
                    sb.Append(EscapeCharacter(ch));
                else if (ch == '\\')
                    sb.Append(@"\\");
                else if ((ch < ' ') || (ch > 127))
                    sb.Append(EscapeCharacter(ch));
                else
                    sb.Append(ch);
            }

            sb.Append(quoteCharacter.Value);

            return sb.ToString();
        }

        public sealed class ListSkipAdapter<T> : IList<T> {
            public readonly IList<T> List;
            public readonly int Offset;

            public ListSkipAdapter (IList<T> list, int offset) {
                List = list;
                Offset = offset;
            }

            public int IndexOf (T item) {
                throw new NotImplementedException();
            }

            public void Insert (int index, T item) {
                List.Insert(index + Offset, item);
            }

            public void RemoveAt (int index) {
                List.RemoveAt(index + Offset);
            }

            public T this[int index] {
                get {
                    return List[index + Offset];
                }
                set {
                    List[index + Offset] = value;
                }
            }

            public void Add (T item) {
                List.Add(item);
            }

            public void Clear () {
                throw new NotImplementedException();
            }

            public bool Contains (T item) {
                throw new NotImplementedException();
            }

            public void CopyTo (T[] array, int arrayIndex) {
                for (int i = 0, c = Count; i < c; i++)
                    array[i + arrayIndex] = List[i + Offset];
            }

            public int Count {
                get { return List.Count - Offset; }
            }

            public bool IsReadOnly {
                get { return List.IsReadOnly; }
            }

            public bool Remove (T item) {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator () {
                return (List as IEnumerable<T>).Skip(Offset).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
                return (List as IEnumerable<T>).Skip(Offset).GetEnumerator();
            }
        }

        public static IList<T> Skip<T> (this IList<T> list, int offset) {
            return new ListSkipAdapter<T>(list, offset);
        }

        public static string Indent (object inner) {
            if (inner == null)
                return "";

            var text = inner.ToString();

            return String.Join(
                Environment.NewLine,
                (from l in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                 select "    " + l).ToArray()
            );
        }
    }
}
