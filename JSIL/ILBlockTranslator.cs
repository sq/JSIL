using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public readonly ITypeInfoSource TypeInfo;

        public ILBlockTranslator (DecompilerContext context, MethodDefinition method, ILBlock ilb, JavascriptFormatter output, ITypeInfoSource typeInfo) {
            Context = context;
            ThisMethod = method;
            Block = ilb;
            Output = output;
            TypeInfo = typeInfo;
        }

        public void Translate () {
            TranslateNode(Block);
        }

        public void TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node        NYI: {0}", node.GetType().Name);

            Output.Token("JSIL.UntranslatableNode");
            Output.LPar();
            Output.Value(node.GetType().Name);
            Output.RPar();
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

        protected void Translate_UnaryOp (ILExpression node, string op) {
            Output.LPar();
            Output.Token(op);
            TranslateNode(node.Arguments[0]);
            Output.RPar();
        }

        protected void Translate_BinaryOp (ILExpression node, string op) {
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.Space();
            Output.Token(op);
            Output.Space();
            TranslateNode(node.Arguments[1]);
            Output.RPar();
        }

        protected void EmitLambda (MethodDefinition method) {
            Output.OpenFunction(from p in method.Parameters select p.Name);

            AssemblyTranslator.TranslateMethodBody(Context, Output, method, TypeInfo);

            Output.CloseBrace();
        }

        protected static bool IsDelegateType (TypeReference type) {
            var typedef = type.Resolve();
            if (
                (typedef != null) && (typedef.BaseType != null) &&
                (
                    (typedef.BaseType.FullName == "System.Delegate") ||
                    (typedef.BaseType.FullName == "System.MulticastDelegate")
                )
            ) {
                return true;
            }

            return false;
        }


        //
        // IL Node Types
        //

        protected void TranslateBlock (IEnumerable<ILNode> children) {
            foreach (var node in children) {
                TranslateNode(node as dynamic);

                if (node is ILExpression)
                    Output.Semicolon();
            }
        }

        public void TranslateNode (ILBlock block) {
            TranslateBlock(block.GetChildren());
        }

        public void TranslateNode (ILExpression expression) {
            var methodName = String.Format("Translate_{0}", expression.Code);
            var bindingFlags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.InvokeMethod |
                        System.Reflection.BindingFlags.NonPublic;

            try {
                object[] arguments;
                if (expression.Operand != null)
                    arguments = new object[] { expression, expression.Operand };
                else
                    arguments = new object[] { expression };

                var t = GetType();

                if (t.GetMember(methodName, bindingFlags).Length == 0) {
                    var newMethodName = methodName.Substring(0, methodName.LastIndexOf("_"));
                    if (t.GetMember(newMethodName, bindingFlags).Length != 0)
                        methodName = newMethodName;
                }

                GetType().InvokeMember(
                    methodName, bindingFlags,
                    null, this, arguments
                );
            } catch (MissingMethodException) {
                string operandType = "";
                if (expression.Operand != null)
                    operandType = expression.Operand.GetType().FullName;

                Console.Error.WriteLine("Instruction NYI: {0} {1}", expression.Code, operandType);
                Output.Token("JSIL.UntranslatableInstruction");
                Output.LPar();
                Output.Value(expression.Code.ToString());
                if (operandType.Length > 0) {
                    Output.Comma();
                    Output.Value(operandType);
                }
                Output.RPar();
            }
        }

        public void TranslateNode (ILCondition condition) {
            Output.Keyword("if");
            Output.Space();
            Output.LPar();
            TranslateNode(condition.Condition);
            Output.RPar();
            Output.Space();

            Output.OpenBrace();
            TranslateNode(condition.TrueBlock);

            if ((condition.FalseBlock != null) && (condition.FalseBlock.Body.Count > 0)) {
                Output.CloseAndReopenBrace("else");
                TranslateNode(condition.FalseBlock);
            }

            Output.CloseBrace();
        }

        public void TranslateNode (ILTryCatchBlock tcb) {
            Output.Keyword("try");
            Output.Space();

            Output.OpenBrace();
            TranslateNode(tcb.TryBlock);

            if (tcb.FaultBlock != null) {
                throw new NotImplementedException();
            }

            if (tcb.CatchBlocks.Count > 0) {
                Output.CloseAndReopenBrace(String.Format("catch ($exception)"));

                bool isFirst = true, foundUniversalCatch = false, openBrace = false;
                foreach (var cb in tcb.CatchBlocks) {
                    if (cb.ExceptionType.FullName == "System.Object") {
                        Console.Error.WriteLine("Ignoring impossible catch clause");
                        Output.Comment("Impossible catch clause ignored");
                        continue;
                    } else if (cb.ExceptionType.FullName == "System.Exception") {
                        foundUniversalCatch = true;

                        if (!isFirst)
                            Output.CloseAndReopenBrace("else");

                    } else {
                        if (foundUniversalCatch)
                            throw new NotImplementedException("Catch-all clause must be last");

                        if (isFirst) {
                            Output.Keyword("if");
                            Output.Space();
                            Output.LPar();

                            Output.Identifier("JSIL.CheckType", true);
                            Output.LPar();
                            Output.Identifier("$exception");
                            Output.Comma();
                            Output.Identifier(cb.ExceptionType);
                            Output.RPar();

                            Output.RPar();
                            Output.Space();
                            openBrace = true;
                            Output.OpenBrace();
                        } else {
                            var excType = cb.ExceptionType;

                            Output.CloseAndReopenBrace(
                                (o) => {
                                    o.Keyword("else if");
                                    o.Space();
                                    o.LPar();

                                    o.Identifier("JSIL.CheckType", true);
                                    o.LPar();
                                    o.Identifier("$exception");
                                    o.Comma();
                                    o.Identifier(excType);
                                    o.RPar();

                                    o.RPar();
                                }
                            );
                        }
                    }

                    if (cb.ExceptionVariable != null) {
                        Output.Identifier(cb.ExceptionVariable.Name);
                        Output.Token(" = ");
                        Output.Identifier("$exception");
                        Output.Semicolon();
                    }

                    TranslateBlock(cb.Body);

                    isFirst = false;
                }

                if (!foundUniversalCatch) {
                    Output.CloseAndReopenBrace("else");
                    Output.Keyword("throw");
                    Output.Space();
                    Output.Identifier("$exception");
                    Output.Semicolon();
                }

                if (openBrace)
                    Output.CloseBrace();
            }

            if (tcb.FinallyBlock != null) {
                Output.CloseAndReopenBrace("finally");
                TranslateNode(tcb.FinallyBlock);
            }

            Output.CloseBrace();
        }

        public void TranslateNode (ILWhileLoop loop) {
            Output.Keyword("while");
            Output.Space();
            Output.LPar();

            if (loop.Condition != null)
                TranslateNode(loop.Condition);
            else
                Output.Keyword("true");

            Output.RPar();
            Output.Space();

            Output.OpenBrace();
            TranslateNode(loop.BodyBlock);
            Output.CloseBrace();
        }


        //
        // MSIL Instructions
        //

        protected void Translate_Clt (ILExpression node) {
            Translate_BinaryOp(node, "<");
        }

        protected void Translate_Ceq (ILExpression node) {
            Translate_BinaryOp(node, "===");
        }

        protected void Translate_Mul (ILExpression node) {
            Translate_BinaryOp(node, "*");
        }

        protected void Translate_Div (ILExpression node) {
            Translate_BinaryOp(node, "/");
        }

        protected void Translate_Add (ILExpression node) {
            Translate_BinaryOp(node, "+");
        }

        protected void Translate_LogicNot (ILExpression node) {
            if (node.Arguments[0].Code == ILCode.Ceq) {
                Translate_BinaryOp(node.Arguments[0], "!==");
            } else {
                Translate_UnaryOp(node, "!");
            }
        }

        protected void Translate_Neg (ILExpression node) {
            Translate_UnaryOp(node, "-");
        }

        protected void Translate_Throw (ILExpression node) {
            Output.Keyword("throw");
            Output.Space();
            TranslateNode(node.Arguments[0]);
        }

        protected void Translate_Endfinally (ILExpression node) {
            Output.Comment("Endfinally");
        }

        protected void Translate_LoopOrSwitchBreak (ILExpression node) {
            Output.Keyword("break");
        }

        protected void Translate_Ret (ILExpression node) {
            Output.Keyword("return");

            if (node.Arguments.Count == 1) {
                Output.Space();
                TranslateNode(node.Arguments[0]);
            }
        }

        protected void Translate_Ldloc (ILExpression node, ILVariable variable) {
            Output.Identifier(variable);
        }

        protected void Translate_Ldloca (ILExpression node, ILVariable variable) {
            Translate_Ldloc(node, variable);
        }

        protected void Translate_Stloc (ILExpression node, ILVariable variable) {
            Output.Identifier(variable);
            Output.Token(" = ");
            TranslateNode(node.Arguments[0]);
        }

        protected void Translate_Ldsfld (ILExpression node, FieldReference field) {
            Output.Identifier(field.DeclaringType);
            Output.Dot();
            Output.Identifier(field.Name);
        }

        protected void Translate_Ldsflda (ILExpression node, FieldReference field) {
            Translate_Ldsfld(node, field);
        }

        protected void Translate_Stsfld (ILExpression node, FieldReference field) {
            Output.Identifier(field.DeclaringType);
            Output.Dot();
            Output.Identifier(field.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments[0]);
        }

        protected void Translate_Ldfld (ILExpression node, FieldReference field) {
            TranslateNode(node.Arguments[0]);
            Output.Dot();
            Output.Identifier(field.Name);
        }

        protected void Translate_Ldflda (ILExpression node, FieldReference field) {
            Output.Keyword("new");
            Output.Space();
            Output.Identifier("JSIL.MemberReference", true);
            Output.LPar();

            TranslateNode(node.Arguments[0]);
            Output.Comma();
            Output.Value(Util.EscapeIdentifier(field.Name));

            Output.RPar();
        }

        protected void Translate_Ldobj (ILExpression node, TypeReference type) {
            TranslateNode(node.Arguments[0]);
        }

        protected void Translate_Stobj (ILExpression node, TypeReference type) {
            TranslateNode(node.Arguments[0]);
            Output.Token(" = ");
            TranslateNode(node.Arguments[1]);
        }

        protected void Translate_Stfld (ILExpression node, FieldReference field) {
            TranslateNode(node.Arguments[0]);
            Output.Dot();
            Output.Identifier(field.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments[1]);
        }

        protected void Translate_Ldstr (ILExpression node, string text) {
            Output.Value(text);
        }

        protected void Translate_Ldnull (ILExpression node) {
            Output.Keyword("null");
        }

        protected void Translate_Ldftn (ILExpression node, MethodReference method) {
            var mdef = method.Resolve();

            if ((mdef != null) && mdef.IsCompilerGenerated()) {
                EmitLambda(mdef);
            } else {
                Output.Identifier(method, true);
            }
        }

        protected void Translate_Ldc (ILExpression node, long value) {
            TypeInfo typeInfo = null;
            if (node.ExpectedType != null)
                typeInfo = TypeInfo.Get(node.ExpectedType);

            if ((typeInfo != null) && (typeInfo.EnumMembers.Count > 0)) {
                EnumMemberInfo em;

                if (typeInfo.ValueToEnumMember.TryGetValue(value, out em))
                    Output.Identifier(Util.EscapeIdentifier(em.FullName, false), true);
                else
                    Output.Value(value);
            } else {
                Output.Value(value);
            }
        }

        protected void Translate_Ldc (ILExpression node, ulong value) {
            Output.Value(value);
        }

        protected void Translate_Ldc (ILExpression node, double value) {
            Output.Value(value);
        }

        protected void Translate_Ldc (ILExpression node, decimal value) {
            Output.Value((double)value);
        }

        protected void Translate_Ldlen (ILExpression node) {
            TranslateNode(node.Arguments[0]);
            Output.Dot();
            Output.Identifier("length");
        }

        protected void Translate_Ldelem (ILExpression node) {
            TranslateNode(node.Arguments[0]);
            Output.OpenBracket();
            TranslateNode(node.Arguments[1]);
            Output.CloseBracket();
        }

        protected void Translate_Stelem (ILExpression node) {
            TranslateNode(node.Arguments[0]);
            Output.OpenBracket();
            TranslateNode(node.Arguments[1]);
            Output.CloseBracket();
            Output.Token(" = ");
            TranslateNode(node.Arguments[2]);
        }

        protected void Translate_NullCoalescing (ILExpression node) {
            Output.Identifier("JSIL.Coalesce", true);
            Output.LPar();
            CommaSeparatedList(node.Arguments);
            Output.RPar();
        }

        protected void Translate_Castclass (ILExpression node, TypeReference targetType) {
            if (IsDelegateType(targetType) && IsDelegateType(node.ExpectedType)) {
                // TODO: We treat all delegate types as equivalent, so we can skip these casts for now
                TranslateNode(node.Arguments[0]);
                return;
            }

            Output.Identifier("JSIL.Cast", true);
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.Comma();
            Output.Identifier(targetType);
            Output.RPar();
        }

        protected void Translate_Isinst (ILExpression node, TypeReference targetType) {
            Output.Identifier("JSIL.TryCast", true);
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.Comma();
            Output.Identifier(targetType);
            Output.RPar();
        }

        protected void Translate_Unbox_Any (ILExpression node, TypeReference targetType) {
            Translate_Castclass(node, targetType);
        }

        protected void Translate_Conv (ILExpression node, string typeName) {
            Output.Identifier("JSIL.Cast", true);
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.Comma();
            Output.Identifier(typeName, true);
            Output.RPar();
        }

        protected void Translate_Conv_I4 (ILExpression node) {
            Translate_Conv(node, "System.Int32");
        }

        protected void Translate_Box (ILExpression node, TypeReference valueType) {
            // TODO: We could do boxing the strict way, but in practice, I don't think it's necessary...
            /*
            Output.Keyword("new");
            Output.Space();
            Output.Identifier(node.Operand as dynamic);
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.RPar();
             */

            TranslateNode(node.Arguments[0]);
        }

        protected void Translate_Newobj (ILExpression node, MethodReference constructor) {
            if (IsDelegateType(constructor.DeclaringType)) {
                Output.Identifier("System.Delegate.New", true);
                Output.LPar();
                Output.Value(constructor.DeclaringType);
                Output.Comma();
            } else {
                Output.Keyword("new");
                Output.Space();
                Output.Identifier(constructor.DeclaringType);
                Output.LPar();
            }

            CommaSeparatedList(node.Arguments);
            Output.RPar();
        }

        protected void Translate_Newarr (ILExpression node, TypeReference elementType) {
            Output.Identifier("System.Array.New", true);
            Output.LPar();

            Output.Identifier(elementType);
            Output.Comma();

            CommaSeparatedList(node.Arguments);

            Output.RPar();
        }

        protected void Translate_InitArray (ILExpression node, TypeReference elementType) {
            Output.Identifier("System.Array.New", true);
            Output.LPar();

            Output.Identifier(elementType);
            Output.Comma();

            Output.OpenBracket();
            CommaSeparatedList(node.Arguments);
            Output.CloseBracket();

            Output.RPar();
        }

        protected void Translate_Call (ILExpression node, MethodReference method) {
            // This translates the MSIL equivalent of 'typeof(T)' into a direct reference to the specified type
            if (method.FullName == "System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)") {
                var tr = node.Arguments[0].Operand as TypeReference;
                if (tr != null) {
                    Output.Identifier((TypeReference)node.Arguments[0].Operand);
                    return;
                } else {
                    Console.Error.WriteLine("Unrecognized typeof expression");
                }
            }

            IEnumerable<ILExpression> arguments = node.Arguments;

            if (method.HasThis) {
                // If the call is of the form this.Method(), we don't need to specify the this parameter explicitly
                if ((arguments.First().Code == ILCode.Ldloc) &&
                    (arguments.First().Operand is ILVariable) &&
                    (method.DeclaringType == ThisMethod.DeclaringType)
                ) {
                    arguments = arguments.Skip(1);
                    Output.Keyword("this");
                    Output.Dot();
                    Output.Identifier(method, false);

                } else {
                    Output.Identifier(method, true);
                    Output.Dot();
                    Output.Identifier("call");
                }
            } else {
                Output.Identifier(method, true);
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

        protected void Translate_CallvirtGetter (ILExpression node, MethodReference getter) {
            Translate_Callvirt(node, getter);
        }

        protected void Translate_CallvirtSetter (ILExpression node, MethodReference setter) {
            Translate_Callvirt(node, setter);
        }

        protected void Translate_PostIncrement (ILExpression node, int arg) {
            if (Math.Abs(arg) != 1)
                throw new NotImplementedException("No idea what this means...");

            TranslateNode(node.Arguments[0]);
            if (arg == 1)
                Output.Token("++");
            else
                Output.Token("--");
        }
    }
}
