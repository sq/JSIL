using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Internal;
using Microsoft.CSharp.RuntimeBinder;
using Mono.Cecil;

namespace JSIL {
    class ILBlockTranslator {
        public readonly DecompilerContext Context;
        public readonly MethodDefinition ThisMethod;
        public readonly ILBlock Block;
        public readonly JavascriptFormatter Output;

        public ILBlockTranslator (ICSharpCode.Decompiler.DecompilerContext context, Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ILAst.ILBlock ilb, JavascriptFormatter output) {
            Context = context;
            ThisMethod = method;
            Block = ilb;
            Output = output;
        }

        public void Translate () {
            TranslateNode(Block);
        }

        public void TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node        NYI: {0}", node.GetType().Name);
        }

        protected void CommaSeparatedList (IEnumerable<ILExpression> values) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst)
                    Output.Comma();

                TranslateNode(value);
                isFirst = false;
            }
        }

        protected void Translate_BinOp (ILExpression node, string op) {
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.Space();
            Output.Token(op);
            Output.Space();
            TranslateNode(node.Arguments[1]);
            Output.RPar();
        }


        //
        // IL Node Types
        //

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

                Console.Error.WriteLine("Instruction NYI: {0} {1}", expression.Code, operandType);
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


        //
        // MSIL Instructions
        //

        protected void Translate_Clt (ILExpression node) {
            Translate_BinOp(node, "<");
        }

        protected void Translate_Mul (ILExpression node) {
            Translate_BinOp(node, "*");
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

        protected void Translate_Stloc (ILExpression node, ILVariable variable) {
            Output.Identifier(variable.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments.First());
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

        protected void Translate_Ldfld (ILExpression node, FieldDefinition field) {
            TranslateNode(node.Arguments.First());
            Output.Dot();
            Output.Identifier(field.Name);
        }

        protected void Translate_Stfld (ILExpression node, FieldDefinition field) {
            TranslateNode(node.Arguments.First());
            Output.Dot();
            Output.Identifier(field.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments[1]);
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

        protected void Translate_Newobj (ILExpression node, MethodReference constructor) {
            Output.Keyword("new");
            Output.Space();
            Output.Identifier(constructor.DeclaringType);

            Output.LPar();
            CommaSeparatedList(node.Arguments);
            Output.RPar();
        }

        protected void Translate_InitArray (ILExpression node, TypeReference elementType) {
            Output.Identifier("System.Array.New", true);
            Output.LPar();

            Output.Identifier(elementType);
            Output.Comma();
            Output.NewLine();

            Output.OpenBracket(true);
            CommaSeparatedList(node.Arguments);
            Output.CloseBracket(true);

            Output.RPar();
            Output.NewLine();
        }

        protected void Translate_Call (ILExpression node, MethodReference method) {
            IEnumerable<ILExpression> arguments = node.Arguments;

            Output.Identifier(method, true);

            if (method.HasThis) {
                // If the call is of the form this.Method(), we don't need to specify the this parameter explicitly
                if ((arguments.First().Code == ILCode.Ldloc) &&
                    (arguments.First().Operand is ILVariable) &&
                    (method.DeclaringType == ThisMethod.DeclaringType)
                ) {
                    arguments = arguments.Skip(1);

                } else {
                    Output.Dot();
                    Output.Identifier("call");
                }
            }

            Output.LPar();
            CommaSeparatedList(arguments);
            Output.RPar();
        }

        protected void Translate_Callvirt (ILExpression node, MethodReference method) {
            TranslateNode(node.Arguments[0]);
            Output.Dot();

            Output.Identifier(method, false);

            Output.LPar();
            CommaSeparatedList(node.Arguments.Skip(1));
            Output.RPar();
        }

        protected void Translate_CallGetter (ILExpression node, MethodReference getter) {
            Translate_Call(node, getter);
        }

        protected void Translate_CallSetter (ILExpression node, MethodReference setter) {
            Translate_Call(node, setter);
        }
    }
}
