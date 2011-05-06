using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using Microsoft.CSharp.RuntimeBinder;
using Mono.Cecil;

namespace JSIL {
    class ILBlockTranslator {
        public readonly DecompilerContext Context;
        public readonly MethodDefinition ThisMethod;
        public readonly ILBlock Block;
        public readonly JavascriptFormatter Output = null;
        public readonly ITypeInfoSource TypeInfo;

        public readonly HashSet<string> ParameterNames = new HashSet<string>();
        public readonly Dictionary<string, JSVariable> Variables = new Dictionary<string, JSVariable>();
        public readonly DynamicCallSiteInfoCollection DynamicCallSites = new DynamicCallSiteInfoCollection();

        public readonly JSILIdentifier JSIL;
        public readonly JSSpecialIdentifiers JS;
        public readonly CLRSpecialIdentifiers CLR;

        public ILBlockTranslator (DecompilerContext context, MethodDefinition method, ILBlock ilb, ITypeInfoSource typeInfo, IEnumerable<ILVariable> parameters, IEnumerable<ILVariable> allVariables) {
            Context = context;
            ThisMethod = method;
            Block = ilb;
            TypeInfo = typeInfo;

            JSIL = new JSILIdentifier(TypeSystem);
            JS = new JSSpecialIdentifiers(TypeSystem);
            CLR = new CLRSpecialIdentifiers(TypeSystem);

            if (method.HasThis)
                Variables.Add("this", JSThisParameter.New(method.DeclaringType));

            foreach (var parameter in parameters) {
                if ((parameter.Name == "this") && (parameter.OriginalParameter.Index == -1))
                    continue;

                ParameterNames.Add(parameter.Name);
                Variables.Add(parameter.Name, new JSParameter(parameter.Name, parameter.Type));
            }

            foreach (var variable in allVariables)
                Variables.Add(variable.Name, JSVariable.New(variable));
        }

