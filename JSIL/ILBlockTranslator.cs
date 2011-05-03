using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
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

        public JSBlockStatement Translate () {
            return TranslateNode(Block);
        }

        public JSNode TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node        NYI: {0}", node.GetType().Name);

            Output.Token("JSIL.UntranslatableNode");
            Output.LPar();
            Output.Value(node.GetType().Name);
            Output.RPar();

            return null;
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

        protected JSBinaryOperatorExpression Translate_BinaryOp (ILExpression node, JSBinaryOperator op) {
            return new JSBinaryOperatorExpression(
                op,
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1])
            );
        }

        protected void Translate_EqualityComparison (ILExpression node, bool checkEqual) {
            if (
                (node.Arguments[0].ExpectedType.FullName == "System.Boolean") &&
                (node.Arguments[1].ExpectedType.FullName == "System.Boolean") &&
                (node.Arguments[1].Code.ToString().Contains("Ldc_"))
            ) {
                // Comparison against boolean constant
                bool comparand = Convert.ToInt64(node.Arguments[1].Operand) != 0;

                // TODO: This produces '!(x > y)' when 'x <= y' would be preferable.
                //  This should be easy to fix once javascript output is done via AST construction.
                if (comparand != checkEqual)
                    Output.Token("!");

                TranslateNode(node.Arguments[0]);
            } else {
                Translate_BinaryOp(node, checkEqual ? "===" : "!==");
            }
        }

        protected JSFunctionExpression EmitLambda (MethodDefinition method) {
            var body = AssemblyTranslator.TranslateMethodBody(Context, Output, method, TypeInfo);
            return new JSFunctionExpression(
                (from p in method.Parameters select new JSVariable(p.Name, p.ParameterType)).ToArray(),
                body
            );
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

        protected JSBlockStatement TranslateBlock (IEnumerable<ILNode> children) {
            var nodes = new List<JSStatement>();

            foreach (var node in children) {
                var translated = TranslateStatement(node);

                if (translated != null)
                    nodes.Add(translated);
            }

            return new JSBlockStatement(nodes.ToArray());
        }

        protected JSStatement TranslateStatement (ILNode node) {
            var translated = TranslateNode(node as dynamic);

            var statement = translated as JSStatement;
            if (statement == null) {
                var expression = (JSExpression)translated;

                if (expression != null)
                    statement = new JSExpressionStatement(expression);
                else
                    Debug.WriteLine("Warning: Null statement");
            }

            return statement;
        }

        public JSBlockStatement TranslateNode (ILBlock block) {
            return TranslateBlock(block.GetChildren());
        }

        public JSExpression TranslateNode (ILExpression expression) {
            JSExpression result = null;

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

                var invokeResult = GetType().InvokeMember(
                    methodName, bindingFlags,
                    null, this, arguments
                );
                result = invokeResult as JSExpression;

                if (result == null)
                    Debug.WriteLine(String.Format("Instruction {0} did not produce a JS AST node", expression.Code));
            } catch (MissingMethodException) {
                string operandType = "";
                if (expression.Operand != null)
                    operandType = expression.Operand.GetType().FullName;

                Console.Error.WriteLine("Instruction NYI: {0} {1}", expression.Code, operandType);
                return new JSInvocationExpression(
                    JSDotExpression.New(new JSIdentifier("JSIL"), "UntranslatableInstruction"),
                    new JSStringLiteral(expression.Code.ToString()), 
                    new JSStringLiteral(operandType)
                );
                Output.RPar();
            }

            return result;
        }

        public JSIfStatement TranslateNode (ILCondition condition) {
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

            return null;
        }

        public JSStatement TranslateNode (ILTryCatchBlock tcb) {
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

            return null;
        }

        public JSStatement TranslateNode (ILWhileLoop loop) {
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

            return null;
        }


        //
        // MSIL Instructions
        //

        protected void Translate_Clt (ILExpression node) {
            Translate_BinaryOp(node, "<");
        }

        protected void Translate_Cgt (ILExpression node) {
            if (
                (!node.Arguments[0].ExpectedType.IsValueType) &&
                (!node.Arguments[1].ExpectedType.IsValueType) &&
                (node.Arguments[0].ExpectedType == node.Arguments[1].ExpectedType) &&
                (node.Arguments[0].Code == ILCode.Isinst)
            ) {
                // The C# expression 'x is y' translates into roughly '(x is y) > null' in IL, 
                //  because there's no IL opcode for != and the IL isinst opcode returns object, not bool
                Output.Identifier("JSIL.CheckType", true);
                Output.LPar();
                TranslateNode(node.Arguments[1]);
                Output.Comma();
                Output.Identifier((TypeReference) node.Arguments[0].Operand);
                Output.RPar();
            } else {
                Translate_BinaryOp(node, ">");
            }
        }

        protected void Translate_Ceq (ILExpression node) {
            Translate_EqualityComparison(node, true);
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

        protected void Translate_Sub (ILExpression node) {
            Translate_BinaryOp(node, "-");
        }

        protected void Translate_Shl (ILExpression node) {
            Translate_BinaryOp(node, "<<");
        }

        protected void Translate_Shr (ILExpression node) {
            Translate_BinaryOp(node, ">>");
        }

        protected void Translate_And (ILExpression node) {
            Translate_BinaryOp(node, "&");
        }

        protected void Translate_LogicNot (ILExpression node) {
            var arg = node.Arguments[0];

            switch (arg.Code) {
                case ILCode.Ceq:
                    Translate_EqualityComparison(arg, false);
                    return;
                case ILCode.Clt:
                case ILCode.Clt_Un:
                    Translate_BinaryOp(arg, ">=");
                    return;
                case ILCode.Cgt:
                case ILCode.Cgt_Un:
                    Translate_BinaryOp(arg, "<=");
                    return;
                default:
                    break;
            }

            Translate_UnaryOp(node, "!");
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

        protected JSIdentifier Translate_Ldloc (ILExpression node, ILVariable variable) {
            return new JSVariable(variable.Name, variable.Type);
        }

        protected JSIdentifier Translate_Ldloca (ILExpression node, ILVariable variable) {
            return Translate_Ldloc(node, variable);
        }

        protected JSBinaryOperatorExpression Translate_Stloc (ILExpression node, ILVariable variable) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSVariable(variable.Name, variable.Type),
                TranslateNode(node.Arguments[0])
            );
        }

        protected void Translate_Ldsfld (ILExpression node, FieldReference field) {
            Output.Identifier(field.DeclaringType);
            Output.Dot();
            Output.Identifier(field.Name);
        }

        protected void Translate_Ldsflda (ILExpression node, FieldReference field) {
            Output.Keyword("new");
            Output.Space();
            Output.Identifier("JSIL.MemberReference", true);
            Output.LPar();

            Output.Identifier(field.DeclaringType);
            Output.Comma();
            Output.Value(Util.EscapeIdentifier(field.Name));

            Output.RPar();
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

        protected JSExpression Translate_Ldobj (ILExpression node, TypeReference type) {
            return TranslateNode(node.Arguments[0]);
        }

        protected JSExpression Translate_Stobj (ILExpression node, TypeReference type) {
            return Translate_BinaryOp(
                node, JSOperator.Assignment
            );
        }

        protected void Translate_Stfld (ILExpression node, FieldReference field) {
            TranslateNode(node.Arguments[0]);
            Output.Dot();
            Output.Identifier(field.Name);
            Output.Token(" = ");
            TranslateNode(node.Arguments[1]);
        }

        protected JSStringLiteral Translate_Ldstr (ILExpression node, string text) {
            return JSLiteral.New(text);
        }

        protected JSExpression Translate_Ldnull (ILExpression node) {
            return JSLiteral.Null<object>();
        }

        protected JSExpression Translate_Ldftn (ILExpression node, MethodReference method) {
            var mdef = method.Resolve();

            if ((mdef != null) && mdef.IsCompilerGenerated()) {
                return EmitLambda(mdef);
            } else {
                return new JSIdentifier(method.FullName);
            }
        }

        protected JSExpression Translate_Ldc (ILExpression node, long value) {
            TypeInfo typeInfo = null;
            if (node.ExpectedType != null)
                typeInfo = TypeInfo.Get(node.ExpectedType);

            if ((typeInfo != null) && (typeInfo.EnumMembers.Count > 0)) {
                EnumMemberInfo em;

                if (typeInfo.ValueToEnumMember.TryGetValue(value, out em))
                    return new JSIdentifier(Util.EscapeIdentifier(em.FullName, false));
                else
                    return JSLiteral.New(value);
            } else if (node.ExpectedType.FullName == "System.Boolean") {
                return JSLiteral.New(value != 0);
            } else {
                return JSLiteral.New(value);
            }
        }

        protected JSExpression Translate_Ldc (ILExpression node, ulong value) {
            return JSLiteral.New(value);
        }

        protected JSExpression Translate_Ldc (ILExpression node, double value) {
            return JSLiteral.New(value);
        }

        protected JSExpression Translate_Ldc (ILExpression node, decimal value) {
            return JSLiteral.New(value);
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

        protected JSExpression Translate_Newobj (ILExpression node, MethodReference constructor) {
            if (IsDelegateType(constructor.DeclaringType)) {
                return new JSInvocationExpression(
                    JSDotExpression.New(new JSIdentifier("System"), "Delegate", "New"),
                    new JSType(constructor.DeclaringType)
                );
            } else if (constructor.DeclaringType.IsArray) {
                return new JSInvocationExpression(
                    JSDotExpression.New(new JSIdentifier("JSIL"), "MultidimensionalArray", "New"),
                    new JSType(constructor.DeclaringType.GetElementType())
                );
            }

            return new JSNewExpression(
                constructor.DeclaringType,
                (from a in node.Arguments select TranslateNode(a)).ToArray()
            );
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

        protected JSExpression Translate_Call (ILExpression node, MethodReference method) {
            // This translates the MSIL equivalent of 'typeof(T)' into a direct reference to the specified type
            if (method.FullName == "System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)") {
                var tr = node.Arguments[0].Operand as TypeReference;
                if (tr != null) {
                    return new JSType(
                        (TypeReference)node.Arguments[0].Operand
                    );
                } else {
                    Console.Error.WriteLine("Unrecognized typeof expression");
                }
            }

            IEnumerable<ILExpression> arguments = node.Arguments;
            JSExpression thisExpression;
            JSIdentifier methodName;

            if (method.HasThis) {
                var firstArg = arguments.First();
                var ilv = firstArg.Operand as ILVariable;
                
                // Methods sometimes get 'this' passed as a ref, primarily when they are struct methods.
                // Use a cheap hack to make them look like the same type.
                var firstArgType = firstArg.ExpectedType.FullName;
                if (firstArgType.EndsWith("&"))
                    firstArgType = firstArgType.Substring(0, firstArgType.Length - 1);

                // If the call is of the form x.Method(...), we don't need to specify the this parameter
                //  explicitly using the form type.Method.call(x, ...).
                // Make sure that 'this' references only pass this check when they don't refer to 
                //  members of base types/interfaces.
                if (
                    (method.DeclaringType.FullName == firstArgType) &&
                    (
                        (ilv == null) || (ilv.Name != "this") ||
                        (firstArg.ExpectedType.FullName == ThisMethod.DeclaringType.FullName)
                    )
                ) {
                    thisExpression = TranslateNode(firstArg);
                    methodName = new JSIdentifier(method.Name);
                    arguments = arguments.Skip(1);
                } else {
                    thisExpression = new JSDotExpression(
                        new JSType(method.DeclaringType),
                        "prototype"
                    );
                    methodName = new JSIdentifier(method.Name);
                    // Output.Identifier("call");
                }
            } else {
                thisExpression = new JSType(method.DeclaringType);
                methodName = new JSIdentifier(method.Name);
            }

            return new JSInvocationExpression(
                new JSDotExpression(thisExpression, methodName), 
                (from a in arguments select TranslateNode(a)).ToArray()
            );
        }

        protected JSExpression Translate_Callvirt (ILExpression node, MethodReference method) {
            return new JSInvocationExpression(
                new JSDotExpression(
                    TranslateNode(node.Arguments[0]),
                    method.Name
                ),
                (from a in node.Arguments.Skip(1) select TranslateNode(a)).ToArray()
            );
        }

        protected JSExpression Translate_CallGetter (ILExpression node, MethodReference getter) {
            return Translate_Call(node, getter);
        }

        protected JSExpression Translate_CallSetter (ILExpression node, MethodReference setter) {
            return Translate_Call(node, setter);
        }

        protected JSExpression Translate_CallvirtGetter (ILExpression node, MethodReference getter) {
            return Translate_Callvirt(node, getter);
        }

        protected JSExpression Translate_CallvirtSetter (ILExpression node, MethodReference setter) {
            return Translate_Callvirt(node, setter);
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
