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

        public JavascriptFormatter (TextWriter output, ITypeInfoSource typeInfo) {
            Output = output;
            PlainTextOutput = new PlainTextOutput(Output);
            PlainTextFormatter = new TextOutputFormatter(PlainTextOutput);
            TypeInfo = typeInfo;
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

        public void CloseAndReopenBrace (Action<JavascriptFormatter> midtext) {
            PlainTextOutput.Unindent();
            PlainTextOutput.Write("} ");
            midtext(this);
            PlainTextOutput.WriteLine(" }");
            PlainTextOutput.Indent();
        }

        public void CloseAndReopenBrace (string midtext) {
            PlainTextOutput.Unindent();
            PlainTextOutput.WriteLine(String.Format("}} {0} {{", midtext));
            PlainTextOutput.Indent();
        }

        public void OpenFunction (IEnumerable<string> parameterNames) {
            PlainTextOutput.Write("function");
            Space();

            LPar();
            CommaSeparatedList(parameterNames, ListValueType.Identifier);
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
            TypeIdentifier(type as dynamic);
        }

        protected void TypeIdentifier (TypeReference type) {
            PlainTextOutput.Write(Util.EscapeIdentifier(
                type.FullName, false
            ));
        }

        protected void TypeIdentifier (ArrayType type) {
            Identifier("System.Array.Of", true);
            LPar();
            Identifier(type.ElementType);
            RPar();
        }

        protected void TypeIdentifier (GenericInstanceType type) {
            Identifier(type.ElementType);
            Dot();
            Identifier("Of");
            LPar();
            CommaSeparatedList(type.GenericArguments);
            RPar();
        }

        public void Identifier (MethodReference method, bool fullyQualified = true) {
            string methodName = method.Name;

            // TODO: This doesn't work because it breaks Console.WriteLine :(
            /*
            var mdef = method.Resolve();
            if (mdef != null) {
                MethodGroupItem mgi;
                if (TypeInfo.Get(method.DeclaringType).MethodToMethodGroupItem.TryGetValue(mdef, out mgi))
                    methodName = mgi.MangledName;
            }
             */

            if (fullyQualified) {
                Identifier(method.DeclaringType);
                Dot();

                if (method.HasThis) {
                    Identifier("prototype");
                    Dot();
                }
            }

            Identifier(methodName);
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
                CommentType.MultiLine, commentText
            );
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
    }
}