        public TypeSystem TypeSystem {
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

        protected JSExpression Translate_IgnoredMethod (string methodName, IEnumerable<ParameterDefinition> parameters) {
            return new JSInvocationExpression(
                JSIL.IgnoredMember, JSLiteral.New(String.Format(
                    "{0}({1})",
                    methodName,
                    String.Join(", ", (from p in parameters select p.Name).ToArray())
                ))
            );
        }

        protected JSExpression Translate_MethodReplacement (MethodReference method, JSExpression methodExpression, JSExpression[] arguments) {
            var typeInfo = TypeInfo.Get(method.DeclaringType);
            if (typeInfo != null) {
                MetadataCollection metadata;
                if (typeInfo.MemberMetadata.TryGetValue(method, out metadata)) {
                    var parms = metadata.GetAttributeParameters("JSIL.Meta.JSReplacement");
                    if (parms != null)
                        methodExpression = new JSVerbatimLiteral((string)parms[0].Value);
                }
            }

            if (TypeInfo.IsIgnored(method))
                return Translate_IgnoredMethod(method.Name, method.Parameters);

            switch (method.FullName) {
                case "System.Object JSIL.Builtins::Eval(System.String)":
                    methodExpression = JS.eval;
                break;
                case "System.Object JSIL.Verbatim::Expression(System.String)":
                    var expression = arguments[0] as JSStringLiteral;
                    if (expression == null)
                        throw new InvalidOperationException("JSIL.Verbatim.Expression must recieve a string literal as an argument");

                    return new JSVerbatimLiteral(expression.Value);
            }

            return new JSInvocationExpression(
                methodExpression, arguments
            );
        }

        protected bool Translate_PropertyCall (JSExpression thisExpression, MethodDefinition method, JSExpression[] arguments, out JSExpression result) {
            result = null;

            var typeInfo = TypeInfo.Get(method.DeclaringType);
            if (typeInfo == null)
                return false;

            PropertyDefinition property;
            if (!typeInfo.MethodToProperty.TryGetValue(method, out property))
                return false;

            if (TypeInfo.IsIgnored(property)) {
                result = new JSInvocationExpression(
                    JSIL.IgnoredMember, JSLiteral.New(property.Name)
                );
                return true;
            }

            // JS provides no way to override [], so keep it as a regular method call
            if (property.IsIndexer())
                return false;

            // Accesses to a base property should go through a regular method invocation, since
            //  javascript properties do not have a mechanism for base access
            if (method.HasThis && !TypesAreEqual(property.DeclaringType, thisExpression.GetExpectedType(TypeSystem)))
                return false;

            if (method == property.GetMethod) {
                result = new JSDotExpression(
                    thisExpression, new JSProperty(property)
                );
            } else {
                result = new JSBinaryOperatorExpression(
                    JSOperator.Assignment,
                    new JSDotExpression(
                        thisExpression, new JSProperty(property)
                    ),
                    arguments[0], property.PropertyType
                );
            }

            return true;
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

        public static TypeReference DereferenceType (TypeReference type) {
            var brt = type as ByReferenceType;
            if (brt != null)
                return brt.ElementType;

            var pt = type as PointerType;
            if (pt != null)
                return pt.ElementType;

            return type;
        }

        public static bool IsNumeric (TypeReference type) {
            if (type.IsPrimitive) {
                switch (type.FullName) {
                    case "System.String":
                    case "System.IntPtr":
                    case "System.UIntPtr":
                    case "System.Boolean":
                        return false;
                    default:
                        return true;
                }
            } else {
                return false;
            }
        }

        public static bool IsIntegral (TypeReference type) {
            if (type.IsPrimitive) {
                switch (type.FullName) {
                    case "System.String":
                    case "System.IntPtr":
                    case "System.UIntPtr":
                    case "System.Decimal":
                    case "System.Double":
                    case "System.Single":
                    case "System.Boolean":
                        return false;
                    default:
                        return true;
                }
            } else {
                return false;
            }
        }

        public static bool IsDelegateType (TypeReference type) {
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

        public static bool TypesAreEqual (TypeReference target, TypeReference source) {
            var dTarget = target.Resolve();
            var dSource = source.Resolve();

            if (Object.Equals(dTarget, dSource) && (dSource != null))
                return true;
            if (Object.Equals(target, source))
                return true;

            return false;
        }

        public static bool TypesAreAssignable (TypeReference target, TypeReference source) {
            if (TypesAreEqual(target, source))
                return true;

            var dSource = source.Resolve();

            if (TypesAreAssignable(target, dSource.BaseType))
                return true;

            foreach (var iface in dSource.Interfaces) {
                if (TypesAreAssignable(target, iface))
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
            } catch (TargetInvocationException tie) {
                Console.Error.WriteLine("Error occurred while translating node {0}", expression);
                throw;
            }

            return result;
        }

        protected bool TranslateCallSiteConstruction (ILCondition condition, out JSStatement result) {
            var cond = condition.Condition;
            if (
                (cond.Code == ILCode.LogicNot) &&
                (cond.Arguments.Count > 0) &&
                (cond.Arguments[0].Code == ILCode.GetCallSite) &&
                (condition.TrueBlock != null) &&
                (condition.TrueBlock.Body.Count == 1) &&
                (condition.TrueBlock.Body[0] is ILExpression)
            ) {
                var callSiteExpression = (ILExpression)condition.TrueBlock.Body[0];
                var callSiteType = callSiteExpression.Arguments[0].ExpectedType;
                var binderExpression = callSiteExpression.Arguments[0].Arguments[0];
                var binderMethod = (MethodReference)binderExpression.Operand;
                var arguments = Translate(binderExpression.Arguments);
                var targetType = ((IGenericInstance)callSiteType).GenericArguments[0];

                DynamicCallSites.InitializeCallSite(
                    (FieldReference)cond.Arguments[0].Operand,
                    binderMethod.Name,
                    targetType,
                    arguments
                );

                result = new JSNullStatement();
                return true;
            }

            result = null;
            return false;
        }

        public JSStatement TranslateNode (ILCondition condition) {
            JSStatement result = null;
            if (TranslateCallSiteConstruction(condition, out result))
                return result;

            JSStatement falseBlock = null;
            if ((condition.FalseBlock != null) && (condition.FalseBlock.Body.Count > 0))
                falseBlock = TranslateNode(condition.FalseBlock);

            result = new JSIfStatement(
                TranslateNode(condition.Condition),
                TranslateNode(condition.TrueBlock),
                falseBlock
            );

            return result;
        }

        public JSLabelStatement TranslateNode (ILLabel label) {
            return new JSLabelStatement(label.Name);
        }

        public JSSwitchCase TranslateNode (ILSwitch.CaseBlock block) {
            JSExpression[] values = null;

            if (block.Values != null)
                values = (from v in block.Values select JSLiteral.New(v)).ToArray();

            return new JSSwitchCase(
                values,
                TranslateNode(new ILBlock(block.Body))
            );
        }

        public JSSwitchStatement TranslateNode (ILSwitch swtch) {
            return new JSSwitchStatement(
                TranslateNode(swtch.Condition),
                (from cb in swtch.CaseBlocks select TranslateNode(cb)).ToArray()
            );
        }

        public JSTryCatchBlock TranslateNode (ILTryCatchBlock tcb) {
            var body = TranslateNode(tcb.TryBlock);
            JSVariable catchVariable = null;
            JSBlockStatement catchBlock = null;
            JSBlockStatement finallyBlock = null;

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
                        // Bad IL sometimes contains entirely meaningless catch clauses. It's best to just ignore them.
                        if (
                            (cb.Body.Count == 1) && (cb.Body[0] is ILExpression) &&
                            (((ILExpression)cb.Body[0]).Code == ILCode.Rethrow)
                        ) {
                            continue;
                        }

                        if (foundUniversalCatch) {
                            Debug.WriteLine("Found multiple catch-all catch clauses. Any after the first will be ignored.");
                            continue;
                        }

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

            if (tcb.FaultBlock != null) {
                Debug.WriteLine("Warning: Fault blocks are not translatable.");
                body.Statements.Add(new JSExpressionStatement(new JSInvocationExpression(
                    JSIL.UntranslatableNode, 
                    JSLiteral.New(tcb.FaultBlock.ToString())
                )));
            }

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

        protected JSBinaryOperatorExpression Translate_CompoundAssignment (ILExpression node) {
            JSAssignmentOperator op;
            var translated = (JSBinaryOperatorExpression)TranslateNode(node.Arguments[0]);

            switch (node.Arguments[0].Code) {
                case ILCode.Add:
                    op = JSOperator.AddAssignment;
                    break;
                case ILCode.Sub:
                    op = JSOperator.SubtractAssignment;
                    break;
                case ILCode.Mul:
                    op = JSOperator.MultiplyAssignment;
                    break;
                // We can't emit the /= operator since its semantics differ from C#'s
                case ILCode.Div:
                    return new JSBinaryOperatorExpression(
                        JSOperator.Assignment, translated.Left, 
                        translated, translated.ExpectedType
                    );
                case ILCode.Rem:
                    op = JSOperator.RemainderAssignment;
                    break;
                case ILCode.Shl:
                    op = JSOperator.ShiftLeftAssignment;
                    break;
                case ILCode.Shr_Un:
                    op = JSOperator.ShiftRightUnsignedAssignment;
                    break;
                case ILCode.Shr:
                    op = JSOperator.ShiftRightAssignment;
                    break;
                case ILCode.And:
                    op = JSOperator.BitwiseAndAssignment;
                    break;
                case ILCode.Or:
                    op = JSOperator.BitwiseOrAssignment;
                    break;
                case ILCode.Xor:
                    op = JSOperator.BitwiseXorAssignment;
                    break;
                default:
                    return null;
            }

            return new JSBinaryOperatorExpression(
                op, translated.Left, translated.Right, translated.ExpectedType
            );
        }

        protected JSTernaryOperatorExpression Translate_TernaryOp (ILExpression node) {
            return new JSTernaryOperatorExpression(
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1]),
                TranslateNode(node.Arguments[2]),
                node.ExpectedType
            );
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

        protected JSBinaryOperatorExpression Translate_Shr_Un (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.ShiftRightUnsigned);
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

        protected JSThrowExpression Translate_Rethrow (ILExpression node) {
            return new JSThrowExpression(new JSIdentifier("$exception", TypeSystem.Object));
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

        protected JSContinueExpression Translate_LoopContinue (ILExpression node) {
            return new JSContinueExpression();
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

        protected JSExpression Translate_Stloc (ILExpression node, ILVariable variable) {
            if (node.Arguments[0].Code == ILCode.GetCallSite)
                DynamicCallSites.SetAlias(variable, (FieldReference)node.Arguments[0].Operand);

            // GetCallSite and CreateCallSite produce null expressions, so we want to ignore assignments containing them
            var value = TranslateNode(node.Arguments[0]);
            if (value.IsNull)
                return new JSNullExpression();

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment, Translate_Ldloc(node, variable),
                value,
                value.GetExpectedType(TypeSystem)
            );
        }

        protected JSExpression Translate_Ldsfld (ILExpression node, FieldReference field) {
            if (TypeInfo.IsIgnored(field))
                return new JSInvocationExpression(
                    JSIL.IgnoredMember, JSLiteral.New(field.Name)
                ); 

            return new JSDotExpression(new JSType(field.DeclaringType), new JSField(field));
        }

        protected JSExpression Translate_Ldsflda (ILExpression node, FieldReference field) {
            return new JSMemberReferenceExpression(Translate_Ldsfld(node, field));
        }

        protected JSBinaryOperatorExpression Translate_Stsfld (ILExpression node, FieldReference field) {
            var rhs = TranslateNode(node.Arguments[0]);

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                Translate_Ldsfld(node, field),
                rhs,
                rhs.GetExpectedType(TypeSystem)
            );
        }

        protected JSExpression Translate_Ldfld (ILExpression node, FieldReference field) {
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);

            // GetCallSite and CreateCallSite produce null expressions, so we want to ignore field references containing them
            if (translated.IsNull)
                return new JSNullExpression();

            if (TypeInfo.IsIgnored(field))
                return new JSInvocationExpression(
                    JSIL.IgnoredMember, JSLiteral.New(field.Name)
                );

            JSExpression thisExpression;
            if (DereferenceType(firstArg.InferredType).IsValueType) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression))
                    Debug.WriteLine("Warning: Accessing a field of a value type with a value as this instead of a reference.");
            } else {
                thisExpression = translated;
            }

