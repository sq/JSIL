using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

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
                        case '<':
                            sb.Append("$lt");
                        break;
                        case '>':
                            sb.Append("$gt");
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
                        case '`':
                            sb.Append("$bt");
                        break;
                        case '@':
                            sb.Append("$at");
                        break;
                        case '-':
                            sb.Append("_");
                        break;
                        case '=':
                            sb.Append("$eq");
                        break;
                        case ' ':
                            sb.Append("$sp");
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

        public static string EscapeCharacter (char character) {
            switch (character) {
                case '\'':
                    return @"\'";
                case '"':
                    return "\\\"";
                case '\t':
                    return @"\t";
                case '\r':
                    return @"\r";
                case '\n':
                    return @"\n";
                default:
                    return String.Format(@"\x{0:x}", (int)character);
            }
        }

        public static string EscapeString (string text, char? quoteCharacter = null) {
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
                else if ((ch < ' ') || (ch > 127))
                    sb.Append(EscapeCharacter(ch));
                else
                    sb.Append(ch);
            }

            sb.Append(quoteCharacter.Value);

            return sb.ToString();
        }

        public static string GetFullName (this MethodDeclaration methodDeclaration) {
            var pdt = methodDeclaration.PrivateImplementationType.Annotation<TypeReference>();

            if (pdt == null)
                return methodDeclaration.Name;
            else
                return String.Format("{0}.{1}", pdt.FullName, methodDeclaration.Name);
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
