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

        protected void WriteIdentifier (string name, object identifier) {
            Formatter.WriteRaw(Util.EscapeIdentifier(name));
        }

        protected void WriteIdentifier (TypeReference type) {
            Formatter.WriteRaw(Util.EscapeIdentifier(
                type.FullName,
                escapePeriods: false
            ));
        }

        protected void WriteIdentifier (MethodReference method) {
            WriteIdentifier(
                Util.EscapeIdentifier(
                    method.Name, 
                    declaringType: Util.EscapeIdentifier(method.DeclaringType.FullName)
                ), 
                method
            );
        }

        protected void WriteIdentifier (VariableReference variable) {
            var variableName = variable.Name;
            if (String.IsNullOrEmpty(variableName))
                variableName = String.Format("$v{0}", variable.Index);

            WriteIdentifier(
                variableName, variable
            );
        }

        protected void WriteComment (string text) {
            Formatter.WriteComment(String.Format("/* {0} */", text.Replace("*/", "")));
            Formatter.WriteSpace();
        }

        protected void WriteParameters (MethodDefinition method) {
            for (int i = 0, c = method.Parameters.Count; i < c; i++) {
                var parameter = method.Parameters[i];

                if (i != 0) {
                    Formatter.WriteOperator(",");
                    Formatter.WriteSpace();
                }

                WriteComment(parameter.ParameterType.FullName);

                Formatter.WriteRaw(parameter.Name);
            }
        }

        public override void VisitVariableDeclarationExpression (VariableDeclarationExpression node) {
            Formatter.WriteRaw("var");
            Formatter.WriteSpace();
            WriteIdentifier(node.Variable);
        }

        public override void VisitVariableReferenceExpression (VariableReferenceExpression node) {
            WriteIdentifier(node.Variable);
        }

        public override void VisitMethodReferenceExpression (MethodReferenceExpression node) {
            var target = node.Target;

            if (target != null) {
                Visit(target);
                Formatter.WriteOperator(".");
            } else if (!node.Method.HasThis) {
                WriteIdentifier(node.Method.DeclaringType);
                Formatter.WriteOperator(".");
            }

            WriteIdentifier(node.Method);
        }

        public override void VisitCastExpression (CastExpression node) {
            WriteComment(String.Format("({0})", node.TargetType.FullName));
            Visit(node.Expression);
        }

        public override void Write (MethodDefinition method) {
            MethodStack.Push(method);

            Formatter.WriteRaw("function");
            Formatter.WriteSpace();
            WriteIdentifier(method);
            Formatter.WriteOperator("(");
            WriteParameters(method);
            Formatter.WriteOperator(")");
            Formatter.WriteLine();

            Write(method.Body.Decompile(Language));

            MethodStack.Pop();
        }
    }
}