            return new JSDotExpression(
                thisExpression, 
                new JSField(field)
            );
        }

        protected JSExpression Translate_Stind (ILExpression node) {
            return new JSInvocationExpression(
                JSIL.UntranslatableInstruction,
                JSLiteral.New(node.Code.ToString())
            );
        }

        protected JSBinaryOperatorExpression Translate_Stfld (ILExpression node, FieldReference field) {
            var rhs = TranslateNode(node.Arguments[1]);

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                Translate_Ldfld(node, field),
                rhs, rhs.GetExpectedType(TypeSystem)
            );
        }

        protected JSExpression Translate_Ldflda (ILExpression node, FieldReference field) {
            return new JSMemberReferenceExpression(Translate_Ldfld(node, field));
        }

        protected JSExpression Translate_Ldobj (ILExpression node, TypeReference type) {
            var reference = TranslateNode(node.Arguments[0]);
            JSExpression referent;

            if (!JSReferenceExpression.TryDereference(JSIL, reference, out referent))
                Debug.WriteLine(String.Format("Warning: unsupported reference type for ldobj: {0}", node.Arguments[0]));

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
            string typeName = null;
            var expressionType = node.ExpectedType ?? node.InferredType;
            TypeInfo typeInfo = null;
            if (expressionType != null) {
                typeName = expressionType.FullName;
                typeInfo = TypeInfo.Get(expressionType);
            }

            if ((typeInfo != null) && (typeInfo.EnumMembers.Count > 0)) {
                EnumMemberInfo[] enumMembers = null;
                if (typeInfo.IsFlagsEnum) {
                    enumMembers = (
                        from em in typeInfo.EnumMembers.Values
                        where (value & em.Value) == em.Value
                        select em
                    ).ToArray();
                } else {
                    EnumMemberInfo em;
                    if (typeInfo.ValueToEnumMember.TryGetValue(value, out em))
                        enumMembers = new EnumMemberInfo[1] { em };
                }

                if ((enumMembers != null) && (enumMembers.Length > 0))
                    return new JSEnumLiteral(value, enumMembers);
                else {
                    switch (node.Code) {
                        case ILCode.Ldc_I4:
                            return new JSIntegerLiteral(value, typeof(int));
                        case ILCode.Ldc_I8:
                            return new JSIntegerLiteral(value, typeof(long));
                    }

                    throw new NotImplementedException();
                }
            } else if (typeName == "System.Boolean") {
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

        protected JSExpression Translate_Ldelem (ILExpression node, TypeReference elementType) {
            var expectedType = elementType ?? node.ExpectedType ?? node.InferredType;

            JSExpression result = new JSIndexerExpression(
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1]),
                expectedType
            );

            if (EmulateStructAssignment.IsStruct(expectedType))
                result = JSReferenceExpression.New(result);

            return result;
        }

        protected JSExpression Translate_Ldelem (ILExpression node) {
            return Translate_Ldelem(node, null);
        }

        protected JSExpression Translate_Ldelema (ILExpression node, TypeReference elementType) {
            return JSReferenceExpression.New(Translate_Ldelem(node, elementType));
        }

        protected JSBinaryOperatorExpression Translate_Stelem (ILExpression node) {
            return Translate_Stelem(node, null);
        }

        protected JSBinaryOperatorExpression Translate_Stelem (ILExpression node, TypeReference elementType) {
            var rhs = TranslateNode(node.Arguments[2]);

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSIndexerExpression(
                    TranslateNode(node.Arguments[0]),
                    TranslateNode(node.Arguments[1])
                ),
                rhs, elementType ?? rhs.GetExpectedType(TypeSystem)
            );
        }

        protected JSInvocationExpression Translate_NullCoalescing (ILExpression node) {
            return JSIL.Coalesce(
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1]),
                node.ExpectedType ?? node.InferredType
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
            var value = TranslateNode(node.Arguments[0]);
            var result = JSIL.Cast(value, targetType);

            if (EmulateStructAssignment.IsStruct(targetType))
                return JSReferenceExpression.New(result);
            else
                return result;
        }

        protected JSExpression Translate_Conv (ILExpression node, TypeReference targetType) {
            var value = TranslateNode(node.Arguments[0]);
            var currentType = value.GetExpectedType(TypeSystem);

            if (IsNumeric(currentType) && IsNumeric(targetType)) {
                if (IsIntegral(targetType)) {
                    if (IsIntegral(currentType))
                        return value;
                    else
                        return new JSInvocationExpression(JS.floor, value);
                } else {
                    return value;
                }
            } else
                return JSIL.Cast(value, targetType);
        }

        protected JSExpression Translate_Conv_I (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int64);
        }

        protected JSExpression Translate_Conv_U (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.UInt64);
        }

        protected JSExpression Translate_Conv_U1 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Byte);
        }

        protected JSExpression Translate_Conv_U2 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.UInt16);
        }

        protected JSExpression Translate_Conv_U4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.UInt32);
        }

        protected JSExpression Translate_Conv_U8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.UInt64);
        }

        protected JSExpression Translate_Conv_I1 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.SByte);
        }

        protected JSExpression Translate_Conv_I2 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int16);
        }

        protected JSExpression Translate_Conv_I4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int32);
        }

        protected JSExpression Translate_Conv_I8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int64);
        }

        protected JSExpression Translate_Conv_R4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Single);
        }

        protected JSExpression Translate_Conv_R8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Double);
        }

        protected JSExpression Translate_Box (ILExpression node, TypeReference valueType) {
            return JSReferenceExpression.New(TranslateNode(node.Arguments[0]));
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

            if (TypeInfo.IsIgnored(constructor))
                return Translate_IgnoredMethod(
                    constructor.Name, constructor.Parameters
                );

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
            // This should get eliminated by the handler for InitObject, but if we just return a null expression here,
            //  stfld treats us as an invalid assignment target.
            return new JSInvocationExpression(JSIL.UntranslatableInstruction, JSLiteral.New("InitializedObject"));
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

            var initializer = JSIL.NewCollectionInitializer(
                new JSArrayExpression(inferredType, values.ToArray())
            );

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                TranslateNode(node.Arguments[0]),
                initializer,
                TypeSystem.Object
            );
        }

        protected JSInvocationExpression Translate_InitObject (ILExpression node) {
            var initializers = new List<JSPairExpression>();

            for (var i = 1; i < node.Arguments.Count; i++) {
                var translated = TranslateNode(node.Arguments[i]);

                var boe = translated as JSBinaryOperatorExpression;
                if (boe != null) {
                    var leftDot = boe.Left as JSDotExpression;

                    if (leftDot != null) {
                        var key = leftDot.Member;
                        var value = boe.Right;

                        initializers.Add(new JSPairExpression(key, value));
                    } else {
                        Debug.WriteLine(String.Format("Warning: Unrecognized object initializer form: {0}", boe));
                    }
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
                    return new JSInvocationExpression(
                        JSIL.UntranslatableInstruction, 
                        JSLiteral.New(node.Arguments[0].ToString())
                    );
                }
            }

            var methodDef = method.Resolve();
            var thisType = DereferenceType(ThisMethod.DeclaringType);
            var declaringType = DereferenceType(method.DeclaringType);

            var declaringTypeDef = declaringType.Resolve();

            IEnumerable<ILExpression> arguments = node.Arguments;
            JSExpression thisExpression;
            JSIdentifier methodName;

            if (method.HasThis) {
                var firstArg = arguments.First();
                var ilv = firstArg.Operand as ILVariable;

                var firstArgType = DereferenceType(firstArg.ExpectedType);

                var translated = TranslateNode(firstArg);

                if (DereferenceType(firstArg.InferredType).IsValueType) {
                    if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression))
                        throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
                } else {
                    thisExpression = translated;
                }

                // If the call is of the form x.Method(...), we don't need to specify the this parameter
                //  explicitly using the form type.Method.call(x, ...).
                // Make sure that 'this' references only pass this check when they don't refer to 
                //  members of base types. It's always okay to use thiscall form for interfaces, since we qualify 
                //  the name of the method/property.
                if (
                    ((TypesAreEqual(declaringType, firstArgType)) &&
                    (
                        (ilv == null) || (ilv.Name != "this") ||
                        (TypesAreEqual(thisType, firstArgType))
                    )) || (
                        (declaringTypeDef != null) && 
                        (declaringTypeDef.IsInterface) &&
                        TypesAreAssignable(declaringTypeDef, thisType) &&
                        TypesAreAssignable(declaringTypeDef, firstArgType)
                    )
                ) {
                    methodName = new JSMethod(method);
                    arguments = arguments.Skip(1);
                } else {
                    thisExpression = JSDotExpression.New(
                        new JSType(method.DeclaringType),
                        JS.prototype, new JSMethod(method)
                    );
                    methodName = JS.call(method.ReturnType);
                }
            } else {
                thisExpression = new JSType(method.DeclaringType);
                methodName = new JSMethod(method);
            }

            var translatedArguments = Translate(arguments.ToArray(), method.Parameters);

            if (methodDef != null) {
                JSExpression propertyResult;
                if (Translate_PropertyCall(thisExpression, methodDef, translatedArguments, out propertyResult))
                    return propertyResult;
            }

            return Translate_MethodReplacement(
                method, new JSDotExpression(thisExpression, methodName), 
                translatedArguments
            );
        }

        protected JSExpression Translate_Callvirt (ILExpression node, MethodReference method) {
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);
            JSExpression thisExpression;

            var methodDef = method.Resolve();

            if (DereferenceType(firstArg.InferredType).IsValueType) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression))
                    throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
            } else {
                thisExpression = translated;
            }

            var translatedArguments = Translate(node.Arguments.Skip(1), method.Parameters);

            if (methodDef != null) {
                JSExpression propertyResult;
                if (Translate_PropertyCall(thisExpression, methodDef, translatedArguments, out propertyResult))
                    return propertyResult;
            }

            return Translate_MethodReplacement(
               method, new JSDotExpression(thisExpression, new JSMethod(method)),
               translatedArguments
            );
        }

        protected JSExpression Translate_InvokeCallSiteTarget (ILExpression node, MethodReference method) {
            ILExpression ldtarget, ldcallsite;
            
            ldtarget = node.Arguments[0];
            if (ldtarget.Code == ILCode.Ldloc) {
                ldcallsite = node.Arguments[1];
            } else if (ldtarget.Code == ILCode.Ldfld) {
                ldcallsite = ldtarget.Arguments[0];
            } else {
                throw new NotImplementedException("Unknown call site pattern");
            }

            DynamicCallSiteInfo callSite;

            if (ldcallsite.Code == ILCode.Ldloc) {
                if (!DynamicCallSites.Get((ILVariable)ldcallsite.Operand, out callSite))
                    throw new InvalidOperationException("Invalid call site invocation");
            } else if (ldcallsite.Code == ILCode.GetCallSite) {
                if (!DynamicCallSites.Get((FieldReference)ldcallsite.Operand, out callSite))
                    throw new InvalidOperationException("Invalid call site invocation");
            } else {
                throw new NotImplementedException("Unknown call site pattern");
            }

            var invocationArguments = Translate(node.Arguments.Skip(1));
            return callSite.Translate(this, invocationArguments);

            /*
            var cond = condition.Condition;
            if (
                (cond.Code == ILCode.LogicNot) &&
                (cond.Arguments.Count > 0) &&
                (cond.Arguments[0].Code == ILCode.GetCallSite) &&
                (condition.TrueBlock != null) &&
                (condition.TrueBlock.Body.Count == 1) &&
                (condition.TrueBlock.Body[0] is ILExpression)
            ) {
                var callSiteExpression = (ILExpression)condition.TrueBlock.Body[0];
                var binderExpression = callSiteExpression.Arguments[0].Arguments[0];
                var binderMethod = (MethodReference)binderExpression.Operand;
                var arguments = Translate(binderExpression.Arguments);

                DynamicCallSites.InitializeCallSite(
                    (FieldReference)cond.Arguments[0].Operand,
                    binderMethod.Name,
                    arguments
                );

                result = new JSNullStatement();
                return true;
            }
             */

            return null;
        }

        protected JSExpression Translate_GetCallSite (ILExpression node, FieldReference field) {
            return new JSNullExpression();
        }

        protected JSExpression Translate_CreateCallSite (ILExpression node, FieldReference field) {
            return new JSNullExpression();
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
                JSIL, TranslateNode(node.Arguments[0]), out target
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
