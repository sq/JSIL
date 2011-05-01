using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public class JavascriptFormatter {
        public readonly TextWriter Output;
        public readonly PlainTextOutput PlainTextOutput;
        public readonly TextOutputFormatter PlainTextFormatter;

        public JavascriptFormatter (TextWriter output) {
            Output = output;
            PlainTextOutput = new PlainTextOutput(Output);
            PlainTextFormatter = new TextOutputFormatter(PlainTextOutput);
        }

        public void LPar () {
            PlainTextOutput.Write("(");
        }

        public void RPar () {
            PlainTextOutput.Write(")");
        }

        public void Space () {
            PlainTextOutput.Write(" ");
        }

        public void Comma () {
            PlainTextOutput.Write(", ");
        }

        public void CommaSeparatedList (IEnumerable<string> values, bool escape) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst)
                    Comma();

                if (escape)
                    PlainTextOutput.Write(Util.EscapeIdentifier(value));
                else
                    PlainTextOutput.Write(value);

                isFirst = false;
            }
        }

        public void CommaSeparatedList (IEnumerable<TypeReference> types) {
            bool isFirst = true;
            foreach (var type in types) {
                if (!isFirst)
                    Comma();

                Identifier(type as dynamic);

                isFirst = false;
            }
        }

        public void OpenBracket (bool indent = false) {
            if (indent)
                PlainTextOutput.Indent();
            PlainTextOutput.Write("[");
            if (indent)
                PlainTextOutput.WriteLine();
        }

        public void CloseBracket (bool indent = false) {
            if (indent)
                PlainTextOutput.Unindent();
            PlainTextOutput.Write("]");
            if (indent)
                PlainTextOutput.WriteLine();
        }

        public void OpenBrace () {
            PlainTextOutput.Indent();
            PlainTextOutput.WriteLine("{");
        }

        public void CloseBrace (bool semicolon = false) {
            PlainTextOutput.Unindent();
            if (semicolon)
                PlainTextOutput.WriteLine("};");
            else
                PlainTextOutput.WriteLine("}");
        }

        public void OpenFunction (IEnumerable<string> parameterNames) {
            PlainTextOutput.Write("function");
            Space();

            LPar();
            CommaSeparatedList(parameterNames, true);
            RPar();

            Space();
            OpenBrace();
        }

        public void OpenFunction (params string[] parameterNames) {
            OpenFunction((IEnumerable<string>)parameterNames);
        }

        public void Identifier (string name, bool escaped = false) {
            if (escaped)
                PlainTextOutput.Write(name);
            else
                PlainTextOutput.Write(Util.EscapeIdentifier(
                    name, true
                ));
        }

        public void Identifier (TypeReference type) {
            PlainTextOutput.Write(Util.EscapeIdentifier(
                type.FullName, false
            ));
        }

        public void Identifier (ArrayType type) {
            Identifier("System.Array.Of", true);
            LPar();
            Identifier(type.ElementType as dynamic);
            RPar();
        }

        public void Identifier (MethodReference method, TypeReference currentType = null) {
            if (method.HasThis && (method.DeclaringType == currentType)) {
                Identifier("this");
                Dot();
            } else {
                Identifier(method.DeclaringType);
                Dot();

                if (method.HasThis) {
                    Identifier("prototype");
                    Dot();
                }
            }

            Identifier(method.Name);
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

        public void Comment (string commentFormat, params object[] values) {
            var commentText = String.Format(commentFormat, values);
            PlainTextFormatter.WriteComment(
                commentText.Contains('\n') ? CommentType.MultiLine : CommentType.SingleLine, commentText
            );
        }
    }
}
