using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Ast {
    public class JSFunctionExpression : JSExpression {
        public readonly MethodTypeFactory MethodTypes;

        public readonly JSMethod Method;

        public readonly Dictionary<string, JSVariable> AllVariables;
        // This has to be JSVariable, because 'this' is of type (JSVariableReference<JSThisParameter>) for structs
        // We also need to make this an IEnumerable, so it can be a select expression instead of a constant array
        public readonly IEnumerable<JSVariable> Parameters;
        public readonly JSBlockStatement Body;

        public string DisplayName = null;

        public JSFunctionExpression (
            JSMethod method, Dictionary<string, JSVariable> allVariables,
            IEnumerable<JSVariable> parameters, JSBlockStatement body,
            MethodTypeFactory methodTypes
        ) {
            Method = method;
            AllVariables = allVariables;
            Parameters = parameters;
            Body = body;
            MethodTypes = methodTypes;
        }

        public override IEnumerable<JSNode> Children {
            get {
                foreach (var parameter in Parameters)
                    yield return parameter;

                yield return Body;
            }
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSFunctionExpression;
            if (rhs != null) {
                if (!Object.Equals(Method, rhs.Method))
                    return false;
                if (!Object.Equals(AllVariables, rhs.AllVariables))
                    return false;
                if (!Object.Equals(Parameters, rhs.Parameters))
                    return false;

                return EqualsImpl(obj, true);
            }

            return EqualsImpl(obj, true);
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (Method != null) {
                var delegateType = MethodTypes.Get(Method.Reference, typeSystem);
                if (delegateType == null)
                    return Method.Reference.ReturnType;
                else
                    return delegateType;
            } else
                return typeSystem.Void;
        }

        public override string ToString () {
            return String.Format(
                "function {0} ({1}) {{ ... }}", DisplayName ?? Method.Method.Member.ToString(),
                String.Join(", ", (from p in Parameters select p.Name).ToArray())
            );
        }
    }

    public class JSGotoExpression : JSExpression {
        public readonly string TargetLabel;

        public JSGotoExpression (string targetLabel) {
            TargetLabel = targetLabel;
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSGotoExpression;

            if (rhs == null)
                return base.Equals(obj);

            return String.Equals(TargetLabel, rhs.TargetLabel);
        }

        public override string ToString () {
            return String.Format("goto {0}", TargetLabel);
        }
    }

    // Technically, the following expressions should be statements. But in ILAst, they're expressions...
    public class JSReturnExpression : JSExpression {
        public JSReturnExpression (JSExpression value = null)
            : base(value) {
        }

        public JSExpression Value {
            get {
                return Values[0];
            }
        }
    }

    public class JSThrowExpression : JSExpression {
        public JSThrowExpression (JSExpression exception)
            : base(exception) {
        }

        public JSExpression Exception {
            get {
                return Values[0];
            }
        }
    }

    public class JSBreakExpression : JSExpression {
        public int? TargetLoop;

        public override string ToString () {
            if (TargetLoop.HasValue)
                return String.Format("break $loop{0}", TargetLoop);
            else
                return "break";
        }
    }

    public class JSContinueExpression : JSExpression {
        public int? TargetLoop;

        public override string ToString () {
            if (TargetLoop.HasValue)
                return String.Format("continue $loop{0}", TargetLoop);
            else
                return "continue";
        }
    }

    // Indicates that the contained expression is a constructed reference to a JS value.
    public class JSReferenceExpression : JSExpression {
        protected JSReferenceExpression (JSExpression referent)
            : base(referent) {

            if (referent is JSResultReferenceExpression)
                throw new InvalidOperationException("Cannot take a reference to a result-reference");
        }

        /// <summary>
        /// Converts a constructed reference into the expression it refers to, turning it back into a regular expression.
        /// </summary>
        public static bool TryDereference (JSILIdentifier jsil, JSExpression reference, out JSExpression referent) {
            var originalReference = reference;
            var cast = reference as JSCastExpression;
            var isCast = false;

            if (cast != null) {
                isCast = true;
                reference = cast.Expression;
            }

            var variable = reference as JSVariable;
            var rre = reference as JSResultReferenceExpression;
            var refe = reference as JSReferenceExpression;
            var boe = reference as JSBinaryOperatorExpression;

            var expressionType = reference.GetActualType(jsil.TypeSystem);
            if (TypeUtil.IsIgnoredType(expressionType)) {
                referent = new JSUntranslatableExpression(expressionType.FullName);
                return true;
            }

            if ((boe != null) && (boe.Operator is JSAssignmentOperator)) {
                if (TryDereference(jsil, boe.Right, out referent))
                    return true;
            }

            if (variable != null) {
                if (variable.IsReference) {
                    var rv = variable.Dereference();

                    // Since a 'ref' parameter looks just like an implicit reference created by
                    //  a field load, we need to detect cases where we are erroneously stripping
                    //  an argument's reference modifier
                    if (variable.IsParameter) {
                        if (rv.IsReference != variable.GetParameter().IsReference)
                            rv = variable.GetParameter();
                    }

                    referent = rv;
                    if (isCast)
                        referent = JSCastExpression.New(referent, cast.NewType, jsil.TypeSystem);

                    return true;
                }
            }

            if (rre != null) {
                if (rre.Depth == 1) {
                    referent = rre.Referent;
                    if (isCast)
                        referent = JSCastExpression.New(referent, cast.NewType, jsil.TypeSystem);

                    return true;
                } else {
                    referent = rre.Dereference();
                    if (isCast)
                        referent = JSCastExpression.New(referent, cast.NewType, jsil.TypeSystem);

                    return true;
                }
            }

            if (refe == null) {
                referent = null;
                return false;
            }

            referent = refe.Referent;
            if (isCast)
                referent = JSCastExpression.New(referent, cast.NewType, jsil.TypeSystem);

            return true;
        }

        /// <summary>
        /// Converts a constructed reference into an actual reference to the expression it refers to, allowing it to be passed to functions.
        /// </summary>
        public static bool TryMaterialize (JSILIdentifier jsil, JSExpression reference, out JSExpression materialized) {
            var iref = reference as JSResultReferenceExpression;
            var mref = reference as JSMemberReferenceExpression;

            if (iref != null) {
                var invocation = iref.Referent as JSInvocationExpression;
                var jsm = invocation != null ? invocation.JSMethod : null;
                if (jsm != null) {

                    // For some reason the compiler sometimes generates 'addressof Array.Get(...)', and then it
                    //  assigns into that reference later on to fill the array element. This only makes sense because
                    //  for Array.Get to return a struct via the stack, it has to return a reference to the struct,
                    //  and that reference happens to point directly into the array storage. Evil!
                    if (TypeUtil.GetTypeDefinition(jsm.Reference.DeclaringType).FullName == "System.Array") {
                        materialized = JSInvocationExpression.InvokeMethod(
                            invocation.JSType, new JSFakeMethod(
                                "GetReference", new ByReferenceType(jsm.Reference.ReturnType),
                                (from p in jsm.Reference.Parameters select p.ParameterType).ToArray(),
                                jsil.MethodTypes
                            ), invocation.ThisReference, invocation.Arguments.ToArray(), true
                        );
                        return true;
                    }
                }
            }

            if (mref != null) {
                var referent = mref.Referent;
                while (referent is JSReferenceExpression)
                    referent = ((JSReferenceExpression)referent).Referent;

                var dot = referent as JSDotExpressionBase;

                if (dot != null) {
                    materialized = jsil.NewMemberReference(
                        dot.Target, dot.Member.ToLiteral()
                    );
                    return true;
                }
            }

            var variable = reference as JSVariable;
            var refe = reference as JSReferenceExpression;
            if (refe != null)
                variable = refe.Referent as JSVariable;

            if ((variable != null) && (variable.IsReference)) {
                materialized = variable.Dereference();
                return true;
            }

            materialized = null;
            return false;
        }

        public static JSExpression New (JSExpression referent) {
            var variable = referent as JSVariable;
            var invocation = referent as JSInvocationExpression;
            var rre = referent as JSResultReferenceExpression;

            if ((variable != null) && (variable.IsReference)) {
                return variable;
            } else if (invocation != null) {
                return new JSResultReferenceExpression(invocation, 1);
            } else if (rre != null) {
                return rre.Reference();
            } else {
                return new JSReferenceExpression(referent);
            }
        }

        public JSExpression Referent {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return new ByReferenceType(Referent.GetActualType(typeSystem));
        }

        public override string ToString () {
            return String.Format("&({0})", Referent);
        }
    }

    // Represents a reference to the return value of a method call.
    public class JSResultReferenceExpression : JSReferenceExpression {
        public readonly int Depth;

        public JSResultReferenceExpression (JSInvocationExpressionBase invocation, int depth = 1)
            : base(invocation) {
            Depth = depth;
        }

        new public JSInvocationExpressionBase Referent {
            get {
                var sce = base.Referent as JSStructCopyExpression;
                if (sce != null)
                    return (JSInvocationExpressionBase)sce.Struct;
                else
                    return (JSInvocationExpressionBase)base.Referent;
            }
        }

        internal JSResultReferenceExpression Dereference () {
            if (Depth <= 1)
                throw new InvalidOperationException("Dereferencing a non-reference");
            else
                return new JSResultReferenceExpression(Referent, Depth - 1);
        }

        internal JSExpression Reference () {
            return new JSResultReferenceExpression(Referent, Depth + 1);
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            var result = Referent.GetActualType(typeSystem);

            for (var i = 0; i < Depth; i++)
                result = new ByReferenceType(result);

            return result;
        }

        public override string ToString () {
            return String.Format("{0}({1})", new String('&', Depth), Referent);
        }
    }

    // Represents a reference to the result of a member reference (typically a JSDotExpression).
    public class JSMemberReferenceExpression : JSReferenceExpression {
        public JSMemberReferenceExpression (JSExpression referent)
            : base(referent) {
        }
    }

    // Indicates that the contained expression needs to be passed by reference.
    public class JSPassByReferenceExpression : JSExpression {
        public JSPassByReferenceExpression (JSExpression referent)
            : base(referent) {

            if (referent is JSPassByReferenceExpression)
                throw new InvalidOperationException("Nested references are not allowed");
        }

        public JSExpression Referent {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Referent.GetActualType(typeSystem);
        }

        public override string ToString () {
            return String.Format("<ref {0}>", Referent);
        }
    }

    public class JSNullExpression : JSExpression {
        public override bool IsNull {
            get {
                return true;
            }
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            return;
        }

        public override string ToString () {
            return "<Null>";
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Void;
        }

        public override bool Equals (object obj) {
            return EqualsImpl(obj, true);
        }
    }

    public class JSUntranslatableExpression : JSNullExpression {
        public readonly object Type;

        public JSUntranslatableExpression (object type) {
            Type = type;
        }

        public override string ToString () {
            return String.Format("Untranslatable Expression {0}", Type);
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSUntranslatableExpression;
            if (rhs != null) {
                if (!Object.Equals(Type, rhs.Type))
                    return false;
            }

            return EqualsImpl(obj, true);
        }
    }

    public class JSEliminatedVariable : JSNullExpression {
        public readonly JSVariable Variable;

        public JSEliminatedVariable (JSVariable v) {
            Variable = v;
        }
    }

    public class JSIgnoredMemberReference : JSExpression {
        public readonly bool ThrowError;
        public readonly IMemberInfo Member;
        public readonly JSExpression[] Arguments;

        public JSIgnoredMemberReference (bool throwError, IMemberInfo member, params JSExpression[] arguments) {
            ThrowError = throwError;
            Member = member;
            Arguments = arguments;
        }

        public override string ToString () {
            if (Member != null)
                return String.Format("Reference to ignored member {0}", Member.Name);
            else
                return "Reference to ignored member (no info)";
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield break;
            }
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSIgnoredMemberReference;
            if (rhs != null) {
                if (!Object.Equals(Member, rhs.Member))
                    return false;

                if (Arguments.Length != rhs.Arguments.Length)
                    return false;

                for (int i = 0; i < Arguments.Length; i++)
                    if (!Arguments[i].Equals(rhs.Arguments[i]))
                        return false;
            }

            return EqualsImpl(obj, true);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            var field = Member as FieldInfo;
            if (field != null)
                return field.ReturnType;

            var property = Member as PropertyInfo;
            if (property != null)
                return property.ReturnType;

            var method = Member as MethodInfo;
            if (method != null) {
                if (method.Name == ".ctor")
                    return method.DeclaringType.Definition;

                return method.ReturnType;
            }

            return typeSystem.Void;
        }
    }

    public class JSTypeOfExpression : JSExpression {
        public JSTypeOfExpression (JSType type)
            : base(type) {
        }

        public override bool HasGlobalStateDependency {
            get {
                return false;
            }
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public JSType Type {
            get {
                return (JSType)Values[0];
            }
        }

        public override string ToString () {
            return Type.ToString();
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Type.GetActualType(typeSystem);
        }
    }

    public class JSInitializedObject : JSExpression {
        public readonly TypeReference Type;

        public JSInitializedObject (TypeReference type) {
            Type = type;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Type;
        }
    }

    public abstract class JSDotExpressionBase : JSExpression {
        protected JSDotExpressionBase (JSExpression target, JSIdentifier member)
            : base(target, member) {

            if ((target == null) || (member == null))
                throw new ArgumentNullException();
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            var result = Member.GetActualType(typeSystem);
            if (result == null)
                throw new ArgumentNullException();

            return result;
        }

        public JSExpression Target {
            get {
                return Values[0];
            }
        }

        public JSIdentifier Member {
            get {
                return (JSIdentifier)Values[1];
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Target.HasGlobalStateDependency || Member.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                return Target.IsConstant && Member.IsConstant;
            }
        }

        public override string ToString () {
            return String.Format("{0}.{1}", Target, Member);
        }
    }

    public class JSDotExpression : JSDotExpressionBase {
        public JSDotExpression (JSExpression target, JSIdentifier member)
            : base(target, member) {
        }

        public static JSDotExpression New (JSExpression leftMost, params JSIdentifier[] memberNames) {
            if ((memberNames == null) || (memberNames.Length == 0))
                throw new ArgumentException("memberNames");

            var result = new JSDotExpression(leftMost, memberNames[0]);
            for (var i = 1; i < memberNames.Length; i++)
                result = new JSDotExpression(result, memberNames[i]);

            return result;
        }
    }

    // Represents a reference to a method of a type.
    public class JSMethodAccess : JSDotExpressionBase {
        public readonly bool IsStatic;

        public JSMethodAccess (JSExpression type, JSMethod method, bool isStatic)
            : base(type, method) {
            IsStatic = isStatic;
        }

        public JSExpression Type {
            get {
                return Values[0];
            }
        }

        public JSMethod Method {
            get {
                return (JSMethod)Values[1];
            }
        }
    }

    public class JSFieldAccess : JSDotExpressionBase {
        public JSFieldAccess (JSExpression thisReference, JSField field)
            : base(thisReference, field) {
        }

        public JSExpression ThisReference {
            get {
                return Values[0];
            }
        }

        public JSField Field {
            get {
                return (JSField)Values[1];
            }
        }
    }

    public class JSPropertyAccess : JSDotExpressionBase {
        public JSPropertyAccess (JSExpression thisReference, JSProperty property)
            : base(thisReference, property) {
        }

        public JSExpression ThisReference {
            get {
                return Values[0];
            }
        }

        public JSProperty Property {
            get {
                return (JSProperty)Values[1];
            }
        }
    }

    public class JSIndexerExpression : JSExpression {
        public TypeReference ElementType;

        public JSIndexerExpression (JSExpression target, JSExpression index, TypeReference elementType = null)
            : base(target, index) {

            ElementType = elementType;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (ElementType != null)
                return ElementType;

            var targetType = DeReferenceType(Target.GetActualType(typeSystem));

            var at = targetType as ArrayType;
            if (at != null)
                return at.ElementType;
            else
                return base.GetActualType(typeSystem);
        }

        public JSExpression Target {
            get {
                return Values[0];
            }
        }

        public JSExpression Index {
            get {
                return Values[1];
            }
        }

        public override bool IsConstant {
            get {
                return Target.IsConstant && Index.IsConstant;
            }
        }

        public override string ToString () {
            return String.Format("{0}[{1}]", Target, Index);
        }
    }

    public class JSNewExpression : JSExpression {
        public readonly MethodReference ConstructorReference;
        public readonly MethodInfo Constructor;

        public JSNewExpression (TypeReference type, MethodReference constructorReference, MethodInfo constructor, params JSExpression[] arguments)
            : this(new JSType(type), constructorReference, constructor, arguments) {
        }

        public JSNewExpression (JSExpression type, MethodReference constructorReference, MethodInfo constructor, params JSExpression[] arguments)
            : base((new[] { type }).Concat(arguments).ToArray()
        ) {
            ConstructorReference = constructorReference;
            Constructor = constructor;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            var type = Type as JSType;

            if (type != null)
                return type.Type;
            else if (Constructor != null)
                return Constructor.DeclaringType.Definition;
            else
                return typeSystem.Object;
        }

        public JSExpression Type {
            get {
                return Values[0];
            }
        }

        public IList<JSExpression> Arguments {
            get {
                return Values.Skip(1);
            }
        }
    }

    public class JSInvocationExpressionBase : JSExpression {
        protected JSInvocationExpressionBase (params JSExpression[] values)
            : base(values) {

        }
    }

    public class JSInvocationExpression : JSInvocationExpressionBase {
        public readonly bool ConstantIfArgumentsAre;
        public readonly bool ExplicitThis;

        protected JSInvocationExpression (
            JSExpression type, JSExpression method,
            JSExpression thisReference, JSExpression[] arguments,
            bool explicitThis, bool constantIfArgumentsAre
        )
            : base(
              (new[] { type, method, thisReference }).Concat(arguments ?? new JSExpression[0]).ToArray()
                ) {
            if (type == null)
                throw new ArgumentNullException("type");
            if (method == null)
                throw new ArgumentNullException("method");
            if (thisReference == null)
                throw new ArgumentNullException("thisReference");
            if ((arguments != null) && arguments.Any((a) => a == null))
                throw new ArgumentNullException("arguments");

            ExplicitThis = explicitThis;
            ConstantIfArgumentsAre = constantIfArgumentsAre;
        }

        public static JSInvocationExpression InvokeMethod (TypeReference type, JSIdentifier method, JSExpression thisReference, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return InvokeMethod(new JSType(type), method, thisReference, arguments, constantIfArgumentsAre);
        }

        public static JSInvocationExpression InvokeMethod (JSType type, JSIdentifier method, JSExpression thisReference, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return new JSInvocationExpression(
                type, method, thisReference, arguments, false, constantIfArgumentsAre
            );
        }

        public static JSInvocationExpression InvokeMethod (JSExpression method, JSExpression thisReference, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return new JSInvocationExpression(
                new JSNullExpression(), method, thisReference, arguments, false, constantIfArgumentsAre
            );
        }

        public static JSInvocationExpression InvokeBaseMethod (TypeReference type, JSIdentifier method, JSExpression thisReference, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return InvokeBaseMethod(new JSType(type), method, thisReference, arguments, constantIfArgumentsAre);
        }

        public static JSInvocationExpression InvokeBaseMethod (JSType type, JSIdentifier method, JSExpression thisReference, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return new JSInvocationExpression(
                type, method, thisReference, arguments, true, constantIfArgumentsAre
            );
        }

        public static JSInvocationExpression InvokeStatic (TypeReference type, JSIdentifier method, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return InvokeStatic(new JSType(type), method, arguments, constantIfArgumentsAre);
        }

        public static JSInvocationExpression InvokeStatic (JSType type, JSIdentifier method, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return new JSInvocationExpression(
                type, method, new JSNullExpression(), arguments, true, constantIfArgumentsAre
            );
        }

        public static JSInvocationExpression InvokeStatic (JSExpression method, JSExpression[] arguments = null, bool constantIfArgumentsAre = false) {
            return new JSInvocationExpression(
                new JSNullExpression(), method, new JSNullExpression(), arguments, true, constantIfArgumentsAre
            );
        }

        public IEnumerable<TypeReference> GenericArguments {
            get {
                var jsm = JSMethod;
                if (jsm != null)
                    return jsm.GenericArguments;

                return null;
            }
        }

        public JSExpression Type {
            get {
                return Values[0];
            }
        }

        public JSType JSType {
            get {
                return Values[0] as JSType;
            }
        }

        public JSExpression Method {
            get {
                return Values[1];
            }
        }

        public JSMethod JSMethod {
            get {
                return Values[1] as JSMethod;
            }
        }

        public JSFakeMethod FakeMethod {
            get {
                return Values[1] as JSFakeMethod;
            }
        }

        public JSExpression ThisReference {
            get {
                return Values[2];
            }
        }

        public virtual IList<JSExpression> Arguments {
            get {
                return Values.Skip(3);
            }
        }

        public IEnumerable<KeyValuePair<ParameterDefinition, JSExpression>> Parameters {
            get {
                var m = JSMethod;
                if (m == null) {
                    foreach (var a in Arguments)
                        yield return new KeyValuePair<ParameterDefinition, JSExpression>(null, a);
                } else {
                    var eParameters = m.Method.Parameters.GetEnumerator();
                    using (var eArguments = Arguments.GetEnumerator()) {
                        ParameterDefinition currentParameter, lastParameter = null;

                        while (eArguments.MoveNext()) {
                            if (eParameters.MoveNext()) {
                                currentParameter = eParameters.Current as ParameterDefinition;
                            } else {
                                currentParameter = lastParameter;
                            }

                            yield return new KeyValuePair<ParameterDefinition, JSExpression>(
                                currentParameter, eArguments.Current
                            );

                            lastParameter = currentParameter;
                        }
                    }
                }
            }
        }

        public override bool IsConstant {
            get {
                if (ConstantIfArgumentsAre)
                    return (Arguments.All((a) => a.IsConstant) && ThisReference.IsConstant) ||
                        base.IsConstant;
                else
                    return base.IsConstant;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            var targetType = Method.GetActualType(typeSystem);

            var targetAbstractMethod = Method.SelfAndChildrenRecursive.OfType<JSIdentifier>()
                .LastOrDefault((i) => {
                    var m = i as JSMethod;
                    var fm = i as JSFakeMethod;
                    return (m != null) || (fm != null);
                });

            var targetMethod = JSMethod ?? (targetAbstractMethod as JSMethod);
            var targetFakeMethod = FakeMethod ?? (targetAbstractMethod as JSFakeMethod);

            if (targetMethod != null)
                return SubstituteTypeArgs(targetMethod.Method.Source, targetMethod.Reference.ReturnType, targetMethod.Reference);
            else if (targetFakeMethod != null)
                return targetFakeMethod.ReturnType;

            // Any invocation expression targeting a method or delegate will have an expected type that is a delegate.
            // This should be handled by replacing the JSInvocationExpression with a JSDelegateInvocationExpression
            if (TypeUtil.IsDelegateType(targetType))
                throw new NotImplementedException("Invocation with a target type that is a delegate");

            return targetType;
        }

        public override string ToString () {
            return String.Format(
                "{0}(this={1}, args={2})",
                Method,
                ThisReference,
                String.Join(", ", (from a in Arguments select String.Concat(a)).ToArray())
            );
        }
    }

    public class JSDelegateInvocationExpression : JSInvocationExpressionBase {
        public readonly TypeReference ReturnType;

        public JSDelegateInvocationExpression (
            JSExpression thisReference, TypeReference returnType, JSExpression[] arguments
        )
            : base(
                (new[] { thisReference }).Concat(arguments).ToArray()
            ) {

            if (thisReference == null)
                throw new ArgumentNullException("thisReference");
            if (returnType == null)
                throw new ArgumentNullException("returnType");

            ReturnType = returnType;
        }

        public JSExpression ThisReference {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return ReturnType;
        }

        public virtual IList<JSExpression> Arguments {
            get {
                return Values.Skip(1);
            }
        }

        public override string ToString () {
            return String.Format(
                "{0}:({1})",
                ThisReference,
                String.Join(", ", (from a in Arguments select String.Concat(a)).ToArray())
            );
        }
    }

    public class JSInitializerApplicationExpression : JSExpression {
        public JSInitializerApplicationExpression (JSExpression target, JSExpression initializer)
            : base(target, initializer) {
        }

        public JSExpression Target {
            get {
                return Values[0];
            }
        }

        public JSExpression Initializer {
            get {
                return Values[1];
            }
        }

        public override bool IsConstant {
            get {
                return Target.IsConstant && Initializer.IsConstant;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Target.GetActualType(typeSystem);
        }
    }

    public class JSArrayExpression : JSExpression {
        public readonly TypeReference ElementType;

        public JSArrayExpression (TypeReference elementType, params JSExpression[] values)
            : base(values) {

            ElementType = elementType;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (ElementType != null)
                return new ArrayType(ElementType);
            else
                return base.GetActualType(typeSystem);
        }

        new public IEnumerable<JSExpression> Values {
            get {
                return base.Values;
            }
        }

        public override bool IsConstant {
            get {
                return Values.All((v) => v.IsConstant);
            }
        }

        private static JSExpression[] DecodeValues<T> (int totalBytes, Func<T> getNextValue) {
            var result = new JSExpression[totalBytes / Marshal.SizeOf(typeof(T))];
            for (var i = 0; i < result.Length; i++)
                result[i] = JSLiteral.New(getNextValue() as dynamic);
            return result;
        }

        public static JSExpression UnpackArrayInitializer (TypeReference arrayType, byte[] data) {
            var elementType = TypeUtil.DereferenceType(arrayType).GetElementType();
            JSExpression[] values;

            using (var ms = new MemoryStream(data, false))
            using (var br = new BinaryReader(ms))
                switch (elementType.FullName) {
                    case "System.Byte":
                        values = DecodeValues(data.Length, br.ReadByte);
                        break;
                    case "System.UInt16":
                        values = DecodeValues(data.Length, br.ReadUInt16);
                        break;
                    case "System.UInt32":
                        values = DecodeValues(data.Length, br.ReadUInt32);
                        break;
                    case "System.UInt64":
                        values = DecodeValues(data.Length, br.ReadUInt64);
                        break;
                    case "System.SByte":
                        values = DecodeValues(data.Length, br.ReadSByte);
                        break;
                    case "System.Int16":
                        values = DecodeValues(data.Length, br.ReadInt16);
                        break;
                    case "System.Int32":
                        values = DecodeValues(data.Length, br.ReadInt32);
                        break;
                    case "System.Int64":
                        values = DecodeValues(data.Length, br.ReadInt64);
                        break;
                    case "System.Single":
                        values = DecodeValues(data.Length, br.ReadSingle);
                        break;
                    case "System.Double":
                        values = DecodeValues(data.Length, br.ReadDouble);
                        break;
                    default:
                        return new JSUntranslatableExpression(String.Format("Array initializers with element type '{0}' not implemented", elementType.FullName));
                }

            return new JSArrayExpression(elementType, values);
        }
    }

    public class JSPairExpression : JSExpression {
        public JSPairExpression (JSExpression key, JSExpression value)
            : base(key, value) {
        }

        public JSExpression Key {
            get {
                return Values[0];
            }
            set {
                Values[0] = value;
            }
        }

        public JSExpression Value {
            get {
                return Values[1];
            }
            set {
                Values[1] = value;
            }
        }

        public override bool IsConstant {
            get {
                return Values.All((v) => v.IsConstant);
            }
        }
    }

    public class JSMemberDescriptor : JSExpression {
        public readonly bool IsPublic;
        public readonly bool IsStatic;

        public JSMemberDescriptor (bool isPublic, bool isStatic)
            : base() {

            IsPublic = isPublic;
            IsStatic = isStatic;
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Object;
        }
    }

    public class JSObjectExpression : JSExpression {
        public JSObjectExpression (params JSPairExpression[] pairs)
            : base(
                pairs
                ) {
        }

        public override bool IsConstant {
            get {
                return Values.All((v) => v.IsConstant);
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Object;
        }

        new public IEnumerable<JSPairExpression> Values {
            get {
                return from v in base.Values select (JSPairExpression)v;
            }
        }
    }

    public abstract class JSOperatorExpression<TOperator> : JSExpression
        where TOperator : JSOperator {

        public readonly TOperator Operator;
        public readonly TypeReference ActualType;

        protected JSOperatorExpression (TOperator op, TypeReference actualType, params JSExpression[] values)
            : base(values) {

            Operator = op;
            ActualType = actualType;
        }

        public override bool IsConstant {
            get {
                throw new NotImplementedException("IsConstant was not implemented");
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (ActualType != null)
                return ActualType;

            TypeReference inferredType = null;
            foreach (var value in Values) {
                var valueType = value.GetActualType(typeSystem);

                if (inferredType == null)
                    inferredType = valueType;
                else if (valueType.FullName == inferredType.FullName)
                    continue;
                else
                    return base.GetActualType(typeSystem);
            }

            return inferredType;
        }
    }

    public class JSTernaryOperatorExpression : JSExpression {
        public readonly TypeReference ActualType;

        public JSTernaryOperatorExpression (JSExpression condition, JSExpression trueValue, JSExpression falseValue, TypeReference actualType)
            : base(condition, trueValue, falseValue) {

            ActualType = actualType;
        }

        public JSExpression Condition {
            get {
                return Values[0];
            }
        }

        public JSExpression True {
            get {
                return Values[1];
            }
        }

        public JSExpression False {
            get {
                return Values[2];
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Condition.HasGlobalStateDependency || True.HasGlobalStateDependency || False.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                return Condition.IsConstant && True.IsConstant && False.IsConstant;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return ActualType;
        }
    }

    public class JSBinaryOperatorExpression : JSOperatorExpression<JSBinaryOperator> {
        /// <summary>
        /// Construct a binary operator expression with an explicit expected type.
        /// If the explicit expected type is null, expected type will be inferred to be the type of both sides if they share a type.
        /// </summary>
        public JSBinaryOperatorExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeReference actualType)
            : base(
                op, actualType, lhs, rhs
                ) {
        }

        public JSExpression Left {
            get {
                return Values[0];
            }
            set {
                Values[0] = value;
            }
        }

        public JSExpression Right {
            get {
                return Values[1];
            }
            set {
                Values[1] = value;
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Left.HasGlobalStateDependency || Right.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                if (Operator is JSAssignmentOperator)
                    return false;

                return Left.IsConstant && Right.IsConstant;
            }
        }

        public static JSBinaryOperatorExpression New (JSBinaryOperator op, IList<JSExpression> values, TypeReference expectedType) {
            if (values.Count < 2)
                throw new ArgumentException();

            var result = new JSBinaryOperatorExpression(
                op, values[0], values[1], expectedType
            );
            var current = result;

            for (int i = 2, c = values.Count; i < c; i++) {
                var next = new JSBinaryOperatorExpression(op, current.Right, values[i], expectedType);
                current.Right = next;
                current = next;
            }

            return result;
        }

        public override string ToString () {
            return String.Format("({0} {1} {2})", Left, Operator, Right);
        }
    }

    public class JSUnaryOperatorExpression : JSOperatorExpression<JSUnaryOperator> {
        public JSUnaryOperatorExpression (JSUnaryOperator op, JSExpression expression, TypeReference actualType = null)
            : base(op, actualType, expression) {
        }

        public bool IsPostfix {
            get {
                return ((JSUnaryOperator)Operator).IsPostfix;
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Expression.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                if (Operator is JSUnaryMutationOperator)
                    return false;

                return Expression.IsConstant;
            }
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }

        public override string ToString () {
            if (IsPostfix)
                return String.Format("({0}{1})", Expression, Operator);
            else
                return String.Format("({0}{1})", Operator, Expression);
        }
    }

    public class JSCastExpression : JSExpression {
        public readonly TypeReference NewType;

        protected JSCastExpression (JSExpression inner, TypeReference newType)
            : base(inner) {

            NewType = newType;
        }

        public static JSExpression New (JSExpression inner, TypeReference newType, TypeSystem typeSystem) {
            int temp;

            var currentType = inner.GetActualType(typeSystem);
            var currentDerefed = TypeUtil.FullyDereferenceType(currentType, out temp);
            var newDerefed = TypeUtil.FullyDereferenceType(newType, out temp);

            if (TypeUtil.TypesAreEqual(currentDerefed, newDerefed, false))
                return inner;

            var newResolved = newDerefed.Resolve();
            if ((newResolved != null) && newResolved.IsInterface) {
                var currentResolved = currentDerefed.Resolve();

                if (currentResolved != null) {
                    foreach (var iface in currentResolved.Interfaces) {
                        if (TypeUtil.TypesAreEqual(newType, iface, false))
                            return JSChangeTypeExpression.New(inner, typeSystem, newType);
                    }
                }
            }

            var nullLiteral = inner as JSNullLiteral;
            if (nullLiteral != null)
                return new JSNullLiteral(newType);

            return new JSCastExpression(inner, newType);
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Expression.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                return Expression.IsConstant;
            }
        }

        public override bool IsNull {
            get {
                return Expression.IsNull;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return NewType;
        }
    }

    public class JSValueOfNullableExpression : JSExpression {
        public JSValueOfNullableExpression (JSExpression inner)
            : base(inner) {
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Expression.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                return Expression.IsConstant;
            }
        }

        public override bool IsNull {
            get {
                return Expression.IsNull;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Expression.GetActualType(typeSystem);
        }

        public override string ToString () {
            return String.Format("ValueOf({0})", Expression);
        }
    }

    public class JSChangeTypeExpression : JSExpression {
        public readonly TypeReference NewType;

        protected JSChangeTypeExpression (JSExpression inner, TypeReference newType)
            : base(inner) {

            NewType = newType;
        }

        public static JSExpression New (JSExpression inner, TypeSystem typeSystem, TypeReference newType) {
            var cte = inner as JSChangeTypeExpression;
            JSChangeTypeExpression result;

            if (cte != null) {
                inner = cte.Expression;

                result = new JSChangeTypeExpression(cte.Expression, newType);
            } else {
                result = new JSChangeTypeExpression(inner, newType);
            }

            var innerType = inner.GetActualType(typeSystem);
            if (TypeUtil.TypesAreEqual(newType, innerType))
                return inner;
            else
                return result;
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Expression.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                return Expression.IsConstant;
            }
        }

        public override bool IsNull {
            get {
                return Expression.IsNull;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return NewType;
        }
    }

    public class JSStructCopyExpression : JSExpression {
        public JSStructCopyExpression (JSExpression @struct)
            : base(@struct) {
        }

        public JSExpression Struct {
            get {
                return Values[0];
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Struct.HasGlobalStateDependency;
            }
        }

        public override bool IsConstant {
            get {
                return Struct.IsConstant;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Struct.GetActualType(typeSystem);
        }

        public override bool IsNull {
            get {
                return Struct.IsNull;
            }
        }
    }
}
