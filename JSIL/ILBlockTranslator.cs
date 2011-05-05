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

        public readonly Dictionary<string, JSVariable> Variables = new Dictionary<string, JSVariable>();

        public readonly JSILIdentifier JSIL;
        public readonly JSSpecialIdentifiers JS;
        public readonly CLRSpecialIdentifiers CLR;

        public ILBlockTranslator (DecompilerContext context, MethodDefinition method, ILBlock ilb, ITypeInfoSource typeInfo, IEnumerable<ILVariable> allVariables) {
            Context = context;
            ThisMethod = method;
            Block = ilb;
            TypeInfo = typeInfo;

            JSIL = new JSILIdentifier(TypeSystem);
            JS = new JSSpecialIdentifiers(TypeSystem);
            CLR = new CLRSpecialIdentifiers(TypeSystem);

            if (method.HasThis)
                Variables.Add("this", JSThisParameter.New(method.DeclaringType));

            foreach (var parameter in method.Parameters)
                Variables.Add(parameter.Name, JSVariable.New(parameter));

            foreach (var variable in allVariables)
                Variables.Add(variable.Name, JSVariable.New(variable));
        }

        protected TypeSystem TypeSystem {
            get {
                return Context.CurrentModule.TypeSystem;
            }
        }

        public JSBlockStatement Translate () {
            return TranslateNode(Block);
        }

        public JSNode TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node        NYI: {0}", node.GetType().Name);

            return new JSInvocationExpression(
                JSIL.UntranslatableNode,
                new JSStringLiteral(node.GetType().Name)
            );
        }

        public JSExpression[] Translate (IList<ILExpression> values, IList<ParameterDefinition> parameters) {
            var result = new List<JSExpression>();
            ParameterDefinition parameter;

            for (int i = 0, c = values.Count; i < c; i++) {
                var value = values[i];

                var parameterIndex = i + Math.Max(0, (values.Count - parameters.Count));
                if (parameterIndex < parameters.Count)
                    parameter = parameters[parameterIndex];
                else
                    parameter = null;

                var translated = TranslateNode(value);

                if ((parameter != null) && (parameter.ParameterType is ByReferenceType)) {
                    result.Add(new JSPassByReferenceExpression(translated));
                } else
                    result.Add(translated);
            }

            return result.ToArray();
        }

        public JSExpression[] Translate (IEnumerable<ILExpression> values) {
            return (
                from value in values select TranslateNode(value)
            ).ToArray();
        }

        public static JSVariable[] Translate (IEnumerable<ParameterDefinition> parameters) {
            return (
                from p in parameters select JSVariable.New(p)
            ).ToArray();
        }

        protected JSVariable DeclareVariable (string name, TypeReference type) {
            JSVariable result;
            if (Variables.TryGetValue(name, out result)) {
                if (result.Type != type)
                    throw new InvalidOperationException("A variable with that name is already declared in this scope, with a different type.");
            } else {
                result = new JSVariable(name, type);
                Variables.Add(name, result);
            }

            return result;
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
                TranslateNode(node.Arguments[0]),
                node.ExpectedType
            );
        }

        protected JSBinaryOperatorExpression Translate_BinaryOp (ILExpression node, JSBinaryOperator op) {
            return new JSBinaryOperatorExpression(
                op,
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1]),
                node.ExpectedType
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
                    return new JSUnaryOperatorExpression(
                        JSOperator.LogicalNot, TranslateNode(node.Arguments[0])
                    );
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
                    JSIL.UntranslatableInstruction,
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
                catchVariable = DeclareVariable("$exception", Context.CurrentModule.TypeSystem.Object);

                bool isFirst = true, foundUniversalCatch = false, openBrace = false;
                foreach (var cb in tcb.CatchBlocks) {
                    JSExpression pairCondition = null;

                    if (
                        (cb.ExceptionType.FullName == "System.Exception") ||
                        (cb.ExceptionType.FullName == "System.Object")
                    ) {
                        if (foundUniversalCatch)
                            throw new NotImplementedException("Found multiple catch-all clauses");

                        foundUniversalCatch = true;
                    } else {
                        if (foundUniversalCatch)
                            throw new NotImplementedException("Catch-all clause must be last");

                        pairCondition = JSIL.CheckType(catchVariable, cb.ExceptionType);
                    }

                    var pairBody = TranslateBlock(cb.Body);

                    if (cb.ExceptionVariable != null) {
                        var excVariable = DeclareVariable(cb.ExceptionVariable.Name, cb.ExceptionVariable.Type);

                        pairBody.Statements.Insert(
                            0, new JSVariableDeclarationStatement(new JSBinaryOperatorExpression(
                                JSOperator.Assignment, excVariable,
                                catchVariable, cb.ExceptionVariable.Type
                            ))
                        );
                    }

                    pairs.Add(new KeyValuePair<JSExpression, JSStatement>(
                        pairCondition, pairBody
                    ));
                }

                if (!foundUniversalCatch)
                    pairs.Add(new KeyValuePair<JSExpression,JSStatement>(
                        null, new JSExpressionStatement(new JSThrowExpression(catchVariable))
                    ));

                if ((pairs.Count == 1) && (pairs[0].Key == null))
                    catchBlock = new JSBlockStatement(
                        pairs[0].Value
                    );
                else
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
                return JSIL.CheckType(
                    TranslateNode(node.Arguments[1]),
                    (TypeReference)node.Arguments[0].Operand
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

        protected JSBinaryOperatorExpression Translate_Rem (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Remainder);
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

        protected JSBinaryOperatorExpression Translate_Or (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.BitwiseOr);
        }

        protected JSBinaryOperatorExpression Translate_Xor (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.BitwiseXor);
        }

        protected JSUnaryOperatorExpression Translate_Not (ILExpression node) {
            return Translate_UnaryOp(node, JSOperator.BitwiseNot);
        }

        protected JSBinaryOperatorExpression Translate_LogicOr (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.LogicalOr);
        }

        protected JSBinaryOperatorExpression Translate_LogicAnd (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.LogicalAnd);
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
            return Translate_UnaryOp(node, JSOperator.Negation);
        }

        protected JSThrowExpression Translate_Throw (ILExpression node) {
            return new JSThrowExpression(TranslateNode(node.Arguments[0]));
        }

        protected JSExpression Translate_Endfinally (ILExpression node) {
            return JSExpression.Null;
        }

        protected JSBreakExpression Translate_LoopOrSwitchBreak (ILExpression node) {
            return new JSBreakExpression();
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

        protected JSVariable Translate_Ldloc (ILExpression node, ILVariable variable) {
            return new JSIndirectVariable(Variables, variable.Name);
        }

        protected JSExpression Translate_Ldloca (ILExpression node, ILVariable variable) {
            return JSReferenceExpression.New(
                Translate_Ldloc(node, variable)
            );
        }

        protected JSBinaryOperatorExpression Translate_Stloc (ILExpression node, ILVariable variable) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment, Translate_Ldloc(node, variable),
                TranslateNode(node.Arguments[0]),
                TypeSystem
            );
        }

        protected JSDotExpression Translate_Ldsfld (ILExpression node, FieldReference field) {
            return new JSDotExpression(new JSType(field.DeclaringType), new JSField(field));
        }

        protected JSExpression Translate_Ldsflda (ILExpression node, FieldReference field) {
            return new JSMemberReferenceExpression(Translate_Ldsfld(node, field));
        }

        protected JSBinaryOperatorExpression Translate_Stsfld (ILExpression node, FieldReference field) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                Translate_Ldsfld(node, field),
                TranslateNode(node.Arguments[0]),
                TypeSystem
            );
        }

        protected JSDotExpression Translate_Ldfld (ILExpression node, FieldReference field) {
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);

            JSExpression thisExpression;
            if (firstArg.InferredType.GetElementType().IsValueType) {
                if (!JSReferenceExpression.TryDereference(translated, out thisExpression))
                    throw new InvalidOperationException("this-expression for field access on value type must be a reference");
            } else {
                thisExpression = translated;
            }

            return new JSDotExpression(
                thisExpression, 
                new JSField(field)
            );
        }

        protected JSBinaryOperatorExpression Translate_Stfld (ILExpression node, FieldReference field) {
            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                Translate_Ldfld(node, field),
                TranslateNode(node.Arguments[1]),
                TypeSystem
            );
        }

        protected JSExpression Translate_Ldflda (ILExpression node, FieldReference field) {
            return new JSMemberReferenceExpression(Translate_Ldfld(node, field));
        }

        protected JSExpression Translate_Ldobj (ILExpression node, TypeReference type) {
            var reference = TranslateNode(node.Arguments[0]);
            JSExpression referent;

            if (!JSReferenceExpression.TryDereference(reference, out referent))
                throw new NotImplementedException();

            return reference;
        }

        protected JSExpression Translate_Ldind (ILExpression node) {
            return Translate_Ldobj(node, null);
        }

        protected JSExpression Translate_AddressOf (ILExpression node) {
            return JSReferenceExpression.New(TranslateNode(node.Arguments[0]));
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
            return JSLiteral.Null(node.ExpectedType);
        }

        protected JSExpression Translate_Ldftn (ILExpression node, MethodReference method) {
            if (method.HasThis)
                return JSDotExpression.New(
                    new JSType(method.DeclaringType),
                    JS.prototype,
                    new JSMethod(method)
                );
            else
                return new JSDotExpression(
                    new JSType(method.DeclaringType),
                    new JSMethod(method)
                );
        }

        protected JSExpression Translate_Ldc (ILExpression node, long value) {
            TypeInfo typeInfo = null;
            if (node.ExpectedType != null)
                typeInfo = TypeInfo.Get(node.ExpectedType);

            if ((typeInfo != null) && (typeInfo.EnumMembers.Count > 0)) {
                EnumMemberInfo em;

                if (typeInfo.ValueToEnumMember.TryGetValue(value, out em))
                    return new JSEnumLiteral(em);
                else {
                    switch (node.Code) {
                        case ILCode.Ldc_I4:
                        return new JSIntegerLiteral(value, typeof(int));
                        case ILCode.Ldc_I8:
                        return new JSIntegerLiteral(value, typeof(long));
                    }

                    throw new NotImplementedException();
                }
            } else if (node.ExpectedType.FullName == "System.Boolean") {
                return JSLiteral.New(value != 0);
            } else {
                switch (node.Code) {
                    case ILCode.Ldc_I4:
                        return new JSIntegerLiteral(value, typeof(int));
                    case ILCode.Ldc_I8:
                        return new JSIntegerLiteral(value, typeof(long));
                }

                throw new NotImplementedException();
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
                CLR.Length
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
                TranslateNode(node.Arguments[2]),
                TypeSystem
            );
        }

        protected JSInvocationExpression Translate_NullCoalescing (ILExpression node) {
            return JSIL.Coalesce(
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1])
            );
        }

        protected JSExpression Translate_Castclass (ILExpression node, TypeReference targetType) {
            if (IsDelegateType(targetType) && IsDelegateType(node.ExpectedType)) {
                // TODO: We treat all delegate types as equivalent, so we can skip these casts for now
                return TranslateNode(node.Arguments[0]);
            }

            return JSIL.Cast(
                TranslateNode(node.Arguments[0]),
                targetType
            );
        }

        protected JSInvocationExpression Translate_Isinst (ILExpression node, TypeReference targetType) {
            return JSIL.TryCast(
                TranslateNode(node.Arguments[0]),
                targetType
            );
        }

        protected JSExpression Translate_Unbox_Any (ILExpression node, TypeReference targetType) {
            return Translate_Castclass(node, targetType);
        }

        protected JSInvocationExpression Translate_Conv (ILExpression node, TypeReference targetType) {
            return JSIL.Cast(
                TranslateNode(node.Arguments[0]),
                targetType
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
                return JSIL.NewDelegate(
                    constructor.DeclaringType,
                    TranslateNode(node.Arguments[0]),
                    TranslateNode(node.Arguments[1])
                );
            } else if (constructor.DeclaringType.IsArray) {
                return JSIL.NewMultidimensionalArray(
                    constructor.DeclaringType.GetElementType(),
                    Translate(node.Arguments)
                );
            }

            return new JSNewExpression(
                constructor.DeclaringType,
                Translate(node.Arguments)
            );
        }

        protected JSExpression Translate_DefaultValue (ILExpression node, TypeReference type) {
            return JSLiteral.DefaultValue(type);
        }

        protected JSInvocationExpression Translate_Newarr (ILExpression node, TypeReference elementType) {
            return JSIL.NewArray(
                elementType,
                TranslateNode(node.Arguments[0])
            );
        }

        protected JSInvocationExpression Translate_InitArray (ILExpression node, TypeReference elementType) {
            return JSIL.NewArray(
                elementType,
                new JSArrayExpression(elementType, Translate(node.Arguments))
            );
        }

        protected JSExpression Translate_InitializedObject (ILExpression node) {
            return JSExpression.Null;
        }

        protected JSExpression Translate_InitCollection (ILExpression node) {
            TypeReference inferredType = null;
            var values = new List<JSExpression>();

            for (var i = 1; i < node.Arguments.Count; i++) {
                var invocation = (JSInvocationExpression)TranslateNode(node.Arguments[i]);

                var valueType = invocation.Arguments[0].GetExpectedType(TypeSystem);

                if (inferredType == null)
                    inferredType = valueType;
                else if (inferredType.FullName != valueType.FullName)
                    throw new NotImplementedException("Mixed-type collection initializers not supported");

                values.Add(invocation.Arguments[0]);
            }

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                TranslateNode(node.Arguments[0]),
                JSIL.NewCollectionInitializer(
                    new JSArrayExpression(inferredType, values.ToArray())
                ),
                TypeSystem
            );
        }

        protected JSInvocationExpression Translate_InitObject (ILExpression node) {
            var initializers = new List<JSPairExpression>();

            for (var i = 1; i < node.Arguments.Count; i++) {
                var translated = TranslateNode(node.Arguments[i]);

                var boe = translated as JSBinaryOperatorExpression;
                if (boe != null) {
                    var key = ((JSDotExpression)boe.Left).Member;
                    var value = boe.Right;

                    initializers.Add(new JSPairExpression(key, value));
                } else {
                    Debug.WriteLine(String.Format("Warning: Object initializer element not implemented: {0}", translated));
                }
            }

            var target = TranslateNode(node.Arguments[0]);

            return new JSInvocationExpression(
                new JSDotExpression(
                    target, new JSIdentifier("__Initialize__", target.GetExpectedType(TypeSystem))
                ),
                new JSObjectExpression(initializers.ToArray())
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

            var thisType = ThisMethod.DeclaringType.GetElementType();
            var declaringType = method.DeclaringType.GetElementType();

            IEnumerable<ILExpression> arguments = node.Arguments;
            JSExpression thisExpression;
            JSIdentifier methodName;

            if (method.HasThis) {
                var firstArg = arguments.First();
                var ilv = firstArg.Operand as ILVariable;

                var firstArgType = firstArg.ExpectedType.GetElementType();
                var translated = TranslateNode(firstArg);

                if (firstArg.InferredType.GetElementType().IsValueType) {
                    if (!JSReferenceExpression.TryDereference(translated, out thisExpression))
                        throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
                } else {
                    thisExpression = translated;
                }

                // If the call is of the form x.Method(...), we don't need to specify the this parameter
                //  explicitly using the form type.Method.call(x, ...).
                // Make sure that 'this' references only pass this check when they don't refer to 
                //  members of base types/interfaces.
                if (
                    (declaringType.Equals(firstArgType)) &&
                    (
                        (ilv == null) || (ilv.Name != "this") ||
                        (thisType.Equals(firstArgType))
                    )
                ) {
                    methodName = new JSMethod(method);
                    arguments = arguments.Skip(1);
                } else {
                    thisExpression = JSDotExpression.New(
                        new JSType(method.DeclaringType),
                        JS.prototype, new JSMethod(method)
                    );
                    methodName = JS.call;
                }
            } else {
                thisExpression = new JSType(method.DeclaringType);
                methodName = new JSMethod(method);
            }

            return new JSInvocationExpression(
                new JSDotExpression(thisExpression, methodName),
                Translate(arguments.ToArray(), method.Parameters)
            );
        }

        protected JSExpression Translate_Callvirt (ILExpression node, MethodReference method) {
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);
            JSExpression thisExpression;

            if (firstArg.InferredType.GetElementType().IsValueType) {
                if (!JSReferenceExpression.TryDereference(translated, out thisExpression))
                    throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
            } else {
                thisExpression = translated;
            }

            return new JSInvocationExpression(
                new JSDotExpression(
                    thisExpression, new JSMethod(method)
                ),
                Translate(node.Arguments.Skip(1), method.Parameters)
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

            JSExpression target;
            if (!JSReferenceExpression.TryDereference(
                TranslateNode(node.Arguments[0]), out target
            ))
                throw new InvalidOperationException("Postfix increment/decrement require a reference to operate on");

            if (arg == 1)
                return new JSUnaryOperatorExpression(
                    JSOperator.PostIncrement, target
                );
            else
                return new JSUnaryOperatorExpression(
                    JSOperator.PostDecrement, target
                );
        }
    }
}
