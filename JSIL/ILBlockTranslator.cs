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
        public readonly AssemblyTranslator Translator;
        public readonly DecompilerContext Context;
        public readonly MethodDefinition ThisMethod;
        public readonly ILBlock Block;
        public readonly JavascriptFormatter Output = null;

        public readonly HashSet<string> ParameterNames = new HashSet<string>();
        public readonly Dictionary<string, JSVariable> Variables = new Dictionary<string, JSVariable>();
        public readonly DynamicCallSiteInfoCollection DynamicCallSites = new DynamicCallSiteInfoCollection();

        protected readonly Dictionary<ILVariable, JSVariable> RenamedVariables = new Dictionary<ILVariable, JSVariable>();

        public readonly JSILIdentifier JSIL;
        public readonly JSSpecialIdentifiers JS;
        public readonly CLRSpecialIdentifiers CLR;

        protected int LabelledBlockCount = 0;
        protected int UnlabelledBlockCount = 0;

        protected readonly Stack<JSStatement> Blocks = new Stack<JSStatement>();

        public ILBlockTranslator (AssemblyTranslator translator, DecompilerContext context, MethodDefinition method, ILBlock ilb, IEnumerable<ILVariable> parameters, IEnumerable<ILVariable> allVariables) {
            Translator = translator;
            Context = context;
            ThisMethod = method;
            Block = ilb;

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

            foreach (var variable in allVariables) {
                var v = JSVariable.New(variable);
                if (Variables.ContainsKey(v.Identifier)) {
                    v = new JSVariable(variable.OriginalVariable.Name, variable.Type);
                    RenamedVariables[variable] = v;
                    Variables.Add(v.Identifier, v);
                } else {
                    Variables.Add(v.Identifier, v);
                }
            }
        }

        public ITypeInfoSource TypeInfo {
            get {
                return Translator;
            }
        }

        public TypeSystem TypeSystem {
            get {
                return Context.CurrentModule.TypeSystem;
            }
        }

        public JSBlockStatement Translate () {
            try {
                return TranslateNode(Block);
            } catch (AbortTranslation) {
                return null;
            }
        }

        public JSNode TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node        NYI: {0}", node.GetType().Name);

            return new JSUntranslatableStatement(node.GetType().Name);
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

        protected JSVariable DeclareVariable (ILVariable variable) {
            var result = JSVariable.New(variable);

            JSVariable existing;
            if (Variables.TryGetValue(result.Identifier, out existing)) {
                if (result.Type != existing.Type)
                    throw new InvalidOperationException("A variable with that name is already declared in this scope, with a different type.");

                return existing;
            }

            Variables[result.Identifier] = result;
            return result;
        }

        protected static bool CopyOnReturn (TypeReference type) {
            return EmulateStructAssignment.IsStruct(type) || type.IsValueType;
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

        protected JSExpression Translate_UnaryOp (ILExpression node, JSUnaryOperator op) {
            var inner = TranslateNode(node.Arguments[0]);
            var innerType = inner.GetExpectedType(TypeSystem);

            // Detect the weird pattern '!(x = y as z)' and transform it into '(x = y as z) != null'
            if (
                (op == JSOperator.LogicalNot) && 
                !TypesAreEqual(TypeSystem.Boolean, innerType)
            ) {
                return new JSBinaryOperatorExpression(
                    JSOperator.NotEqual, inner, new JSDefaultValueLiteral(innerType), TypeSystem.Boolean
                );
            }

            return new JSUnaryOperatorExpression(
                op, inner, node.ExpectedType ?? node.InferredType
            );
        }

        protected JSExpression Translate_BinaryOp (ILExpression node, JSBinaryOperator op) {
            var lhs = TranslateNode(node.Arguments[0]);
            var rhs = TranslateNode(node.Arguments[1]);

            var boeLeft = lhs as JSBinaryOperatorExpression;
            if (
                (op is JSAssignmentOperator) &&
                (boeLeft != null) && !(boeLeft.Operator is JSAssignmentOperator)
            )
                return new JSUntranslatableExpression(node);

            return new JSBinaryOperatorExpression(
                op, lhs, rhs, node.ExpectedType ?? node.InferredType
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

        protected JSExpression Translate_MethodReplacement (MethodReference method, JSExpression thisExpression, JSExpression methodExpression, JSExpression[] arguments, bool virt) {
            var methodInfo = TypeInfo.GetMethod(method);
            if (methodInfo != null) {
                var metadata = methodInfo.Metadata;

                if (metadata != null) {
                    var parms = metadata.GetAttributeParameters("JSIL.Meta.JSReplacement");
                    if (parms != null)
                        return new JSVerbatimLiteral(
                            (string)parms[0].Value, thisExpression, method.ReturnType
                        );
                }
            }

            if (methodInfo.IsIgnored)
                return Translate_IgnoredMethod(method.Name, method.Parameters);

            switch (method.FullName) {
                case "System.Object JSIL.Builtins::Eval(System.String)":
                    methodExpression = JS.eval;
                break;
                case "System.Object JSIL.Verbatim::Expression(System.String)": {
                    var expression = arguments[0] as JSStringLiteral;
                    if (expression == null)
                        throw new InvalidOperationException("JSIL.Verbatim.Expression must recieve a string literal as an argument");

                    return new JSVerbatimLiteral(
                        expression.Value, null
                    );
                }
                case "System.Object JSIL.JSGlobal::get_Item(System.String)": {
                    var expression = arguments[0] as JSStringLiteral;
                    if (expression != null)
                        return new JSDotExpression(
                            JSIL.GlobalNamespace, new JSStringIdentifier(expression.Value, TypeSystem.Object)
                        );
                    else
                        return new JSIndexerExpression(
                            JSIL.GlobalNamespace, arguments[0], TypeSystem.Object
                        );
                }
                case "System.Object JSIL.JSLocal::get_Item(System.String)": {
                    var expression = arguments[0] as JSStringLiteral;
                    if (expression == null)
                        throw new InvalidOperationException("JSLocal must recieve a string literal as an index");

                    return new JSStringIdentifier(expression.Value, TypeSystem.Object);
                }
                case "System.Object JSIL.Builtins::get_This()":
                    return Variables["this"];
            }

            JSExpression result;

            var methodDef = method.Resolve();
            JSExpression propertyResult;
            if (
                (methodDef != null) &&
                Translate_PropertyCall(thisExpression, methodDef, arguments, virt, out propertyResult)
            ) {
                result = propertyResult;
            } else {
                result = new JSInvocationExpression(
                    methodExpression, arguments
                );
            }

            if (CopyOnReturn(method.ReturnType))
                result = JSReferenceExpression.New(result);

            return result;
        }

        protected bool Translate_PropertyCall (JSExpression thisExpression, MethodDefinition method, JSExpression[] arguments, bool virt, out JSExpression result) {
            result = null;

            var methodInfo = TypeInfo.GetMethod(method);
            if (methodInfo == null)
                return false;

            var propertyInfo = methodInfo.DeclaringProperty;
            if (propertyInfo == null)
                return false;

            if (propertyInfo.IsIgnored) {
                result = new JSInvocationExpression(
                    JSIL.IgnoredMember, JSLiteral.New(propertyInfo.Name)
                );
                return true;
            }

            // JS provides no way to override [], so keep it as a regular method call
            if (propertyInfo.Member.IsIndexer())
                return false;

            var parms = methodInfo.Metadata.GetAttributeParameters("JSIL.Meta.JSReplacement") ??
                propertyInfo.Metadata.GetAttributeParameters("JSIL.Meta.JSReplacement");
            if (parms != null) {
                result = new JSVerbatimLiteral((string)parms[0].Value, thisExpression, propertyInfo.Type);
                return true;
            }

            var thisType = thisExpression.GetExpectedType(TypeSystem);
            Func<JSExpression> generate = () => {
                if ((propertyInfo.Member.GetMethod != null) && (method.FullName == propertyInfo.Member.GetMethod.FullName)) {
                    return new JSDotExpression(
                        thisExpression, new JSProperty(propertyInfo)
                    );
                } else {
                    if (arguments.Length == 0)
                        throw new InvalidOperationException("Attempting to invoke a property setter with no arguments");

                    return new JSBinaryOperatorExpression(
                        JSOperator.Assignment,
                        new JSDotExpression(
                            thisExpression, new JSProperty(propertyInfo)
                        ),
                        arguments[0], propertyInfo.Type
                    );
                }
            };

            // Accesses to a base property should go through a regular method invocation, since
            //  javascript properties do not have a mechanism for base access
            if (method.HasThis) {
                if (AllBaseTypesOf(GetTypeDefinition(thisType)).Contains(propertyInfo.DeclaringType.Definition)) {
                    return false;
                } else {
                    result = generate();
                    return true;
                }
            }

            result = generate();
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

        public static TypeDefinition GetTypeDefinition (TypeReference typeRef) {
            if (typeRef == null)
                return null;

            var ts = typeRef.Module.TypeSystem;
            typeRef = DereferenceType(typeRef);

            if (typeRef.IsGenericParameter)
                return null;
            else if (typeRef is ArrayType)
                return new TypeReference(ts.Object.Namespace, "Array", ts.Object.Module, ts.Object.Scope).ResolveOrThrow();
            else
                return typeRef.ResolveOrThrow();
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
            type = DereferenceType(type);

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
            type = DereferenceType(type);

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
            type = DereferenceType(type);

            var typedef = GetTypeDefinition(type);
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
            if ((target == null) || (source == null))
                return (target == source);

            if (target.IsByReference != source.IsByReference)
                return false;
            if (target.IsPointer != source.IsPointer)
                return false;
            if (target.IsGenericParameter != source.IsGenericParameter)
                return false;
            if (target.IsArray != source.IsArray)
                return false;

            var dTarget = GetTypeDefinition(target);
            var dSource = GetTypeDefinition(source);

            if (Object.Equals(dTarget, dSource) && (dSource != null))
                return true;
            if (Object.Equals(target, source))
                return true;

            if (String.Equals(target.FullName, source.FullName))
                return true;

            return false;
        }

        public static IEnumerable<TypeDefinition> AllBaseTypesOf (TypeDefinition type) {
            if (type == null)
                yield break;

            var baseType = GetTypeDefinition(type.BaseType);

            while (baseType != null) {
                yield return baseType;

                baseType = GetTypeDefinition(baseType.BaseType);
            }
        }

        public static bool TypesAreAssignable (TypeReference target, TypeReference source) {
            if (TypesAreEqual(target, source))
                return true;

            var dSource = GetTypeDefinition(source);
            if (TypesAreEqual(target, dSource))
                return true;

            if ((dSource.BaseType != null) && TypesAreAssignable(target, dSource.BaseType))
                return true;

            foreach (var iface in dSource.Interfaces) {
                if (TypesAreAssignable(target, iface))
                    return true;
            }

            return false;
        }

        protected bool ContainsLabels (ILNode root) {
            var labels = root.GetSelfAndChildrenRecursive<ILLabel>();
            return labels.Count() > 0;
        }


        //
        // IL Node Types
        //

        protected JSBlockStatement TranslateBlock (IEnumerable<ILNode> children) {
            JSBlockStatement result, currentBlock;

            // TODO: Fix this heuristic by building a flow graph at the beginning of method translation
            if (children.Any(
                n => ContainsLabels(n)
            )) {
                var index = LabelledBlockCount++;
                result = new JSLabelGroupStatement(index);

                currentBlock = new JSBlockStatement();
                currentBlock.Label = String.Format("__entry{0}__", index);
                result.Statements.Add(currentBlock);
            } else {
                currentBlock = result = new JSBlockStatement();
            }

            foreach (var node in children) {
                var label = node as ILLabel;
                var expr = node as ILExpression;
                var isGoto = (expr != null) && (expr.Code == ILCode.Br);

                if (label != null) {
                    currentBlock = new JSBlockStatement {
                        Label = label.Name
                    };
                    result.Statements.Add(currentBlock);

                    continue;
                } else if (isGoto) {
                    currentBlock.Statements.Add(new JSExpressionStatement(new JSGotoExpression(
                        ((ILLabel)expr.Operand).Name
                    )));
                } else {
                    var translated = TranslateStatement(node);
                    if (translated != null)
                        currentBlock.Statements.Add(translated);
                }
            }

            return result;
        }

        protected JSStatement TranslateStatement (ILNode node) {
            var translated = TranslateNode(node as dynamic);

            var statement = translated as JSStatement;
            if (statement == null) {
                var expression = (JSExpression)translated;

                if (expression != null)
                    statement = new JSExpressionStatement(expression);
                else
                    Console.Error.WriteLine("Warning: Null statement: {0}", node);
            }

            return statement;
        }

        public JSBlockStatement TranslateNode (ILBlock block) {
            return TranslateBlock(block.GetChildren());
        }

        public JSExpression TranslateNode (ILFixedStatement fxd) {
            throw new AbortTranslation();
        }

        public JSExpression TranslateNode (ILExpression expression) {
            JSExpression result = null;

            var type = expression.ExpectedType ?? expression.InferredType;
            if ((type != null) && (type.IsPointer || type.IsPinned || type.IsFunctionPointer))
                return new JSUntranslatableExpression(expression);

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
                    Console.Error.WriteLine(String.Format("Instruction {0} did not produce a JS AST expression", expression));
            } catch (MissingMethodException) {
                string operandType = "";
                if (expression.Operand != null)
                    operandType = expression.Operand.GetType().FullName;

                Console.Error.WriteLine("Instruction NYI: {0} {1}", expression.Code, operandType);
                return new JSUntranslatableExpression(expression);
            } catch (TargetInvocationException tie) {
                if (tie.InnerException is AbortTranslation)
                    throw tie.InnerException;

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
            var result = new JSSwitchStatement(
                TranslateNode(swtch.Condition)
            );

            Blocks.Push(result);

            result.Cases.AddRange(
                (from cb in swtch.CaseBlocks select TranslateNode(cb))
            );

            Blocks.Pop();

            return result;
        }

        public JSTryCatchBlock TranslateNode (ILTryCatchBlock tcb) {
            var body = TranslateNode(tcb.TryBlock);
            JSVariable catchVariable = null;
            JSBlockStatement catchBlock = null;
            JSBlockStatement finallyBlock = null;

            if (tcb.CatchBlocks.Count > 0) {
                var pairs = new List<KeyValuePair<JSExpression, JSStatement>>();
                catchVariable = DeclareVariable(new ILVariable {
                    IsGenerated = true,
                    Name = "$exception", 
                    Type = Context.CurrentModule.TypeSystem.Object
                });

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
                            Console.Error.WriteLine("Found multiple catch-all catch clauses. Any after the first will be ignored.");
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
                        var excVariable = DeclareVariable(cb.ExceptionVariable);

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
                Console.Error.WriteLine("Warning: Fault blocks are not translatable.");
                body.Statements.Add(new JSUntranslatableStatement("Fault Block"));
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

            var result = new JSWhileLoop(condition);
            result.Label = String.Format("__while{0}__", UnlabelledBlockCount++);
            Blocks.Push(result);

            var body = TranslateNode(loop.BodyBlock);

            Blocks.Pop();
            result.Statements.Add(body);
            return result;
        }


        //
        // MSIL Instructions
        //

        protected JSExpression Translate_Clt (ILExpression node) {
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
                var value = TranslateNode(node.Arguments[0].Arguments[0]);
                var targetType = (TypeReference)node.Arguments[0].Operand;

                if (targetType.IsGenericParameter)
                    return JSChangeTypeExpression.New(Translate_GenericTypeCast(targetType), TypeSystem, TypeSystem.Boolean);

                var targetInfo = TypeInfo.Get(targetType);

                if (targetInfo.IsIgnored)
                    return JSLiteral.New(false);
                else
                    return JSIL.CheckType(
                        value, targetType
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
                node.ExpectedType ?? node.InferredType
            );
        }

        protected JSExpression Translate_Mul (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Multiply);
        }

        protected JSExpression Translate_Div (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Divide);
        }

        protected JSExpression Translate_Rem (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Remainder);
        }

        protected JSExpression Translate_Add (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Add);
        }

        protected JSExpression Translate_Sub (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.Subtract);
        }

        protected JSExpression Translate_Shl (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.ShiftLeft);
        }

        protected JSExpression Translate_Shr (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.ShiftRight);
        }

        protected JSExpression Translate_Shr_Un (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.ShiftRightUnsigned);
        }

        protected JSExpression Translate_And (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.BitwiseAnd);
        }

        protected JSExpression Translate_Or (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.BitwiseOr);
        }

        protected JSExpression Translate_Xor (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.BitwiseXor);
        }

        protected JSExpression Translate_Not (ILExpression node) {
            return Translate_UnaryOp(node, JSOperator.BitwiseNot);
        }

        protected JSExpression Translate_LogicOr (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.LogicalOr);
        }

        protected JSExpression Translate_LogicAnd (ILExpression node) {
            return Translate_BinaryOp(node, JSOperator.LogicalAnd);
        }

        protected JSExpression Translate_LogicNot (ILExpression node) {
            return Translate_UnaryOp(node, JSOperator.LogicalNot);
        }

        protected JSExpression Translate_Neg (ILExpression node) {
            return Translate_UnaryOp(node, JSOperator.Negation);
        }

        protected JSThrowExpression Translate_Rethrow (ILExpression node) {
            return new JSThrowExpression(new JSStringIdentifier("$exception", TypeSystem.Object));
        }

        protected JSThrowExpression Translate_Throw (ILExpression node) {
            return new JSThrowExpression(TranslateNode(node.Arguments[0]));
        }

        protected JSExpression Translate_Endfinally (ILExpression node) {
            return JSExpression.Null;
        }

        protected JSBreakExpression Translate_LoopOrSwitchBreak (ILExpression node) {
            var result = new JSBreakExpression();

            if (Blocks.Count > 0)
                result.TargetLabel = Blocks.Peek().Label;

            return result;
        }

        protected JSContinueExpression Translate_LoopContinue (ILExpression node) {
            var result = new JSContinueExpression();

            if (Blocks.Count > 0)
                result.TargetLabel = Blocks.Peek().Label;

            return result;
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
            JSVariable renamed;
            if (RenamedVariables.TryGetValue(variable, out renamed))
                return new JSIndirectVariable(Variables, renamed.Identifier);
            else
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
            if ((value.IsNull) && !(value is JSUntranslatableExpression))
                return new JSNullExpression();

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment, Translate_Ldloc(node, variable),
                value,
                value.GetExpectedType(TypeSystem)
            );
        }

        protected JSExpression Translate_Ldsfld (ILExpression node, FieldReference field) {
            var fieldInfo = TypeInfo.GetField(field);
            if (fieldInfo.IsIgnored)
                return new JSInvocationExpression(
                    JSIL.IgnoredMember, JSLiteral.New(field.Name)
                );

            JSExpression result = new JSDotExpression(
                new JSType(field.DeclaringType),
                new JSField(fieldInfo)
            );

            if (CopyOnReturn(field.FieldType))
                result = JSReferenceExpression.New(result);

            // TODO: When returning a value type we should be returning it by reference, but doing that would break Ldflda.
            return result;
        }

        protected JSExpression Translate_Ldsflda (ILExpression node, FieldReference field) {
            return new JSMemberReferenceExpression(
                Translate_Ldsfld(node, field)
            );
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
            if ((translated.IsNull) && !(translated is JSUntranslatableExpression))
                return new JSNullExpression();

            var fieldInfo = TypeInfo.GetField(field);
            if (fieldInfo.IsIgnored)
                return new JSInvocationExpression(
                    JSIL.IgnoredMember, JSLiteral.New(field.Name)
                );

            JSExpression thisExpression;
            if (DereferenceType(firstArg.InferredType).IsValueType) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression)) {
                    Console.Error.WriteLine("Warning: Accessing {0} without a reference as this.", field.FullName);
                    thisExpression = translated;
                }
            } else {
                thisExpression = translated;
            }

            JSExpression result = new JSDotExpression(
                thisExpression,
                new JSField(fieldInfo)
            );

            if (CopyOnReturn(field.FieldType))
                result = JSReferenceExpression.New(result);

            return result;
        }

        protected JSExpression Translate_Stind (ILExpression node) {
            return new JSUntranslatableExpression(node);
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
                Console.Error.WriteLine(String.Format("Warning: unsupported reference type for ldobj: {0}", node.Arguments[0]));

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

        protected JSExpression Translate_Arglist (ILExpression node) {
            throw new AbortTranslation();
        }

        protected JSExpression Translate_Localloc (ILExpression node) {
            throw new AbortTranslation();
        }

        protected JSStringLiteral Translate_Ldstr (ILExpression node, string text) {
            return JSLiteral.New(text);
        }

        protected JSExpression Translate_Ldnull (ILExpression node) {
            return JSLiteral.Null(node.ExpectedType ?? node.InferredType);
        }

        protected JSExpression Translate_Ldftn (ILExpression node, MethodReference method) {
            var methodInfo = TypeInfo.GetMethod(method);

            if (method.HasThis)
                return JSDotExpression.New(
                    new JSType(method.DeclaringType),
                    JS.prototype,
                    new JSMethod(method, methodInfo)
                );
            else
                return new JSDotExpression(
                    new JSType(method.DeclaringType),
                    new JSMethod(method, methodInfo)
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

        protected JSExpression Translate_Ldlen (ILExpression node) {
            var arg = TranslateNode(node.Arguments[0]);
            var argType = GetTypeDefinition(arg.GetExpectedType(TypeSystem));
            var lengthProp = (from p in argType.Properties where p.Name == "Length" select p).First();
            return Translate_CallGetter(node, lengthProp.GetMethod);
        }

        protected JSExpression Translate_Ldelem (ILExpression node, TypeReference elementType) {
            var expectedType = elementType ?? node.ExpectedType ?? node.InferredType;

            JSExpression result = new JSIndexerExpression(
                TranslateNode(node.Arguments[0]),
                TranslateNode(node.Arguments[1]),
                expectedType
            );

            if (CopyOnReturn(expectedType))
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
            if (IsDelegateType(targetType) && IsDelegateType(node.ExpectedType ?? node.InferredType)) {
                // TODO: We treat all delegate types as equivalent, so we can skip these casts for now
                return TranslateNode(node.Arguments[0]);
            }

            return JSIL.Cast(
                TranslateNode(node.Arguments[0]),
                targetType
            );
        }

        protected JSExpression Translate_GenericTypeCast (TypeReference targetType) {
            return new JSUntranslatableExpression(String.Format("Cast to generic parameter type '{0}'", targetType.FullName));
        }

        protected JSExpression Translate_Isinst (ILExpression node, TypeReference targetType) {
            var firstArg = TranslateNode(node.Arguments[0]);
            if (targetType.IsGenericParameter)
                return JSChangeTypeExpression.New(Translate_GenericTypeCast(targetType), TypeSystem, targetType);

            var targetInfo = TypeInfo.Get(targetType);
            if (targetInfo.IsIgnored)
                return new JSNullLiteral(targetType);
            else
                return JSIL.TryCast(firstArg, targetType);
        }

        protected JSExpression Translate_Unbox_Any (ILExpression node, TypeReference targetType) {
            var value = TranslateNode(node.Arguments[0]);
            var result = JSIL.Cast(value, JSExpression.ResolveGenericType(targetType, ThisMethod, ThisMethod.DeclaringType));

            if (CopyOnReturn(targetType))
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

        protected JSExpression Translate_Conv_Ovf_U8 (ILExpression node) {
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

        protected JSExpression Translate_Conv_Ovf_I4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int32);
        }

        protected JSExpression Translate_Conv_I8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int64);
        }

        protected JSExpression Translate_Conv_Ovf_I8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int64);
        }

        protected JSExpression Translate_Conv_R4 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Single);
        }

        protected JSExpression Translate_Conv_R8 (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Double);
        }

        protected JSExpression Translate_Conv_R_Un (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Double);
        }

        protected JSExpression Translate_Box (ILExpression node, TypeReference valueType) {
            return JSReferenceExpression.New(TranslateNode(node.Arguments[0]));
        }

        protected JSExpression Translate_Br (ILExpression node, ILLabel targetLabel) {
            return new JSGotoExpression(targetLabel.Name);
        }

        protected JSExpression Translate_Leave (ILExpression node, ILLabel targetLabel) {
            return new JSGotoExpression(targetLabel.Name);
        }

        protected JSExpression Translate_Newobj (ILExpression node, MethodReference constructor) {
            if (IsDelegateType(constructor.DeclaringType)) {
                var thisArg = TranslateNode(node.Arguments[0]);
                var methodRef = TranslateNode(node.Arguments[1]);

                var methodDot = methodRef as JSDotExpression;

                // Detect compiler-generated lambda methods
                if (methodDot != null) {
                    var methodMember = methodDot.Member as JSMethod;

                    if (methodMember != null) {
                        var methodDef = methodMember.Method.Member;
                        if (
                            methodDef.IsPrivate && 
                            methodDef.IsCompilerGenerated()
                        ) {
                            // Lambda with no closed-over values

                            return Translator.TranslateMethod(
                                Context, methodDef
                            );
                        } else if (
                            methodDef.DeclaringType.IsCompilerGenerated() &&
                            TypesAreEqual(
                                thisArg.GetExpectedType(TypeSystem),
                                methodDef.DeclaringType
                            )
                        ) {
                            // Lambda with closed-over values
                            var function = Translator.TranslateMethod(
                                Context, methodDef
                            );

                            new VariableEliminator(
                                function.AllVariables["this"],
                                thisArg
                            ).Visit(function);
                            function.AllVariables.Remove("this");
                                
                            return function;
                        }
                    }
                }

                return JSIL.NewDelegate(
                    constructor.DeclaringType,
                    thisArg, methodRef
                );
            } else if (constructor.DeclaringType.IsArray) {
                return JSIL.NewMultidimensionalArray(
                    constructor.DeclaringType.GetElementType(),
                    Translate(node.Arguments)
                );
            }

            var methodInfo = TypeInfo.GetMethod(constructor);
            if (methodInfo.IsIgnored)
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

        protected JSExpression Translate_InitArray (ILExpression node, TypeReference elementType) {
            var initializer = new JSArrayExpression(elementType, Translate(node.Arguments));

            if (TypesAreEqual(
                TypeSystem.Object, elementType
            ))
                return initializer;
            else
                return JSIL.NewArray(
                    elementType, initializer
                );
        }

        protected JSExpression Translate_InitializedObject (ILExpression node) {
            // This should get eliminated by the handler for InitObject, but if we just return a null expression here,
            //  stfld treats us as an invalid assignment target.
            return new JSUntranslatableExpression(node.Code);
        }

        protected JSExpression Translate_InitCollection (ILExpression node) {
            TypeReference inferredType = null;
            var values = new List<JSExpression>();

            for (var i = 1; i < node.Arguments.Count; i++) {
                var translated = TranslateNode(node.Arguments[i]);

                while (translated is JSReferenceExpression)
                    translated = ((JSReferenceExpression)translated).Referent;

                var invocation = (JSInvocationExpression)translated;

                var valueType = invocation.Arguments[0].GetExpectedType(TypeSystem);

                if (inferredType == null)
                    inferredType = valueType;
                else if (inferredType.FullName != valueType.FullName)
                    Console.Error.WriteLine("Mixed-type collection initializers not supported: {0}", node);

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
            var target = TranslateNode(node.Arguments[0]);
            var typeInfo = TypeInfo.Get(target.GetExpectedType(TypeSystem));

            var initializers = new List<JSPairExpression>();

            for (var i = 1; i < node.Arguments.Count; i++) {
                var translated = TranslateNode(node.Arguments[i]);

                while (translated is JSReferenceExpression)
                    translated = ((JSReferenceExpression)translated).Referent;

                var boe = translated as JSBinaryOperatorExpression;
                var ie = translated as JSInvocationExpression;

                if (boe != null) {
                    var left = boe.Left;

                    while (left is JSReferenceExpression)
                        left = ((JSReferenceExpression)left).Referent;

                    var leftDot = left as JSDotExpression;

                    if (leftDot != null) {
                        var key = leftDot.Member;
                        var value = boe.Right;

                        initializers.Add(new JSPairExpression(key, value));
                    } else {
                        Console.Error.WriteLine(String.Format("Warning: Unrecognized object initializer target: {0}", left));
                    }
                } else if (ie != null) {
                    var method = ie.Target.AllChildrenRecursive.OfType<JSMethod>().FirstOrDefault();

                    if (
                        (method != null) && (method.Method.DeclaringProperty != null)
                    ) {
                        initializers.Add(new JSPairExpression(
                            new JSProperty(method.Method.DeclaringProperty), ie.Arguments[0]
                        ));
                    } else {
                        Console.Error.WriteLine(String.Format("Warning: Object initializer element not implemented: {0}", translated));
                    }
                } else {
                    Console.Error.WriteLine(String.Format("Warning: Object initializer element not implemented: {0}", translated));
                }
            }

            return new JSInvocationExpression(
                new JSDotExpression(
                    target, new JSStringIdentifier("__Initialize__", target.GetExpectedType(TypeSystem))
                ),
                new JSObjectExpression(initializers.ToArray())
            );
        }

        protected JSExpression Translate_Ldtoken (ILExpression node, TypeReference type) {
            return new JSType(type);
        }

        protected JSExpression Translate_Ldtoken (ILExpression node, MethodReference method) {
            var methodInfo = TypeInfo.GetMethod(method);
            return new JSMethod(method, methodInfo);
        }

        protected JSExpression Translate_Ldtoken (ILExpression node, FieldReference field) {
            var fieldInfo = TypeInfo.GetField(field);
            return new JSField(fieldInfo);
        }

        protected JSExpression Translate_Call (ILExpression node, MethodReference method) {
            var methodInfo = TypeInfo.GetMethod(method);
            var thisType = DereferenceType(ThisMethod.DeclaringType);
            var declaringType = DereferenceType(method.DeclaringType);

            var declaringTypeDef = GetTypeDefinition(declaringType);

            IEnumerable<ILExpression> arguments = node.Arguments;
            JSExpression thisExpression;
            JSExpression invokeTarget;

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
                    arguments = arguments.Skip(1);
                    invokeTarget = JSDotExpression.New(thisExpression, new JSMethod(method, methodInfo));
                } else {
                    invokeTarget = JSDotExpression.New(new JSType(method.DeclaringType), JS.prototype, new JSMethod(method, methodInfo), JS.call(method.ReturnType));
                }
            } else {
                thisExpression = new JSType(method.DeclaringType);
                invokeTarget = JSDotExpression.New(thisExpression, new JSMethod(method, methodInfo));
            }

            var translatedArguments = Translate(arguments.ToArray(), method.Parameters);

            return Translate_MethodReplacement(
                method, thisExpression, invokeTarget, 
                translatedArguments, false
            );
        }

        protected JSExpression Translate_Callvirt (ILExpression node, MethodReference method) {
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);
            JSExpression thisExpression;

            if (DereferenceType(firstArg.InferredType).IsValueType) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression))
                    throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
            } else {
                thisExpression = translated;
            }

            var translatedArguments = Translate(node.Arguments.Skip(1), method.Parameters);
            var methodInfo = TypeInfo.GetMethod(method);

            return Translate_MethodReplacement(
               method, thisExpression, new JSDotExpression(thisExpression, new JSMethod(method, methodInfo)),
               translatedArguments, true
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

    public class AbortTranslation : Exception {
    }
}
