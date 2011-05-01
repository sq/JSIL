using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.ILAst;
using Microsoft.CSharp.RuntimeBinder;
using Mono.Cecil;

namespace JSIL {
    class ILBlockTranslator {
        public readonly DecompilerContext Context;
        public readonly MethodDefinition Method;
        public readonly ILBlock Block;
        public readonly JavascriptFormatter Output;

        public ILBlockTranslator (ICSharpCode.Decompiler.DecompilerContext context, Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ILAst.ILBlock ilb, JavascriptFormatter output) {
            Context = context;
            Method = method;
            Block = ilb;
            Output = output;
        }

        public void Translate () {
            TranslateNode(Block);
        }

        public void TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node type not implemented: {0}", node.GetType().Name);
        }

        public void TranslateNode (ILBlock block) {
            foreach (var node in block.GetChildren()) {
                TranslateNode(node as dynamic);
                Output.Semicolon();
            }
        }

        public void TranslateNode (ILExpression expression) {
            var methodName = String.Format("Translate_{0}", expression.Code);
            try {
                object[] arguments;
                if (expression.Operand != null)
                    arguments = new object[] { expression, expression.Operand };
                else
                    arguments = new object[] { expression };

                GetType().InvokeMember(
                    methodName, System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.InvokeMethod |
                        System.Reflection.BindingFlags.NonPublic,
                    null, this, arguments
                );
            } catch (MissingMethodException) {
                string operandType = "";
                if (expression.Operand != null)
                    operandType = expression.Operand.GetType().FullName;

                Console.Error.WriteLine("Instruction not implemented: {0} {1}", expression.Code, operandType);
            }
        }

        public void TranslateNode (ILCondition condition) {
            Output.Keyword("if");
            Output.Space();
            Output.LPar();
            TranslateNode(condition.Condition);
            Output.RPar();
            Output.OpenBrace();
            Output.CloseBrace();
        }

        protected void Translate_Ret (ILExpression node) {
            Output.Keyword("return");

            if (node.Arguments.Count == 1) {
                Output.Space();
                TranslateNode(node.Arguments.First());
            }
        }

        protected void Translate_Ldloc (ILExpression node, ILVariable variable) {
            Output.Identifier(variable.Name);
        }

        protected void Translate_Ldsfld (ILExpression node, FieldDefinition field) {
            Output.Identifier(field.DeclaringType);
            Output.Dot();
            Output.Identifier(field.Name);
        }

        protected void Translate_Stsfld (ILExpression node, FieldDefinition field) {
            Output.Identifier(field.DeclaringType);
            Output.Dot();
            Output.Identifier(field.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments.First());
        }

        protected void Translate_Stloc (ILExpression node, ILVariable variable) {
            Output.Identifier(variable.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments.First());
        }

        protected void Translate_Ldstr (ILExpression node, string text) {
            Output.Value(text);
        }

        protected void Translate_Ldc_I4 (ILExpression node, Int32 value) {
            Output.Value(value);
        }

        protected void Translate_Box (ILExpression node, TypeReference valueType) {
            // We could do boxing the strict way, but in practice, I don't think it's necessary...
            /*
            Output.Keyword("new");
            Output.Space();
            Output.Identifier(node.Operand as dynamic);
            Output.LPar();
            TranslateNode(node.Arguments.First());
            Output.RPar();
             */

            TranslateNode(node.Arguments.First());
        }

        protected void Translate_Call (ILExpression node, MethodReference method) {
            Output.Identifier(method, Method.DeclaringType);
            if (method.HasThis) {
                Output.Dot();
                Output.Identifier("call");
            }

            Output.LPar();

            bool isFirst = true;
            foreach (var argument in node.Arguments) {
                if (!isFirst)
                    Output.Comma();

                TranslateNode(argument);
                isFirst = false;
            }

            Output.RPar();
        }
    }
}
