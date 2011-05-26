using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Internal {
    public enum ListValueType {
        Primitive,
        Identifier,
        Raw
    }

    public class JavascriptFormatter {
        public readonly TextWriter Output;
        public readonly PlainTextOutput PlainTextOutput;
        public readonly TextOutputFormatter PlainTextFormatter;
        public readonly ITypeInfoSource TypeInfo;
        public readonly AssemblyDefinition Assembly;

        protected readonly HashSet<string> DeclaredNamespaces = new HashSet<string>();

        public JavascriptFormatter (TextWriter output, ITypeInfoSource typeInfo, AssemblyDefinition assembly) {
            Output = output;
            PlainTextOutput = new PlainTextOutput(Output);
            PlainTextFormatter = new TextOutputFormatter(PlainTextOutput);
            TypeInfo = typeInfo;
            Assembly = assembly;
        }

        public void LPar () {
            PlainTextOutput.Write("(");
            PlainTextOutput.Indent();
        }

        public void RPar () {
            PlainTextOutput.Unindent();
            PlainTextOutput.Write(")");
        }

        public void Space () {
            PlainTextOutput.Write(" ");
        }

        public void Comma () {
            PlainTextOutput.Write(", ");
        }

        public void CommaSeparatedList (IEnumerable<object> values, ListValueType valueType = ListValueType.Primitive) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst)
                    Comma();

                if (valueType == ListValueType.Primitive)
                    Value(value as dynamic);
                else if (valueType == ListValueType.Identifier)
                    Identifier(value as dynamic);
                else
                    PlainTextOutput.Write(value.ToString());

                isFirst = false;
            }
        }

        public void CommaSeparatedList<T, U> (
            IEnumerable<KeyValuePair<T, U>> pairs, 
            ListValueType keyType = ListValueType.Primitive,
            ListValueType valueType = ListValueType.Primitive
        ) {
            bool isFirst = true;
            foreach (var kvp in pairs) {
                if (!isFirst)
                    Comma();

                if (keyType == ListValueType.Primitive)
                    Value(kvp.Key as dynamic);
                else if (keyType == ListValueType.Identifier)
                    Identifier(kvp.Key as dynamic);
                else
                    PlainTextOutput.Write(kvp.Key.ToString());

                Token(": ");

                if (valueType == ListValueType.Primitive)
                    Value(kvp.Value as dynamic);
                else if (valueType == ListValueType.Identifier)
                    Identifier(kvp.Value as dynamic);
                else
                    PlainTextOutput.Write(kvp.Value.ToString());

                isFirst = false;
            }
        }

        public void CommaSeparatedList (IEnumerable<TypeReference> types) {
            bool isFirst = true;
            foreach (var type in types) {
                if (!isFirst)
                    Comma();

                Identifier(type, false);

                isFirst = false;
            }
        }

        public void OpenBracket (bool indent = false) {
            PlainTextOutput.Write("[");

            if (indent) {
                PlainTextOutput.Indent();
                PlainTextOutput.WriteLine();
            }
        }

        public void CloseBracket (bool indent = false) {
            if (indent) {
                PlainTextOutput.Unindent();
                PlainTextOutput.WriteLine();
            }

            PlainTextOutput.Write("]");
            if (indent)
                PlainTextOutput.WriteLine();
        }

        public void OpenBrace () {
            PlainTextOutput.WriteLine("{");
            PlainTextOutput.Indent();
        }

        public void CloseBrace (bool newLine = true) {
            PlainTextOutput.Unindent();
            if (newLine)
                PlainTextOutput.WriteLine("}");
            else
                PlainTextOutput.Write("}");
        }

        public void CloseAndReopenBrace (Action<JavascriptFormatter> midtext) {
            PlainTextOutput.Unindent();
            PlainTextOutput.Write("} ");
            midtext(this);
            PlainTextOutput.WriteLine(" {");
            PlainTextOutput.Indent();
        }

        public void CloseAndReopenBrace (string midtext) {
            PlainTextOutput.Unindent();
            PlainTextOutput.WriteLine(String.Format("}} {0} {{", midtext));
            PlainTextOutput.Indent();
        }

        public void OpenFunction (string functionName, Action<JavascriptFormatter> emitParameters) {
            PlainTextOutput.Write("function");
            Space();

            if (functionName != null) {
                PlainTextOutput.Write(Util.EscapeIdentifier(functionName));
                Space();
            }

            LPar();
            if (emitParameters != null)
                emitParameters(this);
            RPar();

            Space();
            OpenBrace();
        }

        public static string GetParent (TypeReference type) {
            var fullname = type.FullName;
            var index = fullname.LastIndexOfAny(new char[] { '.', '+', '/' });
            if (index < 0)
                return "JSIL.GlobalNamespace";
            else
                return fullname.Substring(0, index);
        }

        public void TypeReference (TypeReference type) {
            Value(Util.EscapeIdentifier(type.FullName, EscapingMode.String));
        }

        public void Identifier (string name, EscapingMode? escapingMode = EscapingMode.MemberIdentifier) {
            if (escapingMode.HasValue)
                PlainTextOutput.Write(Util.EscapeIdentifier(
                    name, escapingMode.Value
                ));
            else
                PlainTextOutput.Write(name);
        }

        public void Identifier (ILVariable variable) {
            if (variable.Type.IsByReference) {
                Identifier(variable.Name);
                Dot();
                Identifier("value");
            } else {
                Identifier(variable.Name);
            }
        }

        public void Identifier (TypeReference type, bool includeParens = false) {
            if (type.FullName == "JSIL.Proxy.AnyType")
                Identifier("System.Object", null);
            else
                TypeIdentifier(type as dynamic, includeParens);
        }

        protected void TypeIdentifier (TypeReference type, bool includeParens) {
            if (type.IsGenericParameter) {
                Comment(type.FullName);
                if (type.IsValueType)
                    Identifier("System.ValueType", null);
                else
                    Identifier("System.Object", null);
            } else {
                var typedef = type.Resolve();
                if ((typedef != null) && (typedef.Module.Assembly == Assembly) && !typedef.IsPublic)
                    PlainTextOutput.Write("$private.");

                PlainTextOutput.Write(Util.EscapeIdentifier(
                    type.FullName, EscapingMode.TypeIdentifier
                ));
            }
        }

        protected void TypeIdentifier (ByReferenceType type, bool includeParens) {
            if (includeParens)
                LPar();

            Identifier("JSIL.Reference.Of", null);
            LPar();
            Identifier(type.ElementType);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        protected void TypeIdentifier (ArrayType type, bool includeParens) {
            if (includeParens)
                LPar();

            Identifier("System.Array.Of", null);
            LPar();
            Identifier(type.ElementType);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        protected void TypeIdentifier (OptionalModifierType modopt, bool includeParens) {
            Identifier(modopt.ElementType as dynamic, includeParens);
        }

        protected void TypeIdentifier (RequiredModifierType modreq, bool includeParens) {
            Identifier(modreq.ElementType as dynamic, includeParens);
        }

        protected void TypeIdentifier (PointerType ptr, bool includeParens) {
            Identifier(ptr.ElementType as dynamic, includeParens);
        }

        protected void TypeIdentifier (GenericInstanceType type, bool includeParens) {
            Identifier(type.ElementType as dynamic, includeParens);
        }

        public void Identifier (MethodReference method, bool fullyQualified = true) {
            if (fullyQualified) {
                Identifier(method.DeclaringType);
                Dot();

                if (method.HasThis) {
                    Identifier("prototype");
                    Dot();
                }
            }

            var info = TypeInfo.GetMethod(method);
            Identifier(info.Name);
        }

        public void Keyword (string keyword) {
            PlainTextOutput.Write(keyword);
        }

        public void Token (string token) {
            PlainTextOutput.Write(token);
        }

        public void Dot () {
            PlainTextOutput.Write(".");
        }

        public void Semicolon () {
            PlainTextOutput.WriteLine(";");
        }

        public void NewLine () {
            PlainTextOutput.WriteLine();
        }

        public void Value (bool value) {
            if (value)
                PlainTextOutput.Write("true");
            else
                PlainTextOutput.Write("false");
        }

        public void Value (string value) {
            PlainTextOutput.Write(Util.EscapeString(value));
        }

        public void Value (long value) {
            PlainTextOutput.Write(value.ToString());
        }

        public void Value (ulong value) {
            PlainTextOutput.Write(value.ToString());
        }

        public void Value (double value) {
            PlainTextOutput.Write(value.ToString("R"));
        }

        public void Value (TypeReference type) {
            Value(GetNameOfType(type as dynamic));
        }

        protected static string GetNameOfType (TypeReference type) {
            return type.FullName;
        }

        protected static string GetNameOfType (ArrayType type) {
            return GetNameOfType(type.ElementType as dynamic) + "[]";
        }

        protected static string GetNameOfType (GenericInstanceType type) {
            return String.Format("{0}[{1}]",
                GetNameOfType(type.ElementType as dynamic),
                String.Join(", ", (from ga in type.GenericArguments
                                   select GetNameOfType(ga as dynamic)).ToArray())
            );
        }

        public void Comment (string commentFormat, params object[] values) {
            var commentText = String.Format(" " + commentFormat + " ", values);
            PlainTextFormatter.WriteComment(
                CommentType.MultiLine, commentText
            );
            PlainTextFormatter.Space();
        }

        public void DefaultValue (TypeReference typeReference) {
            string fullName = typeReference.FullName;

            if (TypeAnalysis.IsIntegerOrEnum(typeReference)) {
                Value(0);
                return;
            } else if (!typeReference.IsValueType) {
                Keyword("null");
                return;
            }

            switch (fullName) {
                case "System.Nullable`1":
                    Keyword("null");
                    return;
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    Value(0.0);
                    return;
            }

            Keyword("new");
            Space();
            Identifier(typeReference);
            LPar();
            RPar();
        }

        public void DeclareNamespace (string ns) {
            if (String.IsNullOrEmpty(ns))
                return;

            if (DeclaredNamespaces.Contains(ns))
                return;

            DeclaredNamespaces.Add(ns);

            var lastDot = ns.LastIndexOfAny(new char[] { '.', '/', '+', ':' });
            string parent;
            if (lastDot > 0) {
                parent = ns.Substring(0, lastDot);

                // Handle ::
                if (parent.EndsWith(":"))
                    parent = parent.Substring(0, parent.Length - 1);

                DeclareNamespace(parent);
            }

            Identifier("JSIL.DeclareNamespace", null);
            LPar();
            Value(Util.EscapeIdentifier(ns, EscapingMode.String));
            RPar();
            Semicolon();
        }

        public void Label (string labelName) {
            PlainTextFormatter.Unindent();
            Identifier(labelName);
            Token(": ");
            PlainTextFormatter.Indent();
            NewLine();
        }
    }
}
