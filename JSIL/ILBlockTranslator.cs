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
        public readonly JavascriptFormatter Output = null;
        public readonly ITypeInfoSource TypeInfo;

        public ILBlockTranslator (DecompilerContext context, MethodDefinition method, ILBlock ilb, ITypeInfoSource typeInfo) {
            Context = context;
            ThisMethod = method;
            Block = ilb;
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

        public JSExpression[] Translate (IEnumerable<ILExpression> values) {
            return (from v in values select TranslateNode(v)).ToArray();
        }

        public JSVariable[] Translate (IEnumerable<ParameterDefinition> parameters) {
            return (
                from p in parameters select new JSVariable(p.Name, p.ParameterType)
            ).ToArray();
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

        protected JSUnaryOperatorExpression Translate_UnaryOp (ILExpression node, JSUnaryOperator op) {
            return new JSUnaryOperatorExpression(
                op,
                TranslateNode(node.Arguments[0])
            );
        }

        protected JSBinaryOperatorExpression Translate_BinaryOp (ILExpression node, JSBinaryOperator op) {
            return new JSBinaryOperatorExpression(
                op,
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1])
            );
        }

        protected JSExpression Translate_EqualityComparison (ILExpression node, bool checkEqual) {
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
                    return Translate_UnaryOp(node.Arguments[0], JSOperator.LogicalNot);
                else
                    return TranslateNode(node.Arguments[0]);

            } else {
                return Translate_BinaryOp(node, checkEqual ? JSOperator.Equal : JSOperator.NotEqual);
            }
        }

        protected JSFunctionExpression EmitLambda (MethodDefinition method) {
            return AssemblyTranslator.TranslateMethod(Context, method, TypeInfo);
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
                    Debug.WriteLine(String.Format("Instruction {0} did not produce a JS AST expression", expression));
            } catch (MissingMethodException) {
                string operandType = "";
                if (expression.Operand != null)
                    operandType = expression.Operand.GetType().FullName;

                Console.Error.WriteLine("Instruction NYI: {0} {1}", expression.Code, operandType);
                return new JSInvocationExpression(
                    JSDotExpression.New(new JSIdentifier("JSIL"), "UntranslatableInstruction"),
                    String.IsNullOrWhiteSpace(operandType) ?
                        new JSExpression[] { 
                            new JSStringLiteral(expression.Code.ToString())
                        } :
                        new JSExpression[] { 
                            new JSStringLiteral(expression.Code.ToString()), 
                            new JSStringLiteral(operandType)
                        }
                );
                Output.RPar();
            }

            return result;
        }

        public JSIfStatement TranslateNode (ILCondition condition) {
            JSStatement falseBlock = null;
            if ((condition.FalseBlock != null) && (condition.FalseBlock.Body.Count > 0))
                falseBlock = TranslateNode(condition.FalseBlock);

            return new JSIfStatement(
                TranslateNode(condition.Condition),
                TranslateNode(condition.TrueBlock),
                falseBlock
            );
        }

        public JSStatement TranslateNode (ILTryCatchBlock tcb) {
            var body = TranslateNode(tcb.TryBlock);
            JSVariable catchVariable = null;
            JSBlockStatement catchBlock = null;
            JSBlockStatement finallyBlock = null;

            if (tcb.FaultBlock != null)
                throw new NotImplementedException();

            if (tcb.CatchBlocks.Count > 0) {
                var pairs = new List<KeyValuePair<JSExpression, JSStatement>>();
                catchVariable = new JSVariable("$exception", Context.CurrentModule.TypeSystem.Object);

                bool isFirst = true, foundUniversalCatch = false, openBrace = false;
                foreach (var cb in tcb.CatchBlocks) {
                    JSExpression pairCondition = null;

                    if (cb.ExceptionType.FullName == "System.Object") {
                        Console.Error.WriteLine("Ignoring impossible catch clause");
                        continue;
                    } else if (cb.ExceptionType.FullName == "System.Exception") {
                        foundUniversalCatch = true;
                    } else {
                        if (foundUniversalCatch)
                            throw new NotImplementedException("Catch-all clause must be last");

                        pairCondition = new JSInvocationExpression(
                            new JSDotExpression(new JSIdentifier("JSIL"), "CheckType"),
                            catchVariable, new JSType(cb.ExceptionType)
                        );
                    }

                    var pairBody = TranslateBlock(cb.Body);

                    if (cb.ExceptionVariable != null)
                        pairBody.Statements.Insert(
                            0, new JSExpressionStatement(new JSBinaryOperatorExpression(
                                JSOperator.Assignment, new JSVariable(cb.ExceptionVariable.Name, cb.ExceptionVariable.Type), catchVariable
                            ))
                        );

                    pairs.Add(new KeyValuePair<JSExpression, JSStatement>(
                        pairCondition, pairBody
                    ));
                }

                if (!foundUniversalCatch)
                    pairs.Add(new KeyValuePair<JSExpression,JSStatement>(
                        null, new JSExpressionStatement(new JSThrowExpression(catchVariable))
                    ));

                catchBlock = new JSBlockStatement(
                    JSIfStatement.New(pairs.ToArray())
                );
            }

            if (tcb.FinallyBlock != null)
                finallyBlock = TranslateNode(tcb.FinallyBlock);

            return new JSTryCatchBlock(
                body, catchVariable, catchBlock, finallyBlock
            );
        }

        public JSWhileLoop TranslateNode (ILWhileLoop loop) {
            JSExpression condition;
            if (loop.Condition != null)
                condition = TranslateNode(loop.Condition);
            else
                condition = JSLiteral.New(true);

            return new JSWhileLoop(
                condition,
                TranslateNode(loop.BodyBlock)
            );
        }


        //
        // MSIL Instructions
        //

        protected JSBinaryOperatorExpression Translate_Clt (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.LessThan);
        }

        protected JSExpression Translate_Cgt (ILExpression node) {
            if (
                (!node.Arguments[0].ExpectedType.IsValueType) &&
                (!node.Arguments[1].ExpectedType.IsValueType) &&
                (node.Arguments[0].ExpectedType == node.Arguments[1].ExpectedType) &&
                (node.Arguments[0].Code == ILCode.Isinst)
            ) {
                // The C# expression 'x is y' translates into roughly '(x is y) > null' in IL, 
                //  because there's no IL opcode for != and the IL isinst opcode returns object, not bool
                return new JSInvocationExpression(
                    new JSDotExpression(new JSIdentifier("JSIL"), "CheckType"),
                    TranslateNode(node.Arguments[1]),
                    new JSType((TypeReference)node.Arguments[0].Operand)
                );
            } else {
                return Translate_BinaryOp(node, JSOperator.GreaterThan);
            }
        }

        protected JSExpression Translate_Ceq (ILExpression node) {
            return Translate_EqualityComparison(node, true);
        }

        protected JSBinaryOperatorExpression Translate_Mul (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Multiply);
        }

        protected JSBinaryOperatorExpression Translate_Div (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Divide);
        }

        protected JSBinaryOperatorExpression Translate_Add (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Add);
        }

        protected JSBinaryOperatorExpression Translate_Sub (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Subtract);
        }

        protected JSBinaryOperatorExpression Translate_Shl (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.ShiftLeft);
        }

        protected JSBinaryOperatorExpression Translate_Shr (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.ShiftRight);
        }

        protected JSBinaryOperatorExpression Translate_And (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.BitwiseAnd);
        }

        protected JSExpression Translate_LogicNot (ILExpression node) {
            var arg = node.Arguments[0];

            switch (arg.Code) {
                case ILCode.Ceq:
                    return Translate_EqualityComparison(arg, false);
                case ILCode.Clt:
                case ILCode.Clt_Un:
                    return Translate_BinaryOp(arg, JSOperator.GreaterThanOrEqual);
                case ILCode.Cgt:
                case ILCode.Cgt_Un:
                    return Translate_BinaryOp(arg, JSOperator.LessThanOrEqual);
            }

            return Translate_UnaryOp(node, JSOperator.LogicalNot);
        }

        protected JSUnaryOperatorExpression Translate_Neg (ILExpression node) {
            return Translate_UnaryOp(node, "-");
        }

        protected JSThrowExpression Translate_Throw (ILExpression node) {
            return new JSThrowExpression(TranslateNode(node.Arguments[0]));
        }

        protected JSExpression Translate_Endfinally (ILExpression node) {
            return JSExpression.Null;
        }

        protected void Translate_LoopOrSwitchBreak (ILExpression node) {
            Output.Keyword("break");
        }

        protected JSReturnExpression Translate_Ret (ILExpression node) {
            if (node.Arguments.FirstOrDefault() != null) {
                return new JSReturnExpression(
                    TranslateNode(node.Arguments[0])
                );
            } else if (node.Arguments.Count == 0) {
                return new JSReturnExpression();
            } else {
                throw new NotImplementedException();
            }
        }

        protected JSIdentifier Translate_Ldloc (ILExpression node, ILVariable variable) {
            return new JSVariable(variable.Name, variable.Type);
        }

        protected JSReferenceExpression Translate_Ldloca (ILExpression node, ILVariable variable) {
            return new JSReferenceExpression(Translate_Ldloc(node, variable));
        }

        protected JSBinaryOperatorExpression Translate_Stloc (ILExpression node, ILVariable variable) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSVariable(variable.Name, variable.Type),
                TranslateNode(node.Arguments[0])
            );
        }

        protected JSDotExpression Translate_Ldsfld (ILExpression node, FieldReference field) {
            return new JSDotExpression(new JSType(field.DeclaringType), field.Name);
        }

        protected JSReferenceExpression Translate_Ldsflda (ILExpression node, FieldReference field) {
            return new JSReferenceExpression(Translate_Ldsfld(node, field));
        }

        protected JSBinaryOperatorExpression Translate_Stsfld (ILExpression node, FieldReference field) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSDotExpression(new JSType(field.DeclaringType), field.Name),
                TranslateNode(node.Arguments[0])
            );
        }

        protected JSDotExpression Translate_Ldfld (ILExpression node, FieldReference field) {
            return new JSDotExpression(
                TranslateNode(node.Arguments[0]), 
                field.Name
            );
        }

        protected JSBinaryOperatorExpression Translate_Stfld (ILExpression node, FieldReference field) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSDotExpression(TranslateNode(node.Arguments[0]), field.Name),
                TranslateNode(node.Arguments[1])
            );
        }

        protected JSReferenceExpression Translate_Ldflda (ILExpression node, FieldReference field) {
            return new JSReferenceExpression(Translate_Ldfld(node, field));
        }

        protected JSExpression Translate_Ldobj (ILExpression node, TypeReference type) {
            return TranslateNode(node.Arguments[0]);
        }

        protected JSExpression Translate_Stobj (ILExpression node, TypeReference type) {
            return Translate_BinaryOp(
                node, JSOperator.Assignment
            );
        }

        protected JSStringLiteral Translate_Ldstr (ILExpression node, string text) {
            return JSLiteral.New(text);
        }

        protected JSExpression Translate_Ldnull (ILExpression node) {
            return JSLiteral.Null();
        }

        protected JSExpression Translate_Ldftn (ILExpression node, MethodReference method) {
            var mdef = method.Resolve();

            if ((mdef != null) && mdef.IsCompilerGenerated()) {
                return EmitLambda(mdef);
            } else {
                if (method.HasThis)
                    throw new NotImplementedException();
                else
                    return new JSDotExpression(
                        new JSType(method.DeclaringType),
                        method.Name
                    );
            }
        }

        protected JSExpression Translate_Ldc (ILExpression node, long value) {
            TypeInfo typeInfo = null;
            if (node.ExpectedType != null)
                typeInfo = TypeInfo.Get(node.ExpectedType);

            if ((typeInfo != null) && (typeInfo.EnumMembers.Count > 0)) {
                EnumMemberInfo em;

                if (typeInfo.ValueToEnumMember.TryGetValue(value, out em))
                    return new JSEnumLiteral(em);
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

        protected JSDotExpression Translate_Ldlen (ILExpression node) {
            return new JSDotExpression(
                TranslateNode(node.Arguments[0]),
                "Length"
            );
        }

        protected JSIndexerExpression Translate_Ldelem (ILExpression node) {
            return new JSIndexerExpression(
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1])
            );
        }

        protected JSBinaryOperatorExpression Translate_Stelem (ILExpression node) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSIndexerExpression(
                    TranslateNode(node.Arguments[0]),
                    TranslateNode(node.Arguments[1])
                ),
                TranslateNode(node.Arguments[2])
            );
        }

        protected JSInvocationExpression Translate_NullCoalescing (ILExpression node) {
            return new JSInvocationExpression(
                JSDotExpression.New(new JSIdentifier("JSIL"), "Coalesce"),
                Translate(node.Arguments)
            );
        }

        protected JSExpression Translate_Castclass (ILExpression node, TypeReference targetType) {
            if (IsDelegateType(targetType) && IsDelegateType(node.ExpectedType)) {
                // TODO: We treat all delegate types as equivalent, so we can skip these casts for now
                return TranslateNode(node.Arguments[0]);
            }

            return new JSInvocationExpression(
                new JSDotExpression(new JSIdentifier("JSIL"), "Cast"),
                TranslateNode(node.Arguments[0]),
                new JSType(targetType)
            );
        }

        protected JSInvocationExpression Translate_Isinst (ILExpression node, TypeReference targetType) {
            return new JSInvocationExpression(
                new JSDotExpression(new JSIdentifier("JSIL"), "TryCast"),
                TranslateNode(node.Arguments[0]),
                new JSType(targetType)
            );
        }

        protected JSExpression Translate_Unbox_Any (ILExpression node, TypeReference targetType) {
            return Translate_Castclass(node, targetType);
        }

        protected JSInvocationExpression Translate_Conv (ILExpression node, TypeReference targetType) {
            return new JSInvocationExpression(
                new JSDotExpression(new JSIdentifier("JSIL"), "Cast"),
                TranslateNode(node.Arguments[0]),
                new JSType(targetType)
            );
        }

        protected JSInvocationExpression Translate_Conv_U4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.UInt32);
        }

        protected JSInvocationExpression Translate_Conv_U8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.UInt64);
        }

        protected JSInvocationExpression Translate_Conv_I4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int32);
        }

        protected JSInvocationExpression Translate_Conv_I8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int64);
        }

        protected JSInvocationExpression Translate_Conv_R4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Single);
        }

        protected JSInvocationExpression Translate_Conv_R8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Double);
        }

        protected JSExpression Translate_Box (ILExpression node, TypeReference valueType) {
            // TODO: We could do boxing the strict way, but in practice, I don't think it's necessary...
            /*
            Output.Keyword("new");
            Output.Space();
            Output.Identifier(node.Operand as dynamic);
            Output.LPar();
            TranslateNode(node.Arguments[0]);
            Output.RPar();
             */

            return TranslateNode(node.Arguments[0]);
        }

        protected JSExpression Translate_Newobj (ILExpression node, MethodReference constructor) {
            if (IsDelegateType(constructor.DeclaringType)) {
                return new JSInvocationExpression(
                    JSDotExpression.New(new JSIdentifier("System"), "Delegate", "New"),
                    (new JSExpression[] { 
                        JSLiteral.New(constructor.DeclaringType) 
                    }).Concat(
                        Translate(node.Arguments)
                    ).ToArray()
                );
            } else if (constructor.DeclaringType.IsArray) {
                
                return new JSInvocationExpression(
                    JSDotExpression.New(new JSIdentifier("JSIL"), "MultidimensionalArray", "New"),
                    (new JSExpression[] { 
                        new JSType(constructor.DeclaringType.GetElementType())
                    }).Concat(
                        Translate(node.Arguments)
                    ).ToArray()
                );
            }

            return new JSNewExpression(
                constructor.DeclaringType,
                Translate(node.Arguments)
            );
        }

        protected JSInvocationExpression Translate_Newarr (ILExpression node, TypeReference elementType) {
            return new JSInvocationExpression(
                JSDotExpression.New(new JSIdentifier("System"), "Array", "New"),
                new JSType(elementType),
                TranslateNode(node.Arguments[0])
            );
        }

        protected JSInvocationExpression Translate_InitArray (ILExpression node, TypeReference elementType) {
            return new JSInvocationExpression(
                JSDotExpression.New(new JSIdentifier("System"), "Array", "New"),
                new JSType(elementType),
                new JSArrayExpression(Translate(node.Arguments))
            );
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
                Translate(arguments)
            );
        }

        protected JSExpression Translate_Callvirt (ILExpression node, MethodReference method) {
            return new JSInvocationExpression(
                new JSDotExpression(
                    TranslateNode(node.Arguments[0]),
                    method.Name
                ),
                Translate(node.Arguments.Skip(1))
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

        protected JSUnaryOperatorExpression Translate_PostIncrement (ILExpression node, int arg) {
            if (Math.Abs(arg) != 1)
                throw new NotImplementedException("No idea what this means...");

            if (arg == 1)
                return new JSUnaryOperatorExpression(
                    JSOperator.Increment, TranslateNode(node.Arguments[0]), true
                );
            else
                return new JSUnaryOperatorExpression(
                    JSOperator.Decrement, TranslateNode(node.Arguments[0]), true
                );
        }
    }
}
