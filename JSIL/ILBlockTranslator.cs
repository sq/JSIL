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
        public readonly MethodReference ThisMethodReference;
        public readonly MethodDefinition ThisMethod;
        public readonly ILBlock Block;
        public readonly JavascriptFormatter Output = null;

        public readonly HashSet<string> ParameterNames = new HashSet<string>();
        public readonly Dictionary<string, JSVariable> Variables = new Dictionary<string, JSVariable>();
        public readonly DynamicCallSiteInfoCollection DynamicCallSites = new DynamicCallSiteInfoCollection();

        protected readonly Dictionary<ILVariable, JSVariable> RenamedVariables = new Dictionary<ILVariable, JSVariable>();

        // Yuck :(
        static readonly ConcurrentCache<Tuple<string, string>, bool> TypeAssignabilityCache = new ConcurrentCache<Tuple<string, string>, bool>();

        public readonly SpecialIdentifiers SpecialIdentifiers;

        protected int UnlabelledBlockCount = 0;

        protected readonly Stack<JSStatement> Blocks = new Stack<JSStatement>();

        public ILBlockTranslator (AssemblyTranslator translator, DecompilerContext context, MethodReference methodReference, MethodDefinition methodDefinition, ILBlock ilb, IEnumerable<ILVariable> parameters, IEnumerable<ILVariable> allVariables) {
            Translator = translator;
            Context = context;
            ThisMethodReference = methodReference;
            ThisMethod = methodDefinition;
            Block = ilb;

            SpecialIdentifiers = new JSIL.SpecialIdentifiers(TypeSystem);

            if (methodReference.HasThis)
                Variables.Add("this", JSThisParameter.New(methodReference.DeclaringType, methodReference));

            foreach (var parameter in parameters) {
                if ((parameter.Name == "this") && (parameter.OriginalParameter.Index == -1))
                    continue;

                ParameterNames.Add(parameter.Name);
                Variables.Add(parameter.Name, new JSParameter(parameter.Name, parameter.Type, methodReference));
            }

            foreach (var variable in allVariables) {
                var v = JSVariable.New(variable, methodReference);
                if (Variables.ContainsKey(v.Identifier)) {
                    v = new JSVariable(variable.OriginalVariable.Name, variable.Type, methodReference);
                    RenamedVariables[variable] = v;
                    Variables.Add(v.Identifier, v);
                } else {
                    Variables.Add(v.Identifier, v);
                }
            }
        }

        protected JSSpecialIdentifiers JS {
            get {
                return SpecialIdentifiers.JS;
            }
        }

        protected JSILIdentifier JSIL {
            get {
                return SpecialIdentifiers.JSIL;
            }
        }

        public ITypeInfoSource TypeInfo {
            get {
                return Translator.TypeInfoProvider;
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
            } catch (AbortTranslation at) {
                Console.Error.WriteLine("Method {0} not translated: {1}", ThisMethod.Name, at.Message);
                return null;
            }
        }

        public JSNode TranslateNode (ILNode node) {
            Console.Error.WriteLine("Node        NYI: {0}", node.GetType().Name);

            return new JSUntranslatableStatement(node.GetType().Name);
        }

        public JSExpression[] Translate (IList<ILExpression> values, IList<ParameterDefinition> parameters, bool hasThis) {
            var result = new List<JSExpression>();
            ParameterDefinition parameter;

            for (int i = 0, c = values.Count; i < c; i++) {
                var value = values[i];

                var parameterIndex = i;
                if (hasThis)
                    parameterIndex -= 1;

                if ((parameterIndex < parameters.Count) && (parameterIndex >= 0))
                    parameter = parameters[parameterIndex];
                else
                    parameter = null;

                var translated = TranslateNode(value);

                if ((parameter != null) && (parameter.ParameterType is ByReferenceType)) {
                    result.Add(new JSPassByReferenceExpression(translated));
                } else
                    result.Add(translated);
            }


            if (result.Any((je) => je == null))
                throw new InvalidOperationException();

            return result.ToArray();
        }

        public JSExpression[] Translate (IEnumerable<ILExpression> values) {
            var result = (
                from value in values select TranslateNode(value)
            ).ToArray();

            if (result.Any((je) => je == null))
                throw new InvalidOperationException();

            return result;
        }

        public static JSVariable[] Translate (IEnumerable<ParameterDefinition> parameters, MethodReference function) {
            return (
                from p in parameters select JSVariable.New(p, function)
            ).ToArray();
        }

        protected JSVariable DeclareVariable (ILVariable variable, MethodReference function) {
            return DeclareVariable(JSVariable.New(variable, function));
        }

        protected JSVariable DeclareVariable (JSVariable variable) {
            JSVariable existing;
            if (Variables.TryGetValue(variable.Identifier, out existing)) {
                if (!TypesAreEqual(variable.Type, existing.Type))
                    throw new InvalidOperationException("A variable with that name is already declared in this scope, with a different type.");
                if (!variable.DefaultValue.Equals(existing.DefaultValue))
                    throw new InvalidOperationException("A variable with that name is already declared in this scope, with a different default value.");

                return existing;
            }

            Variables[variable.Identifier] = variable;

            return variable;
        }

        protected static bool CopyOnReturn (TypeReference type) {
            return EmulateStructAssignment.IsStruct(type);
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
            var innerType = JSExpression.DeReferenceType(inner.GetExpectedType(TypeSystem));

            // Detect the weird pattern '!(x = y as z)' and transform it into '(x = y as z) != null'
            if (
                (op == JSOperator.LogicalNot) && 
                !TypesAreAssignable(TypeSystem.Boolean, innerType)
            ) {
                return new JSBinaryOperatorExpression(
                    JSOperator.Equal, inner, new JSDefaultValueLiteral(innerType), TypeSystem.Boolean
                );
            }

            // Insert correct casts when unary operators are applied to enums.
            if (IsEnum(innerType) && IsEnum(node.ExpectedType ?? node.InferredType)) {
                return JSCastExpression.New(
                    new JSUnaryOperatorExpression(
                        op,
                        JSCastExpression.New(inner, TypeSystem.Int32, TypeSystem),
                        TypeSystem.Int32
                    ),
                    node.ExpectedType ?? node.InferredType, TypeSystem
                );
            }

            return new JSUnaryOperatorExpression(
                op, inner, node.ExpectedType ?? node.InferredType
            );
        }

        protected JSExpression Translate_BinaryOp (ILExpression node, JSBinaryOperator op) {
            // Detect attempts to perform pointer arithmetic
            if (IsIgnoredType(node.Arguments[0].ExpectedType) ||
                IsIgnoredType(node.Arguments[1].ExpectedType) ||
                IsIgnoredType(node.Arguments[0].InferredType) ||
                IsIgnoredType(node.Arguments[1].InferredType)
            ) {
                return new JSUntranslatableExpression(node);
            }

            // Detect attempts to perform pointer arithmetic on a local variable.
            // (ldloca produces a reference, not a pointer, so the previous check won't catch this.)
            if ((node.Arguments[0].Code == ILCode.Ldloca) &&
                !(op is JSAssignmentOperator))
                return new JSUntranslatableExpression(node);

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

        protected JSExpression Translate_MethodReplacement (
            JSMethod method, JSExpression thisExpression, 
            JSExpression[] arguments, bool @virtual, bool @static, bool explicitThis
        ) {
            var methodInfo = method.Method;
            var metadata = methodInfo.Metadata;

            if (metadata != null) {
                var parms = metadata.GetAttributeParameters("JSIL.Meta.JSReplacement");
                if (parms != null) {
                    var argsDict = new Dictionary<string, JSExpression>();
                    argsDict["this"] = thisExpression;
                    argsDict["typeof(this)"] = new JSType(thisExpression.GetExpectedType(TypeSystem));

                    foreach (var kvp in methodInfo.Parameters.Zip(arguments, (p, v) => new { p.Name, Value = v })) {
                        argsDict.Add(kvp.Name, kvp.Value);
                    }

                    return new JSVerbatimLiteral(
                        method, (string)parms[0].Value, argsDict, method.Method.ReturnType
                    );
                }
            }

            if (methodInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, methodInfo, new[] { thisExpression }.Concat(arguments).ToArray());

            switch (method.Method.Member.FullName) {
                case "System.Object JSIL.Builtins::Eval(System.String)":
                    return JSInvocationExpression.InvokeStatic(
                        JS.eval, arguments
                    );
                case "System.Object JSIL.Verbatim::Expression(System.String)": {
                    var expression = arguments[0] as JSStringLiteral;
                    if (expression == null)
                        throw new InvalidOperationException("JSIL.Verbatim.Expression must recieve a string literal as an argument");

                    return new JSVerbatimLiteral(
                        method, expression.Value, null, null
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
                    return new JSIndirectVariable(Variables, "this", ThisMethodReference);
            }

            JSExpression result = Translate_PropertyCall(thisExpression, method, arguments, @virtual, @static);
            if (result == null) {
                if (@static)
                    result = JSInvocationExpression.InvokeStatic(method.Reference.DeclaringType, method, arguments);
                else if (explicitThis)
                    result = JSInvocationExpression.InvokeBaseMethod(method.Reference.DeclaringType, method, thisExpression, arguments);
                else
                    result = JSInvocationExpression.InvokeMethod(method.Reference.DeclaringType, method, thisExpression, arguments);
            }

            return result;
        }

        protected JSExpression Translate_PropertyCall (JSExpression thisExpression, JSMethod method, JSExpression[] arguments, bool @virtual, bool @static) {
            var propertyInfo = method.Method.DeclaringProperty;
            if (propertyInfo == null)
                return null;

            if (propertyInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, propertyInfo, arguments);

            // JS provides no way to override [], so keep it as a regular method call
            if (propertyInfo.Member.IsIndexer())
                return null;

            var parms = method.Method.Metadata.GetAttributeParameters("JSIL.Meta.JSReplacement") ??
                propertyInfo.Metadata.GetAttributeParameters("JSIL.Meta.JSReplacement");
            if (parms != null) {
                var argsDict = new Dictionary<string, JSExpression>();
                argsDict["this"] = thisExpression;
                argsDict["typeof(this)"] = new JSType(thisExpression.GetExpectedType(TypeSystem));

                foreach (var kvp in method.Method.Parameters.Zip(arguments, (p, v) => new { p.Name, Value = v })) {
                    argsDict.Add(kvp.Name, kvp.Value);
                }

                return new JSVerbatimLiteral(method, (string)parms[0].Value, argsDict, propertyInfo.ReturnType);
            }

            var thisType = GetTypeDefinition(thisExpression.GetExpectedType(TypeSystem));
            Func<JSExpression> generate = () => {
                var actualThis = @static ? new JSType(method.Method.DeclaringType.Definition) : thisExpression;

                if ((method.Reference.DeclaringType is GenericInstanceType) && !method.Reference.HasThis) {
                    actualThis = new JSType(method.Reference.DeclaringType);
                }

                if ((propertyInfo.Member.GetMethod != null) && (method.Method.Member.Name == propertyInfo.Member.GetMethod.Name)) {
                    return new JSPropertyAccess(
                        actualThis, new JSProperty(method.Reference, propertyInfo)
                    );
                } else {
                    if (arguments.Length == 0)
                        throw new InvalidOperationException("Attempting to invoke a property setter with no arguments");

                    return new JSBinaryOperatorExpression(
                        JSOperator.Assignment,
                        new JSPropertyAccess(
                            actualThis, new JSProperty(method.Reference, propertyInfo)
                        ),
                        arguments[0], propertyInfo.ReturnType
                    );
                }
            };

            // Accesses to a base property should go through a regular method invocation, since
            //  javascript properties do not have a mechanism for base access
            if (method.Method.Member.HasThis) {                
                if (!TypesAreEqual(method.Method.DeclaringType.Definition, thisType) && !@virtual) {
                    return null;
                } else {
                    return generate();
                }
            }

            return generate();
        }

        public static TypeDefinition GetTypeDefinition (TypeReference typeRef, bool resolveTypedArrays = true) {
            if (typeRef == null)
                return null;

            var ts = typeRef.Module.TypeSystem;
            typeRef = DereferenceType(typeRef);

            bool unwrapped = false;
            do {
                var rmt = typeRef as RequiredModifierType;
                var omt = typeRef as OptionalModifierType;

                if (rmt != null) {
                    typeRef = rmt.ElementType;
                    unwrapped = true;
                } else if (omt != null) {
                    typeRef = omt.ElementType;
                    unwrapped = true;
                } else {
                    unwrapped = false;
                }
            } while (unwrapped);

            if (resolveTypedArrays && (typeRef is ArrayType))
                return new TypeReference(ts.Object.Namespace, "Array", ts.Object.Module, ts.Object.Scope).ResolveOrThrow();
            else if (IsIgnoredType(typeRef))
                return null;
            else 
                return typeRef.Resolve();
        }

        public static TypeReference FullyDereferenceType (TypeReference type, out int depth) {
            depth = 0;

            var brt = type as ByReferenceType;
            while (brt != null) {
                depth += 1;
                type = brt.ElementType;
                brt = type as ByReferenceType;
            }

            return type;
        }

        public static TypeReference DereferenceType (TypeReference type, bool dereferencePointers = false) {
            var brt = type as ByReferenceType;
            if (brt != null)
                return brt.ElementType;

            if (dereferencePointers) {
                var pt = type as PointerType;
                if (pt != null)
                    return pt.ElementType;
            }

            return type;
        }

        public static bool IsNumeric (TypeReference type) {
            type = DereferenceType(type);

            switch (type.MetadataType) {
                case MetadataType.Byte:
                case MetadataType.SByte:
                case MetadataType.Double:
                case MetadataType.Single:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                case MetadataType.UInt64:
                    return true;
                // Blech
                case MetadataType.UIntPtr:
                case MetadataType.IntPtr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsIntegral (TypeReference type) {
            type = DereferenceType(type);

            switch (type.MetadataType) {
                case MetadataType.Byte:
                case MetadataType.SByte:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                case MetadataType.UInt64:
                    return true;
                // Blech
                case MetadataType.UIntPtr:
                case MetadataType.IntPtr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEnum (TypeReference type) {
            var typedef = GetTypeDefinition(type);
            return (typedef != null) && (typedef.IsEnum);
        }

        public static bool IsBoolean (TypeReference type) {
            type = DereferenceType(type);
            return type.MetadataType == MetadataType.Boolean;
        }

        public static bool IsIgnoredType (TypeReference type) {
            type = DereferenceType(type);

            if (type == null)
                return false;
            else if (type.IsPointer)
                return true;
            else if (type.IsPinned)
                return true;
            else if (type.IsFunctionPointer)
                return true;
            else
                return false;
        }

        public static bool IsDelegateType (TypeReference type) {
            type = DereferenceType(type);

            var typedef = GetTypeDefinition(type);
            if (typedef == null)
                return false;
            if (typedef.FullName == "System.Delegate")
                return true;

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
            if (target == source)
                return true;
            else if ((target == null) || (source == null))
                return (target == source);

            bool result;

            int targetDepth, sourceDepth;
            FullyDereferenceType(target, out targetDepth);
            FullyDereferenceType(source, out sourceDepth);

            var targetGit = target as GenericInstanceType;
            var sourceGit = source as GenericInstanceType;

            if ((targetGit != null) && TypesAreEqual(targetGit.ElementType, source))
                return true;
            if ((sourceGit != null) && TypesAreEqual(target, sourceGit.ElementType))
                return true;

            if ((target.IsByReference != source.IsByReference) || (targetDepth != sourceDepth))
                result = false;
            else if (target.IsPointer != source.IsPointer)
                result = false;
            else if (target.IsGenericParameter != source.IsGenericParameter)
                result = false;
            else if (target.IsArray != source.IsArray)
                result = false;
            else if (target.IsFunctionPointer != source.IsFunctionPointer)
                result = false;
            else if (target.IsPinned != source.IsPinned)
                result = false;
            else {
                if (
                    (target.Name == source.Name) &&
                    (target.Namespace == source.Namespace) &&
                    (target.Module == source.Module) &&
                    TypesAreEqual(target.DeclaringType, source.DeclaringType)
                )
                    return true;

                var dTarget = GetTypeDefinition(target);
                var dSource = GetTypeDefinition(source);

                if (Object.Equals(dTarget, dSource) && (dSource != null))
                    result = true;
                else if (Object.Equals(target, source))
                    result = true;
                else if (
                    (dTarget != null) && (dSource != null) &&
                    (dTarget.FullName == dSource.FullName)
                )
                    result = true;
                else
                    result = false;
            }

            return result;
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
            // All values are assignable to object
            if (target.FullName == "System.Object")
                return true;

            int targetDepth, sourceDepth;
            if (TypesAreEqual(FullyDereferenceType(target, out targetDepth), FullyDereferenceType(source, out sourceDepth))) {
                if (targetDepth == sourceDepth)
                    return true;
            }

            // Complex hack necessary because System.Array and T[] do not implement IEnumerable<T>
            var targetGit = target as GenericInstanceType;
            if (
                (targetGit != null) &&
                (targetGit.Name == "IEnumerable`1") &&
                source.IsArray &&
                (targetGit.GenericArguments.FirstOrDefault() == source.GetElementType())
            )
                return true;

            var cacheKey = new Tuple<string, string>(target.FullName, source.FullName);
            return TypeAssignabilityCache.GetOrCreate(
                cacheKey, () => {
                    bool result = false;

                    var dSource = GetTypeDefinition(source);

                    if (dSource == null)
                        result = false;
                    else if (TypesAreEqual(target, dSource))
                        result = true;
                    else if ((dSource.BaseType != null) && TypesAreAssignable(target, dSource.BaseType))
                        result = true;
                    else {
                        foreach (var iface in dSource.Interfaces) {
                            if (TypesAreAssignable(target, iface)) {
                                result = true;
                                break;
                            }
                        }
                    }

                    return result;
                }
            );
        }

        protected bool ContainsLabels (ILNode root) {
            var label = root.GetSelfAndChildrenRecursive<ILLabel>().FirstOrDefault();
            return label != null;
        }


        //
        // IL Node Types
        //

        protected JSBlockStatement TranslateBlock (IEnumerable<ILNode> children) {
            JSBlockStatement result, currentBlock;

            currentBlock = result = new JSBlockStatement();

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
            return TranslateBlock(block.Body);
        }

        public JSExpression TranslateNode (ILFixedStatement fxd) {
            throw new AbortTranslation("Fixed statements not implemented");
        }

        public JSExpression TranslateNode (ILExpression expression) {
            JSExpression result = null;

            var type = expression.ExpectedType ?? expression.InferredType;
            if (ILBlockTranslator.IsIgnoredType(type))
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

            if (
                (result != null) &&
                (expression.ExpectedType != null) &&
                (expression.InferredType != null) &&
                !TypesAreAssignable(expression.ExpectedType, expression.InferredType)
            ) {
                return JSCastExpression.New(result, expression.ExpectedType, TypeSystem);
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

        public JSSwitchCase TranslateNode (ILSwitch.CaseBlock block, TypeReference conditionType = null) {
            JSExpression[] values = null;

            if (block.Values != null) {
                if ((conditionType != null) && (conditionType.MetadataType == MetadataType.Char)) {
                    values = (from v in block.Values select JSLiteral.New(Convert.ToChar(v))).ToArray();
                } else {
                    values = (from v in block.Values select JSLiteral.New(v)).ToArray();
                }
            }

            return new JSSwitchCase(
                values,
                TranslateNode(new ILBlock(block.Body))
            );
        }

        public JSSwitchStatement TranslateNode (ILSwitch swtch) {
            var condition = TranslateNode(swtch.Condition);
            var conditionType = condition.GetExpectedType(TypeSystem);
            var result = new JSSwitchStatement(condition);

            Blocks.Push(result);

            result.Cases.AddRange(
                (from cb in swtch.CaseBlocks select TranslateNode(cb, conditionType))
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
                catchVariable = DeclareVariable(new JSExceptionVariable(TypeSystem, ThisMethodReference));

                bool foundUniversalCatch = false;
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
                        var excVariable = DeclareVariable(cb.ExceptionVariable, ThisMethodReference);

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
                if (catchBlock != null)
                    throw new Exception("A try block cannot have both a catch block and a fault block");

                catchVariable = DeclareVariable(new JSExceptionVariable(TypeSystem, ThisMethodReference));
                catchBlock = new JSBlockStatement(TranslateBlock(tcb.FaultBlock.Body));

                catchBlock.Statements.Add(new JSExpressionStatement(new JSThrowExpression(catchVariable)));
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
            result.Index = UnlabelledBlockCount++;
            Blocks.Push(result);

            var body = TranslateNode(loop.BodyBlock);

            Blocks.Pop();
            result.Statements.Add(body);
            return result;
        }


        //
        // MSIL Instructions
        //

        protected JSExpression Translate_Sizeof (ILExpression node, TypeReference type) {
            return new JSUntranslatableExpression("Sizeof");
        }

        protected JSExpression Translate_NullableOf (ILExpression node) {
            var inner = TranslateNode(node.Arguments[0]);
            var innerType = inner.GetExpectedType(TypeSystem);

            var nullableType = new TypeReference("System", "Nullable`1", TypeSystem.Object.Module, TypeSystem.Object.Scope);
            var nullableGenericType = new GenericInstanceType(nullableType);
            nullableGenericType.GenericArguments.Add(innerType);

            return JSChangeTypeExpression.New(inner, TypeSystem, nullableGenericType);
        }

        protected JSExpression Translate_Wrap (ILExpression node) {
            var inner = TranslateNode(node.Arguments[0]);

            return inner;
        }

        protected JSExpression Translate_ValueOf (ILExpression node) {
            var inner = TranslateNode(node.Arguments[0]);
            var innerType = DereferenceType(inner.GetExpectedType(TypeSystem));

            var innerTypeGit = innerType as GenericInstanceType;
            if (innerTypeGit != null)
                return JSChangeTypeExpression.New(inner, TypeSystem, innerTypeGit.GenericArguments.First());
            else
                return new JSUntranslatableExpression(node);
        }

        protected JSExpression Translate_ComparisonOperator (ILExpression node, JSBinaryOperator op) {
            if (
                (node.Arguments[0].ExpectedType.FullName == "System.Boolean") &&
                (node.Arguments[1].ExpectedType.FullName == "System.Boolean") &&
                (node.Arguments[1].Code.ToString().Contains("Ldc_"))
            ) {
                // Comparison against boolean constant
                bool comparand = Convert.ToInt64(node.Arguments[1].Operand) != 0;
                bool checkEquality = (op == JSOperator.Equal);

                if (comparand != checkEquality)
                    return new JSUnaryOperatorExpression(
                        JSOperator.LogicalNot, TranslateNode(node.Arguments[0])
                    );
                else
                    return TranslateNode(node.Arguments[0]);
            } else if (
                (!node.Arguments[0].ExpectedType.IsValueType) &&
                (!node.Arguments[1].ExpectedType.IsValueType) &&
                (node.Arguments[0].ExpectedType == node.Arguments[1].ExpectedType) &&
                (node.Arguments[0].Code == ILCode.Isinst)
            ) {
                // The C# expression 'x is y' translates into roughly '(x is y) > null' in IL, 
                //  because there's no IL opcode for != and the IL isinst opcode returns object, not bool
                var value = TranslateNode(node.Arguments[0].Arguments[0]);
                var nullLiteral = TranslateNode(node.Arguments[1]) as JSNullLiteral;
                var targetType = (TypeReference)node.Arguments[0].Operand;

                var targetInfo = TypeInfo.Get(targetType);
                JSExpression checkTypeResult;

                if ((targetInfo != null) && targetInfo.IsIgnored)
                    checkTypeResult = JSLiteral.New(false);
                else
                    checkTypeResult = JSIL.CheckType(
                        value, targetType
                    );

                if (nullLiteral != null) {
                    if (
                        (op == JSOperator.Equal) ||
                        (op == JSOperator.LessThanOrEqual) ||
                        (op == JSOperator.LessThan)
                    ) {
                        return new JSUnaryOperatorExpression(
                            JSOperator.LogicalNot, checkTypeResult, TypeSystem.Boolean
                        );
                    } else if (
                        (op == JSOperator.GreaterThan)
                    ) {
                        return checkTypeResult;
                    } else {
                        return new JSUntranslatableExpression(node);
                    }
                }
            }

            return Translate_BinaryOp(node, op);
        }

        protected JSExpression Translate_Clt (ILExpression node) {
            return Translate_ComparisonOperator(node, JSOperator.LessThan);
        }

        protected JSExpression Translate_Cgt (ILExpression node) {
            return Translate_ComparisonOperator(node, JSOperator.GreaterThan);
        }

        protected JSExpression Translate_Ceq (ILExpression node) {
            return Translate_ComparisonOperator(node, JSOperator.Equal);
        }

        protected JSExpression Translate_Cne (ILExpression node) {
            return Translate_ComparisonOperator(node, JSOperator.NotEqual);
        }

        protected JSExpression Translate_Cle (ILExpression node) {
            return Translate_ComparisonOperator(node, JSOperator.LessThanOrEqual);
        }

        protected JSExpression Translate_Cge (ILExpression node) {
            return Translate_ComparisonOperator(node, JSOperator.GreaterThanOrEqual);
        }

        protected JSBinaryOperatorExpression Translate_CompoundAssignment (ILExpression node) {
            JSAssignmentOperator op = null;
            var translated = TranslateNode(node.Arguments[0]);
            if (translated is JSResultReferenceExpression)
                translated = ((JSResultReferenceExpression)translated).Referent;

            var boe = translated as JSBinaryOperatorExpression;
            var invocation = translated as JSInvocationExpression;

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
            }

            if (boe != null) {
                if (op != null) {
                    return new JSBinaryOperatorExpression(
                        op, boe.Left, boe.Right, boe.ExpectedType
                    );
                } else {
                    // Unimplemented compound operators, and operators with semantics that don't match JS, must be emitted normally
                    return new JSBinaryOperatorExpression(
                        JSOperator.Assignment, boe.Left,
                        boe, boe.ExpectedType
                    );
                }
            } else if ((invocation != null) && (invocation.Arguments[0] is JSReferenceExpression)) {
                // Some compound expressions involving structs produce a call instruction instead of a binary expression
                return new JSBinaryOperatorExpression(
                    JSOperator.Assignment, invocation.Arguments[0],
                    invocation, invocation.GetExpectedType(TypeSystem)
                );
            } else {
                throw new NotImplementedException(String.Format("Compound assignments of this type not supported: '{0}'", node));
            }
        }

        protected JSTernaryOperatorExpression Translate_TernaryOp (ILExpression node) {
            var expectedType = node.ExpectedType;
            var inferredType = node.InferredType;

            var left = node.Arguments[1];
            var right = node.Arguments[2];

            // FIXME: ILSpy generates invalid type information for ternary operators.
            //  Detect invalid type information and replace it with less-invalid type information.
            if (
                (!TypesAreEqual(left.ExpectedType, right.ExpectedType)) ||
                (!TypesAreEqual(left.InferredType, right.InferredType))
            ) {
                left.ExpectedType = left.InferredType;
                right.ExpectedType = right.InferredType;
                inferredType = expectedType ?? TypeSystem.Object;
            }

            return new JSTernaryOperatorExpression(
                TranslateNode(node.Arguments[0]),
                TranslateNode(left),
                TranslateNode(right),
                expectedType ?? inferredType
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
            return new JSThrowExpression(new JSStringIdentifier(
                "$exception", new TypeReference("System", "Exception", TypeSystem.Object.Module, TypeSystem.Object.Scope)
            ));
        }

        protected JSThrowExpression Translate_Throw (ILExpression node) {
            return new JSThrowExpression(TranslateNode(node.Arguments[0]));
        }

        protected JSExpression Translate_Endfinally (ILExpression node) {
            return JSExpression.Null;
        }

        protected JSBreakExpression Translate_LoopOrSwitchBreak (ILExpression node) {
            var result = new JSBreakExpression();

            if (Blocks.Count > 0) {
                var theLoop = Blocks.Peek() as JSLoopStatement;
                if (theLoop != null)
                    result.TargetLoop = theLoop.Index.Value;
            }

            return result;
        }

        protected JSContinueExpression Translate_LoopContinue (ILExpression node) {
            var result = new JSContinueExpression();

            if (Blocks.Count > 0) {
                var theLoop = Blocks.Peek() as JSLoopStatement;
                if (theLoop != null)
                    result.TargetLoop = theLoop.Index.Value;
            }

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

        protected JSExpression Translate_Ldloc (ILExpression node, ILVariable variable) {
            JSExpression result;
            JSVariable renamed;

            if (RenamedVariables.TryGetValue(variable, out renamed))
                result = new JSIndirectVariable(Variables, renamed.Identifier, ThisMethodReference);
            else
                result = new JSIndirectVariable(Variables, variable.Name, ThisMethodReference);

            var expectedType = node.ExpectedType ?? node.InferredType ?? variable.Type;
            if (!TypesAreAssignable(expectedType, variable.Type))
                result = Translate_Conv(result, expectedType);

            return result;
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
            if ((value.IsNull) && !(value is JSUntranslatableExpression) && !(value is JSIgnoredMemberReference))
                return new JSNullExpression();

            var expectedType = node.ExpectedType ?? node.InferredType ?? variable.Type;
            if (!TypesAreAssignable(expectedType, value.GetExpectedType(TypeSystem)))
                value = Translate_Conv(value, expectedType);

            JSVariable jsv;

            if (RenamedVariables.TryGetValue(variable, out jsv))
                jsv = new JSIndirectVariable(Variables, jsv.Identifier, ThisMethodReference);
            else
                jsv = new JSIndirectVariable(Variables, variable.Name, ThisMethodReference);

            if (jsv.IsReference) {
                JSExpression materializedValue;
                if (!JSReferenceExpression.TryMaterialize(JSIL, value, out materializedValue))
                    Console.Error.WriteLine(String.Format("Cannot store a non-reference into variable {0}: {1}", jsv, value));
                else
                    value = materializedValue;
            }

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment, jsv,
                value,
                value.GetExpectedType(TypeSystem)
            );
        }

        protected JSExpression Translate_Ldsfld (ILExpression node, FieldReference field) {
            var fieldInfo = TypeInfo.GetField(field);
            if (fieldInfo == null)
                return new JSIgnoredMemberReference(true, null, JSLiteral.New(field.FullName));
            else if (IsIgnoredType(field.FieldType) || fieldInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, fieldInfo);

            JSExpression result = new JSDotExpression(
                new JSType(field.DeclaringType),
                new JSField(field, fieldInfo)
            );

            if (CopyOnReturn(field.FieldType))
                result = JSReferenceExpression.New(result);

            return result;
        }

        protected JSExpression Translate_Ldsflda (ILExpression node, FieldReference field) {
            var fieldInfo = TypeInfo.GetField(field);
            if (fieldInfo == null)
                return new JSIgnoredMemberReference(true, null, JSLiteral.New(field.FullName));
            else if (IsIgnoredType(field.FieldType) || fieldInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, fieldInfo);

            return new JSMemberReferenceExpression(
                new JSDotExpression(
                    new JSType(field.DeclaringType),
                    new JSField(field, fieldInfo)
                )
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
            if ((translated.IsNull) && !(translated is JSUntranslatableExpression) && !(translated is JSIgnoredMemberReference))
                return new JSNullExpression();

            var fieldInfo = TypeInfo.GetField(field);
            if (IsIgnoredType(field.FieldType) || (fieldInfo == null) || fieldInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, fieldInfo, translated);

            JSExpression thisExpression;
            if (IsInvalidThisExpression(firstArg)) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression)) {
                    if (!translated.IsNull)
                        Console.Error.WriteLine("Warning: Accessing {0} without a reference as this.", field.FullName);

                    thisExpression = translated;
                }
            } else {
                thisExpression = translated;
            }

            JSExpression result = new JSDotExpression(
                thisExpression,
                new JSField(field, fieldInfo)
            );

            if (CopyOnReturn(field.FieldType))
                result = JSReferenceExpression.New(result);

            return result;
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
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);

            var fieldInfo = TypeInfo.GetField(field);
            if (IsIgnoredType(field.FieldType) || (fieldInfo == null) || fieldInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, fieldInfo, translated);

            JSExpression thisExpression;
            if (IsInvalidThisExpression(firstArg)) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression)) {
                    if (!translated.IsNull)
                        Console.Error.WriteLine("Warning: Accessing {0} without a reference as this.", field.FullName);

                    thisExpression = translated;
                }
            } else {
                thisExpression = translated;
            }

            return new JSMemberReferenceExpression(new JSDotExpression(
                thisExpression,
                new JSField(field, fieldInfo)
            ));
        }

        protected JSExpression Translate_Ldobj (ILExpression node, TypeReference type) {
            var reference = TranslateNode(node.Arguments[0]);
            JSExpression referent;

            if (reference == null)
                throw new InvalidOperationException();

            if (!JSReferenceExpression.TryDereference(JSIL, reference, out referent))
                Console.Error.WriteLine(String.Format("Warning: unsupported reference type for ldobj: {0}", node.Arguments[0]));

            if ((referent != null) && EmulateStructAssignment.IsStruct(referent.GetExpectedType(TypeSystem)))
                return reference;
            else {
                if (referent != null)
                    return referent;
                else
                    return new JSUntranslatableExpression(node);
            }
        }

        protected JSExpression Translate_Ldind (ILExpression node) {
            return Translate_Ldobj(node, null);
        }

        protected JSExpression Translate_Stobj (ILExpression node, TypeReference type) {
            var target = TranslateNode(node.Arguments[0]);
            var targetVariable = target as JSVariable;
            var value = TranslateNode(node.Arguments[1]);

            if (targetVariable != null) {
                if (!targetVariable.IsReference)
                    Console.Error.WriteLine(String.Format("Warning: unsupported target variable for stobj: {0}", node.Arguments[0]));
            } else {
                JSExpression referent;
                if (!JSReferenceExpression.TryMaterialize(JSIL, target, out referent))
                    Console.Error.WriteLine(String.Format("Warning: unsupported target expression for stobj: {0}", node.Arguments[0]));
                else
                    target = new JSDotExpression(referent, new JSStringIdentifier("value", value.GetExpectedType(TypeSystem)));
            }

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment, target, value, node.ExpectedType ?? node.InferredType
            );
        }

        protected JSExpression Translate_Stind (ILExpression node) {
            return Translate_Stobj(node, null);
        }

        protected JSExpression Translate_AddressOf (ILExpression node) {
            var referent = TranslateNode(node.Arguments[0]);

            var referentInvocation = referent as JSInvocationExpression;
            if (referentInvocation != null)
                return new JSResultReferenceExpression(referentInvocation);

            return JSReferenceExpression.New(referent);
        }

        protected JSExpression Translate_Arglist (ILExpression node) {
            return new JSUntranslatableExpression("Arglist");
        }

        protected JSExpression Translate_Localloc (ILExpression node) {
            return new JSUntranslatableExpression("Localloc");
        }

        protected JSStringLiteral Translate_Ldstr (ILExpression node, string text) {
            return JSLiteral.New(text);
        }

        protected JSExpression Translate_Ldnull (ILExpression node) {
            return JSLiteral.Null(node.ExpectedType ?? node.InferredType);
        }

        protected JSExpression Translate_Ldftn (ILExpression node, MethodReference method) {
            var methodInfo = TypeInfo.GetMethod(method);
            if (methodInfo == null)
                return new JSIgnoredMemberReference(true, null, new JSStringLiteral(method.FullName));

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

        protected JSExpression Translate_Ldvirtftn (ILExpression node, MethodReference method) {
            return new JSUntranslatableExpression("Ldvirtftn");
        }

        protected JSExpression Translate_Ldc (ILExpression node, long value) {
            string typeName = null;
            var expressionType = node.ExpectedType ?? node.InferredType;
            TypeInfo typeInfo = null;
            if (expressionType != null) {
                typeName = expressionType.FullName;
                typeInfo = TypeInfo.Get(expressionType);
            }

            if (
                (typeInfo != null) && 
                (typeInfo.EnumMembers != null) && (typeInfo.EnumMembers.Count > 0)
            ) {
                EnumMemberInfo[] enumMembers = null;
                if (typeInfo.IsFlagsEnum) {
                    if (value == 0) {
                        enumMembers = (
                            from em in typeInfo.EnumMembers.Values
                            where em.Value == 0
                            select em
                        ).Take(1).ToArray();
                    } else {
                        enumMembers = (
                            from em in typeInfo.EnumMembers.Values
                            where (em.Value != 0) &&
                                ((value & em.Value) == em.Value)
                            select em
                        ).ToArray();
                    }
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
            } else if (typeName == "System.Char") {
                return JSLiteral.New((char)value);
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
            if (arg.IsNull)
                return arg;

            var argType = arg.GetExpectedType(TypeSystem);
            var argTypeDef = GetTypeDefinition(argType);
            PropertyDefinition lengthProp = null;
            if (argTypeDef != null)
                lengthProp = (from p in argTypeDef.Properties where p.Name == "Length" select p).FirstOrDefault();

            if (lengthProp == null)
                return new JSUntranslatableExpression(String.Format("Retrieving the length of a type with no length property: {0}", argType.FullName));
            else
                return Translate_CallGetter(node, lengthProp.GetMethod);
        }

        protected JSExpression Translate_Ldelem (ILExpression node, TypeReference elementType) {
            var expectedType = elementType ?? node.ExpectedType ?? node.InferredType;
            var target = TranslateNode(node.Arguments[0]);
            if (target.IsNull)
                return target;

            var indexer = TranslateNode(node.Arguments[1]);

            JSExpression result = new JSIndexerExpression(
                target, indexer,
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

        protected JSExpression Translate_Stelem (ILExpression node) {
            return Translate_Stelem(node, null);
        }

        protected JSExpression Translate_Stelem (ILExpression node, TypeReference elementType) {
            var expectedType = elementType ?? node.ExpectedType ?? node.InferredType;

            var target = TranslateNode(node.Arguments[0]);
            if (target.IsNull)
                return target;

            var indexer = TranslateNode(node.Arguments[1]);
            var rhs = TranslateNode(node.Arguments[2]);

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                new JSIndexerExpression(
                    target, indexer,
                    expectedType
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

            return JSCastExpression.New(
                TranslateNode(node.Arguments[0]),
                targetType,
                TypeSystem
            );
        }

        protected JSExpression Translate_Isinst (ILExpression node, TypeReference targetType) {
            var firstArg = TranslateNode(node.Arguments[0]);

            var targetInfo = TypeInfo.Get(targetType);
            if ((targetInfo != null) && targetInfo.IsIgnored)
                return new JSNullLiteral(targetType);
            else
                return JSIL.TryCast(firstArg, targetType);
        }

        protected JSExpression Translate_Unbox_Any (ILExpression node, TypeReference targetType) {
            var value = TranslateNode(node.Arguments[0]);

            var result = JSCastExpression.New(value, targetType, TypeSystem);

            if (CopyOnReturn(targetType))
                return JSReferenceExpression.New(result);
            else
                return result;
        }

        protected bool IsNumericOrEnum (TypeReference type) {
            var typedef = GetTypeDefinition(type);
            return IsNumeric(type) || ((typedef != null) && typedef.IsEnum);
        }

        protected JSExpression Translate_Conv (JSExpression value, TypeReference expectedType) {
            var currentType = value.GetExpectedType(TypeSystem);

            if (TypesAreEqual(expectedType, currentType))
                return value;

            int currentDepth, expectedDepth;
            var currentDerefed = FullyDereferenceType(currentType, out currentDepth);
            var expectedDerefed = FullyDereferenceType(expectedType, out expectedDepth);

            // Handle assigning a value of type 'T&&' to a variable of type 'T&', etc.
            // 'AreTypesAssignable' will return false, because the types are not equivalent, but no cast is necessary.
            if (TypesAreEqual(expectedDerefed, currentDerefed)) {
                if (currentDepth > expectedDepth) {
                    // If the current expression has more levels of reference than the target type, we must dereference
                    //  the current expression one or more times to strip off the reference levels.
                    var result = value;
                    JSExpression dereferenced;

                    while (currentDepth > expectedDepth) {
                        bool ok = JSReferenceExpression.TryDereference(JSIL, result, out dereferenced);
                        if (!ok)
                            break;

                        currentDepth -= 1;
                        result = dereferenced;
                    }

                    return result;
                } else {
                    return value;
                }
            }

            if (IsDelegateType(expectedType) && IsDelegateType(currentType))
                return value;

            if (IsNumericOrEnum(currentType) && IsNumericOrEnum(expectedType)) {
                if (IsIntegral(expectedType)) {
                    if (IsIntegral(currentType))
                        return value;
                    else
                        return JSInvocationExpression.InvokeStatic(JS.floor, new[] { value }, true);
                } else {
                    return value;
                }
            } else if (!TypesAreAssignable(expectedType, currentType)) {
                if (expectedType.FullName == "System.Boolean") {
                    if (IsIntegral(currentType)) {
                        // i != 0 sometimes becomes (bool)i, so we want to generate the explicit form
                        return new JSBinaryOperatorExpression(
                            JSOperator.NotEqual, value, JSLiteral.New(0), TypeSystem.Boolean
                        );
                    } else if (!currentType.IsValueType) {
                        // We need to detect any attempts to cast object references to boolean and not generate a cast
                        //  for them, so that our logic in Translate_UnaryOp for detecting (x != null) will still work
                        return value;
                    }
                }

                // Never cast AnyType to another type since the implication is that the proxy author will ensure the correct
                //  type is returned.
                if (currentType.FullName == "JSIL.Proxy.AnyType")
                    return value;

                return JSCastExpression.New(value, expectedType, TypeSystem);
            } else
                return value;
        }

        protected JSExpression Translate_Conv (ILExpression node, TypeReference targetType) {
            var value = TranslateNode(node.Arguments[0]);
            var expectedType = node.ExpectedType ?? node.InferredType ?? targetType;

            return Translate_Conv(value, expectedType);
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

        protected JSExpression Translate_Conv_Ovf_I (ILExpression node) {
            return Translate_Conv(node, Context.CurrentModule.TypeSystem.Int32);
        }

        protected JSExpression Translate_Conv_Ovf_I_Un (ILExpression node) {
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
            var value = TranslateNode(node.Arguments[0]);
            return JSReferenceExpression.New(value);
        }

        protected JSExpression Translate_Br (ILExpression node, ILLabel targetLabel) {
            return new JSGotoExpression(targetLabel.Name);
        }

        protected JSExpression Translate_Leave (ILExpression node, ILLabel targetLabel) {
            return new JSGotoExpression(targetLabel.Name);
        }

        protected JSExpression Translate_Newobj_Delegate (ILExpression node, MethodReference constructor, JSExpression[] arguments) {
            var thisArg = arguments[0];
            var methodRef = arguments[1];

            var methodDot = methodRef as JSDotExpression;

            // Detect compiler-generated lambda methods
            if (methodDot != null) {
                var methodMember = methodDot.Member as JSMethod;

                if (methodMember != null) {
                    var methodDef = methodMember.Method.Member;

                    bool compilerGenerated = methodDef.IsCompilerGeneratedOrIsInCompilerGeneratedClass();
                    bool emitInline = (
                            methodDef.IsPrivate && compilerGenerated
                        ) || (
                            compilerGenerated &&
                            TypesAreEqual(
                                thisArg.GetExpectedType(TypeSystem),
                                methodDef.DeclaringType
                            )
                        ) || (
                            methodMember.Method.IsIgnored
                        );

                    if (emitInline) {
                        JSFunctionExpression function;
                        // It's possible that the method we're using wasn't initially translated/analyzed because it's
                        //  a compiler-generated method or part of a compiler generated type
                        if (!Translator.FunctionCache.TryGetExpression(methodMember.QualifiedIdentifier, out function)) {
                            function = Translator.TranslateMethodExpression(Context, methodDef, methodDef);
                        }

                        if (function == null) {
                            return new JSUntranslatableExpression(node);
                        }

                        var thisArgVar = thisArg as JSVariable;

                        // If the closure references the outer 'this' variable, we need to explicitly bind it to the
                        //  closure's local 'this' reference (by setting useBind to true to use Function.bind).
                        // It is also possible for the this-reference to be a variable with a name that collides with
                        //  the name of a local variable within the closure. The solution in this case is the same:
                        //  we make it the closure's local 'this' reference using Function.bind.
                        if (
                            (thisArgVar != null) &&
                            (thisArgVar.IsThis || function.AllVariables.ContainsKey(thisArgVar.Name))
                        ) {
                            return new JSLambda(function, thisArgVar, true);
                        }

                        return new JSLambda(
                            function, thisArg, !(
                                thisArg.IsNull || thisArg is JSNullLiteral
                            )
                        );
                    }
                }
            }

            return JSIL.NewDelegate(
                constructor.DeclaringType,
                thisArg, methodRef
            );
        }

        protected JSExpression Translate_Newobj (ILExpression node, MethodReference constructor) {
            var arguments = Translate(node.Arguments);

            if (IsDelegateType(constructor.DeclaringType)) {
                return Translate_Newobj_Delegate(node, constructor, arguments);
            } else if (constructor.DeclaringType.IsArray) {
                return JSIL.NewMultidimensionalArray(
                    constructor.DeclaringType.GetElementType(), arguments
                );
            }

            var methodInfo = TypeInfo.GetMethod(constructor);
            if ((methodInfo == null) || methodInfo.IsIgnored)
                return new JSIgnoredMemberReference(true, methodInfo, arguments);

            return new JSNewExpression(
                constructor.DeclaringType, methodInfo, arguments
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

        public static JSExpression[] GetArrayDimensions (TypeReference arrayType) {
            var at = arrayType as ArrayType;
            if (at == null)
                return null;

            var result = new List<JSExpression>();
            for (var i = 0; i < at.Dimensions.Count; i++) {
                var dim = at.Dimensions[i];
                if (dim.IsSized)
                    result.Add(JSLiteral.New(dim.UpperBound.Value));
                else
                    return null;
            }

            return result.ToArray();
        }

        protected JSExpression Translate_InitArray (ILExpression node, TypeReference elementType) {
            var initializer = new JSArrayExpression(elementType, Translate(node.Arguments));
            var at = elementType as ArrayType;
            int rank = 0;
            if (at != null)
                rank = at.Rank;

            if (TypesAreEqual(
                TypeSystem.Object, elementType
            ) && rank < 2)
                return initializer;
            else {
                if (rank > 1) {
                    return JSIL.NewMultidimensionalArray(
                        elementType, GetArrayDimensions(at), initializer
                    );
                } else {
                    return JSIL.NewArray(
                        elementType, initializer
                    );
                }
            }
        }

        protected JSExpression Translate_InitializedObject (ILExpression node) {
            // This should get eliminated by the handler for InitObject, but if we just return a null expression here,
            //  stfld treats us as an invalid assignment target.
            return new JSUntranslatableExpression(node.Code);
        }

        protected JSExpression Translate_InitCollection (ILExpression node) {
            var values = new List<JSArrayExpression>();

            for (var i = 1; i < node.Arguments.Count; i++) {
                var translated = TranslateNode(node.Arguments[i]);

                while (translated is JSReferenceExpression)
                    translated = ((JSReferenceExpression)translated).Referent;

                var invocation = (JSInvocationExpression)translated;

                // each JSArrayExpression added to values contains the arguments
                // to the Add method which is called by CollectionInitializer.Apply
                values.Add(new JSArrayExpression(TypeSystem.Object, invocation.Arguments.ToArray()));
            }

            var initializer = JSIL.NewCollectionInitializer(
                values
            );

            return new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                TranslateNode(node.Arguments[0]),
                initializer,
                TypeSystem.Object
            );
        }

        protected JSInitializerApplicationExpression Translate_InitObject (ILExpression node) {
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
                    var method = ie.JSMethod;

                    if (
                        (method != null) && (method.Method.DeclaringProperty != null)
                    ) {
                        initializers.Add(new JSPairExpression(
                            new JSProperty(method.Reference, method.Method.DeclaringProperty), ie.Arguments[0]
                        ));
                    } else {
                        Console.Error.WriteLine(String.Format("Warning: Object initializer element not implemented: {0}", translated));
                    }
                } else {
                    Console.Error.WriteLine(String.Format("Warning: Object initializer element not implemented: {0}", translated));
                }
            }

            return new JSInitializerApplicationExpression(
                target, new JSObjectExpression(initializers.ToArray())
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
            return new JSField(field, fieldInfo);
        }

        protected JSExpression Translate_Call (ILExpression node, MethodReference method) {
            var methodInfo = TypeInfo.GetMethod(method);
            if (methodInfo == null)
                return new JSIgnoredMemberReference(true, null, JSLiteral.New(method.FullName));

            var thisType = DereferenceType(ThisMethod.DeclaringType);
            var declaringType = DereferenceType(method.DeclaringType);

            var declaringTypeDef = GetTypeDefinition(declaringType);

            var arguments = Translate(node.Arguments, method.Parameters, method.HasThis);
            JSExpression thisExpression;

            bool explicitThis = false;

            if (method.HasThis) {
                var firstArg =  node.Arguments.First();
                var ilv = firstArg.Operand as ILVariable;

                var firstArgType = DereferenceType(firstArg.ExpectedType);

                if (IsInvalidThisExpression(firstArg)) {
                    if (!JSReferenceExpression.TryDereference(JSIL, arguments[0], out thisExpression)) {
                        if (arguments[0].IsNull)
                            thisExpression = arguments[0];
                        else
                            throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
                    }
                } else {
                    thisExpression = arguments[0];
                }

                arguments = arguments.Skip(1).ToArray();

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
                } else {
                    explicitThis = true;
                }
            } else {
                explicitThis = true;
                thisExpression = new JSNullExpression();
            }

            var result = Translate_MethodReplacement(
                new JSMethod(method, methodInfo), thisExpression, arguments, false, !method.HasThis, explicitThis
            );

            if (method.ReturnType.MetadataType != MetadataType.Void) {
                var expectedType = node.ExpectedType ?? node.InferredType ?? method.ReturnType;
                if (!TypesAreAssignable(expectedType, result.GetExpectedType(TypeSystem)))
                    result = Translate_Conv(result, expectedType);
            }

            if (CopyOnReturn(result.GetExpectedType(TypeSystem)))
                result = JSReferenceExpression.New(result);

            return result;
        }

        protected bool IsInvalidThisExpression (ILExpression thisNode) {
            if (thisNode.InferredType == null)
                return false;

            var dereferenced = DereferenceType(thisNode.InferredType);
            if ((dereferenced != null) && dereferenced.IsValueType)
                return true;

            return false;
        }

        protected JSExpression Translate_Callvirt (ILExpression node, MethodReference method) {
            var firstArg = node.Arguments[0];
            var translated = TranslateNode(firstArg);
            JSExpression thisExpression;

            if (IsInvalidThisExpression(firstArg)) {
                if (!JSReferenceExpression.TryDereference(JSIL, translated, out thisExpression)) {
                    if (translated.IsNull)
                        thisExpression = translated;
                    else
                        throw new InvalidOperationException("this-expression for method invocation on value type must be a reference");
                }
            } else {
                thisExpression = translated;
            }

            var translatedArguments = Translate(node.Arguments, method.Parameters, method.HasThis).Skip(1).ToArray();
            var methodInfo = TypeInfo.GetMethod(method);

            if (methodInfo == null)
                return new JSIgnoredMemberReference(true, null, JSLiteral.New(method.FullName));

            var result = Translate_MethodReplacement(
               new JSMethod(method, methodInfo), thisExpression, translatedArguments, true, false, false
            );

            if (method.ReturnType.MetadataType != MetadataType.Void) {
                var expectedType = node.ExpectedType ?? node.InferredType ?? method.ReturnType;
                if (!TypesAreAssignable(expectedType, result.GetExpectedType(TypeSystem)))
                    result = Translate_Conv(result, expectedType);
            }

            if (CopyOnReturn(result.GetExpectedType(TypeSystem)))
                result = JSReferenceExpression.New(result);

            return result;
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
                    return new JSUntranslatableExpression(node);
            } else if (ldcallsite.Code == ILCode.GetCallSite) {
                if (!DynamicCallSites.Get((FieldReference)ldcallsite.Operand, out callSite))
                    return new JSUntranslatableExpression(node);
            } else {
                throw new NotImplementedException("Unknown call site pattern");
            }

            var invocationArguments = Translate(node.Arguments.Skip(1));
            return callSite.Translate(this, invocationArguments);
        }

        protected JSExpression Translate_GetCallSite (ILExpression node, FieldReference field) {
            return new JSNullExpression();
        }

        protected JSExpression Translate_CreateCallSite (ILExpression node, FieldReference field) {
            return new JSNullExpression();
        }

        protected JSExpression Translate_CallGetter (ILExpression node, MethodReference getter) {
            var result = Translate_Call(node, getter);

            return result;
        }

        protected JSExpression Translate_CallSetter (ILExpression node, MethodReference setter) {
            return Translate_Call(node, setter);
        }

        protected JSExpression Translate_CallvirtGetter (ILExpression node, MethodReference getter) {
            var result = Translate_Callvirt(node, getter);

            return result;
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
        public AbortTranslation (string reason)
            : base(reason) {
        }
    }
}
