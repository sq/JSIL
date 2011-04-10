using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cecil.Decompiler;
using Cecil.Decompiler.Ast;
using Cecil.Decompiler.Languages;
using Mono.Cecil;

namespace JSIL.Internal {
    public class JavaScriptWriter : CSharpWriter {
        private readonly Stack<MethodDefinition> MethodStack = new Stack<MethodDefinition>();

        public JavaScriptWriter (ILanguage language, IFormatter formatter)
            : base (language, formatter) {
        }

        new protected void WriteIdentifier (string name, object identifier) {
            base.WriteIdentifier(Util.EscapeIdentifier(name), identifier);
        }

        protected void WriteIdentifier (MethodReference method) {
            base.WriteIdentifier(
                Util.EscapeIdentifier(method.Name, Util.EscapeIdentifier(method.DeclaringType.FullName)), 
                method
            );
        }

        protected void WriteComment (string text) {
            base.WriteLiteral("/* " + text + " */");
        }

        protected void WriteParameters (MethodDefinition method) {
            for (int i = 0, c = method.Parameters.Count; i < c; i++) {
                var parameter = method.Parameters[i];

                if (i != 0) {
                    base.WriteToken(",");
                    base.WriteSpace();
                }

                WriteComment(parameter.ParameterType.Name);
                WriteSpace();

                WriteReference(parameter.Name, parameter);
            }
        }

        public override void VisitMethodReferenceExpression (MethodReferenceExpression node) {
            var target = node.Target;
            var methodName = node.Method.Name;

            if (node.Method.DeclaringType != MethodStack.Peek().DeclaringType) {
            } else {
            }

            if (target != null) {
                this.Visit(target);
                base.WriteToken(".");
            } else if (!node.Method.HasThis) {
                this.WriteReference(node.Method.DeclaringType.ToString(), node.Method.DeclaringType);
                base.WriteToken(".");
            }

            WriteIdentifier(node.Method);
        }

        public override void Write (MethodDefinition method) {
            MethodStack.Push(method);

            base.WriteKeyword("function");
            base.WriteSpace();
            WriteIdentifier(method);
            base.WriteToken("(");
            WriteParameters(method);
            base.WriteToken(")");
            base.WriteLine();

            this.Write(method.Body.Decompile(base.language));

            MethodStack.Pop();
        }
    }
}
