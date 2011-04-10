using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cecil.Decompiler;
using Cecil.Decompiler.Ast;
using Cecil.Decompiler.Languages;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace JSIL.Internal {
    public class JavaScriptWriter : CSharpWriter {
        private readonly Stack<TypeDefinition> TypeStack = new Stack<TypeDefinition>();
        private readonly Stack<MethodDefinition> MethodStack = new Stack<MethodDefinition>();

        public JavaScriptWriter (ILanguage language, IFormatter formatter)
            : base (language, formatter) {
        }

        public void PushType (TypeDefinition type) {
            TypeStack.Push(type);
        }

        public void PopType (TypeDefinition type) {
            if (type != TypeStack.Peek())
                throw new InvalidDataException();

            TypeStack.Pop();
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

        protected void WriteIdentifier (VariableReference variable) {
            var variableName = variable.Name;
            if (String.IsNullOrEmpty(variableName))
                variableName = String.Format("$v{0}", variable.Index);

            base.WriteIdentifier(
                variableName, variable
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

        public override void VisitVariableDeclarationExpression (VariableDeclarationExpression node) {
            WriteKeyword("var");
            WriteSpace();
            WriteIdentifier(node.Variable);
        }

        public override void VisitVariableReferenceExpression (VariableReferenceExpression node) {
            WriteIdentifier(node.Variable);
        }

        public override void VisitMethodReferenceExpression (MethodReferenceExpression node) {
            var target = node.Target;
            var methodName = node.Method.Name;

            if (node.Method.DeclaringType != MethodStack.Peek().DeclaringType) {
            } else {
            }

            if (target != null) {
                Visit(target);
                WriteToken(".");
            } else if (!node.Method.HasThis) {
                WriteReference(node.Method.DeclaringType.ToString(), node.Method.DeclaringType);
                WriteToken(".");
            }

            WriteIdentifier(node.Method);
        }

        public override void Write (MethodDefinition method) {
            MethodStack.Push(method);

            WriteKeyword("function");
            WriteSpace();
            WriteIdentifier(method);
            WriteToken("(");
            WriteParameters(method);
            WriteToken(")");
            WriteLine();

            Write(method.Body.Decompile(base.language));

            MethodStack.Pop();
        }
    }
}
