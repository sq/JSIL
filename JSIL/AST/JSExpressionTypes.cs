using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Ast {
    [JSAstIgnoreInheritedMembers]
    public class JSFunctionExpression : JSExpression {
        public readonly MethodTypeFactory MethodTypes;

        public readonly JSMethod Method;

        public readonly Dictionary<string, JSVariable> AllVariables;
        // This has to be JSVariable, because 'this' is of type (JSVariableReference<JSThisParameter>) for structs
        // We also need to make this an IEnumerable, so it can be a select expression instead of a constant array
        // If we were to convert it to an array, later changes to the data the enumerable pulls from wouldn't update it
        [JSAstTraverse(0, "FunctionSignature")]
        public readonly JSVariable[] Parameters;
        [JSAstTraverse(1)]
        public readonly JSBlockStatement Body;

        public int TemporaryVariableCount = 0;

        public string DisplayName = null;

        public JSFunctionExpression (
            JSMethod method, Dictionary<string, JSVariable> allVariables,
            JSVariable[] parameters, JSBlockStatement body,
            MethodTypeFactory methodTypes
        ) {
            Method = method;
            AllVariables = allVariables;
            Parameters = parameters;
            Body = body;
            MethodTypes = methodTypes;
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


    public class JSThrowExpression : JSExpression {
        public JSThrowExpression (JSExpression exception)
            : base(exception) {
        }

        public JSExpression Exception {
            get {
                return Values[0];
            }
        }

        public override string ToString () {
            return String.Format("throw {0}", Exception);
        }
    }

    public abstract class JSFlowControlExpression : JSExpression {
        protected JSFlowControlExpression (params JSExpression[] values)
            :base (values) {
        }
    }

    public class JSGotoExpression : JSFlowControlExpression {
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

    public class JSExitLabelGroupExpression : JSGotoExpression {
        [JSAstIgnore]
        public readonly JSLabelGroupStatement LabelGroup;

        public JSExitLabelGroupExpression (JSLabelGroupStatement labelGroup)
            : base("$exit" + labelGroup.GroupIndex) {

            LabelGroup = labelGroup;
        }

        public override string ToString () {
            return String.Format("exit labelgroup {0}", LabelGroup.GroupIndex);
        }
    }

    // Technically, the following expressions should be statements. But in ILAst, they're expressions...
    public class JSReturnExpression : JSFlowControlExpression {
        public JSReturnExpression (JSExpression value = null)
            : base(value) {
        }

        public JSExpression Value {
            get {
                return Values[0];
            }
        }

        public override string ToString () {
            return String.Format("return {0}", Value);
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSReturnExpression;

            if (rhs == null)
                return base.Equals(obj);

            return Value.Equals(rhs.Value);
        }
    }

    public class JSBreakExpression : JSFlowControlExpression {
        public int? TargetLoop;

        public override string ToString () {
            if (TargetLoop.HasValue)
                return String.Format("break $loop{0}", TargetLoop);
            else
                return "break";
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSBreakExpression;

            if (rhs == null)
                return base.Equals(obj);

            return TargetLoop.Equals(rhs.TargetLoop);
        }
    }

    public class JSContinueExpression : JSFlowControlExpression {
        public int? TargetLoop;

        public override string ToString () {
            if (TargetLoop.HasValue)
                return String.Format("continue $loop{0}", TargetLoop);
            else
                return "continue";
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSContinueExpression;

            if (rhs == null)
                return base.Equals(obj);

            return TargetLoop.Equals(rhs.TargetLoop);
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
            var cte = reference as JSChangeTypeExpression;
            bool isCast = false, isCte = false;

            if (cast != null) {
                isCast = true;
                reference = cast.Expression;
            } else if (cte != null) {
                isCte = true;
                reference = cte.Expression;
            }

            Func<JSExpression, JSExpression> recast = (expression) => {
                if (isCast)
                    return JSCastExpression.New(expression, TypeUtil.DereferenceType(cast.NewType), jsil.TypeSystem);
                else if (isCte)
                    return JSChangeTypeExpression.New(expression, TypeUtil.DereferenceType(cte.NewType), jsil.TypeSystem);
                else
                    return expression;
            };

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

                    referent = recast(rv);

                    return true;
                }
            }

            if (rre != null) {
                if (rre.Depth == 1) {
                    referent = recast(rre.Referent);

                    return true;
                } else {
                    referent = recast(rre.Dereference());

                    return true;
                }
            }

            if (refe == null) {
                referent = null;
                return false;
            }

            referent = recast(refe.Referent);

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

            JSIndexerExpression indexer = null;
            JSReferenceExpression nestedReference = null;
            var variable = reference as JSVariable;

            if (variable == null) {
                var refe = reference as JSReferenceExpression;
                if (refe != null) {
                    variable = refe.Referent as JSVariable;
                    indexer = refe.Referent as JSIndexerExpression;
                    nestedReference = refe.Referent as JSReferenceExpression;
                }
            }

            if (variable != null) {
                if (variable.IsReference) {
                    materialized = variable.Dereference();
                    return true;
                } else {
                    // FIXME: This probably shouldn't happen?
                    materialized = null;
                    return false;
                }
            } else if (indexer != null) {
                materialized = jsil.NewElementReference(
                    indexer.Target, indexer.Index
                );
                return true;
            } else if (nestedReference != null) {
                return TryMaterialize(jsil, nestedReference, out materialized);
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
            return String.Format("ref({0})", Referent);
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

        public JSNullExpression ()
            : base() {
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

    public abstract class JSIgnoredExpression : JSExpression {
        public readonly bool ThrowError;

        protected JSIgnoredExpression (bool throwError) {
            ThrowError = throwError;
        }
    }

    public class JSIgnoredMemberReference : JSIgnoredExpression {
        public readonly IMemberInfo Member;
        [JSAstIgnore]
        public readonly JSExpression[] Arguments;

        public JSIgnoredMemberReference (bool throwError, IMemberInfo member, params JSExpression[] arguments)
            : base (throwError) {
            Member = member;
            Arguments = arguments;
        }

        public override string ToString () {
            if (Member != null)
                return String.Format("Reference to ignored member {0}", Member.Name);
            else
                return "Reference to ignored member (no info)";
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

    public class JSIgnoredTypeReference : JSIgnoredExpression {
        public readonly TypeReference Type;

        public JSIgnoredTypeReference (bool throwError, TypeReference type)
            : base(throwError) {
            Type = type;
        }

        public override string ToString () {
            return String.Format("Reference to ignored type {0}", Type.FullName);
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSIgnoredTypeReference;
            if (rhs != null) {
                if (!TypeUtil.TypesAreEqual(Type, rhs.Type))
                    return false;

                return true;
            }

            return EqualsImpl(obj, true);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Void;
        }
    }

    public interface ITypeOfExpression {
        TypeReference Type { get; }
    }

    public class JSTypeOfExpression : JSType, ITypeOfExpression {
        public JSTypeOfExpression (TypeReference type) 
            : base (type) {
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

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return new TypeReference("System", "Type", typeSystem.Object.Module, typeSystem.Object.Scope);
        }

        public override string ToString () {
            return String.Format("typeof ({0})", base.ToString());
        }

        public new TypeReference Type {
            get { return base.Type; }
        }
    }

    public class JSCachedTypeOfExpression : JSCachedType, ITypeOfExpression {
        public JSCachedTypeOfExpression (TypeReference type, int index) 
            : base (type, index) {
        }

        public new TypeReference Type {
            get { return base.Type; }
        }
    }

    public class JSPublicInterfaceOfExpression : JSExpression {
        public JSPublicInterfaceOfExpression (JSExpression inner)
            : base(inner) {
        }

        public JSExpression Inner {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Inner.GetActualType(typeSystem);
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
        static JSDotExpressionBase () {
            SetValueNames(
                typeof(JSDotExpressionBase),
                "Target",
                "Member"
            );
        }

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

        public override bool HasGlobalStateDependency
        {
            get { return true; }
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

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Field.Field.FieldType;
        }
    }

    public class JSPropertyAccess : JSDotExpressionBase {
        public readonly bool IsWrite;
        public readonly bool IsVirtualCall;
        public readonly bool TypeQualified;

        public readonly JSMethod OriginalMethod;

        public JSPropertyAccess (
            JSExpression thisReference, JSProperty property, 
            bool isWrite, bool typeQualified, JSMethod originalMethod,
            bool isVirtualCall
        )
            : base(thisReference, property) {

            IsWrite = isWrite;
            IsVirtualCall = isVirtualCall;
            TypeQualified = typeQualified;
            OriginalMethod = originalMethod;
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

        static JSIndexerExpression () {
            SetValueNames(
                typeof(JSIndexerExpression),
                "Target",
                "OffsetInBytes"
            );
        }

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

        public IEnumerable<KeyValuePair<ParameterDefinition, JSExpression>> Parameters {
            get {
                return JSInvocationExpression.GetParameterSequenceForMethod(Constructor, Arguments);
            }
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSNewArrayExpression : JSExpression {
        public readonly bool IsMultidimensional;
        public readonly TypeReference ElementType;
        public readonly ArrayType ArrayType;

        [JSAstTraverse(0)]
        public readonly JSExpression[] Dimensions;
        [JSAstTraverse(1)]
        public JSExpression SizeOrArrayInitializer;

        public int? CachedElementTypeIndex;

        public JSNewArrayExpression (TypeReference elementType, JSExpression sizeOrArrayInitializer) {
            ElementType = elementType;
            ArrayType = new ArrayType(elementType);
            IsMultidimensional = false;
            SizeOrArrayInitializer = sizeOrArrayInitializer;
        }

        public JSNewArrayExpression (TypeReference elementType, JSExpression[] dimensions, JSExpression initializer = null) {
            ElementType = elementType;
            ArrayType = new ArrayType(elementType, dimensions.Length);
            IsMultidimensional = true;
            Dimensions = dimensions;
            SizeOrArrayInitializer = initializer;
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (Dimensions != null) {
                for (var i = 0; i < Dimensions.Length; i++) {
                    if (Dimensions[i] == oldChild)
                        Dimensions[i] = (JSExpression)newChild;
                }
            }

            if (SizeOrArrayInitializer == oldChild) {
                SizeOrArrayInitializer = (JSExpression)newChild;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return ArrayType;
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

        protected TypeReference _ActualType = null;

        static JSInvocationExpression () {
            SetValueNames(
                typeof(JSInvocationExpression),
                "Type",
                "Method",
                "ThisReference",
                "Argument"
            );
        }

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

        public IEnumerable<JSCachedType> CachedGenericArguments {
            get {
                var jsm = JSMethod;
                if (jsm != null)
                    return jsm.CachedGenericArguments;

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

                if (m == null)
                    return GetParameterSequenceForMethod(null, Arguments);
                else
                    return GetParameterSequenceForMethod(m.Method, Arguments);
            }
        }

        public static IEnumerable<KeyValuePair<ParameterDefinition, JSExpression>> GetParameterSequenceForMethod (MethodInfo methodInfo, IList<JSExpression> arguments) {
            if (methodInfo == null) {
                foreach (var a in arguments)
                    yield return new KeyValuePair<ParameterDefinition, JSExpression>(null, a);
            } else {
                var eParameters = methodInfo.Parameters.GetEnumerator();
                using (var eArguments = arguments.GetEnumerator()) {
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
            if (_ActualType != null)
                return _ActualType;

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
                return _ActualType = SubstituteTypeArgs(targetMethod.Method.Source, targetMethod.Reference.ReturnType, targetMethod.Reference);
            else if (targetFakeMethod != null)
                return _ActualType = targetFakeMethod.ReturnType;

            // Any invocation expression targeting a method or delegate will have an expected type that is a delegate.
            // This should be handled by replacing the JSInvocationExpression with a JSDelegateInvocationExpression
            if (TypeUtil.IsDelegateType(targetType))
                throw new NotImplementedException("Invocation with a target type that is a delegate");

            return _ActualType = targetType;
        }

        public string BuildArgumentListString () {
            var result = new StringBuilder();

            if ((ThisReference != null) && !ThisReference.IsNull)
                result.AppendFormat("this: {0}", ThisReference);

            var jsm = JSMethod;
            var args = Arguments;
            string argName;

            for (var i = 0; i < args.Count; i++) {
                if (result.Length > 0)
                    result.Append(", ");

                if (jsm != null) {
                    var parms = jsm.Method.Parameters;

                    if (i >= parms.Length) {
                        argName = String.Format(
                            "{0}[{1}]", 
                            jsm.Method.Parameters[parms.Length - 1].Name, 
                            i - parms.Length + 1
                        );
                    } else {
                        if (parms[i].CustomAttributes.Any((ca) => ca.AttributeType.Name.Contains("ParamArray")))
                            argName = String.Format("{0}[0]", parms[i].Name);
                        else
                            argName = parms[i].Name;
                    }
                } else {
                    argName = String.Format("arg{0}", i);
                }

                result.AppendFormat("{0}: {1}", argName, args[i]);
            }

            return result.ToString();
        }

        public override string ToString () {
            var arglist = BuildArgumentListString();

            return String.Format(
                "{0}({1})",
                Method,
                arglist
            );
        }
    }

    public class JSDelegateInvocationExpression : JSInvocationExpressionBase {
        public readonly TypeReference ReturnType;

        static JSDelegateInvocationExpression () {
            SetValueNames(
                typeof(JSDelegateInvocationExpression),
                "Delegate",
                "Argument"
            );
        }

        public JSDelegateInvocationExpression (
            JSExpression @delegate, TypeReference returnType, JSExpression[] arguments
        )
            : base(
                (new[] { @delegate }).Concat(arguments).ToArray()
            ) {

            if (@delegate == null)
                throw new ArgumentNullException("delegate");
            if (returnType == null)
                throw new ArgumentNullException("returnType");

            ReturnType = returnType;
        }

        public JSExpression Delegate {
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
                Delegate,
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

    public class JSNestedObjectInitializerExpression : JSExpression {
        public JSNestedObjectInitializerExpression (JSExpression initializer)
            : base(initializer) {
        }

        public JSExpression Initializer {
            get {
                return Values[0];
            }
        }

        public override bool IsConstant {
            get {
                return Initializer.IsConstant;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Initializer.GetActualType(typeSystem);
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
        static JSPairExpression () {
            SetValueNames(
                typeof(JSPairExpression),
                "Key",
                "Value"
            );
        }

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
        public readonly bool IsVirtual;
        public readonly bool IsReadonly;

        public JSMemberDescriptor (bool isPublic, bool isStatic, bool isVirtual = false, bool isReadonly = false)
            : base() {

            IsPublic = isPublic;
            IsStatic = isStatic;
            IsVirtual = isVirtual;
            IsReadonly = isReadonly;
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

    public abstract class JSOperatorExpressionBase : JSExpression {
        protected JSOperatorExpressionBase (params JSExpression[] values)
            : base (values) {
        }
    }

    public abstract class JSOperatorExpression<TOperator> : JSOperatorExpressionBase
        where TOperator : JSOperator {

        public readonly TOperator Operator;
        public readonly TypeReference ActualType;

        protected JSOperatorExpression (TOperator op, TypeReference actualType, params JSExpression[] values)
            : base(values) {

            if (actualType == null)
                throw new ArgumentNullException("actualType");

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

        static JSTernaryOperatorExpression () {
            SetValueNames(
                typeof(JSTernaryOperatorExpression),
                "Condition",
                "True Clause",
                "False Clause"
            );
        }

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
        static JSBinaryOperatorExpression () {
            SetValueNames(
                typeof(JSBinaryOperatorExpression),
                "Left",
                "Right"
            );
        }

        /// <summary>
        /// Construct a binary operator expression with an explicit expected type.
        /// </summary>
        public JSBinaryOperatorExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeReference actualType)
            : base(
                op, actualType, lhs, rhs
                ) {
        }

        protected JSBinaryOperatorExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeReference actualType, params JSExpression[] extraValues)
            : base(
                op, actualType, new[] { lhs, rhs }.Concat(extraValues).ToArray()
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
        static JSUnaryOperatorExpression () {
            SetValueNames(
                typeof(JSUnaryOperatorExpression),
                "Expression"
            );
        }

        public JSUnaryOperatorExpression (JSUnaryOperator op, JSExpression expression, TypeReference actualType)
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

    public class JSIsExpression : JSExpression {
        public int? CachedTypeIndex;
        public readonly TypeReference Type;

        public JSIsExpression (JSExpression inner, TypeReference type)
            : base(inner) {

            Type = type;
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
            return typeSystem.Boolean;
        }
    }

    public class JSAsExpression : JSCastExpression {
        protected JSAsExpression (JSExpression inner, TypeReference newType)
            : base(inner, newType, false) {
        }

        public static JSExpression New (JSExpression inner, TypeReference newType, TypeSystem typeSystem) {
            return NewInternal(
                inner, newType, typeSystem, false,
                () => new JSAsExpression(inner, newType)
            );
        }
    }

    public class JSCastExpression : JSExpression {
        public int? CachedTypeIndex;

        public readonly bool IsCoercion;
        public readonly TypeReference NewType;

        protected JSCastExpression (JSExpression inner, TypeReference newType, bool isCoercion = false)
            : base(inner) {

            NewType = newType;
            IsCoercion = isCoercion;
        }

        public static JSExpression New (
            JSExpression inner, TypeReference newType, TypeSystem typeSystem, bool force = false, bool isCoercion = false
        ) {
            return NewInternal(
                inner, newType, typeSystem, force,
                () => new JSCastExpression(inner, newType, isCoercion)
            );
        }

        internal static bool AllowBizarreReferenceCast (TypeReference currentType, TypeReference newType) {
            if (currentType.FullName == "System.Boolean") {
                if (newType.FullName == "System.SByte")
                    return true;
            }

            if (TypeUtil.IsEnum(currentType) && TypeUtil.IsIntegral(newType))
                return true;

            // Allow casting T*& to U*&
            if (TypeUtil.IsPointer(currentType) && TypeUtil.IsPointer(newType))
                return true;

            return false;
        }

        internal static JSExpression NewInternal (JSExpression inner, TypeReference newType, TypeSystem typeSystem, bool force, Func<JSExpression> make) {
            int rankCurrent, rankNew;

            var currentType = inner.GetActualType(typeSystem);
            var currentDerefed = TypeUtil.FullyDereferenceType(currentType, out rankCurrent);
            var newDerefed = TypeUtil.FullyDereferenceType(newType, out rankNew);

            if (TypeUtil.TypesAreEqual(currentDerefed, newDerefed, false) && !force)
                return inner;

            if (
                (rankCurrent == rankNew) && 
                (rankCurrent > 0)
            ) {
                if (!AllowBizarreReferenceCast(currentDerefed, newDerefed))
                    return new JSUntranslatableExpression("Cannot cast reference of type '" + currentDerefed.FullName + "' to type '" + newDerefed.FullName + "'");
                else {
                    if (TypeUtil.IsPointer(currentDerefed) && TypeUtil.IsPointer(newDerefed))
                        return JSChangeTypeExpression.New(inner, newType, typeSystem);
                    else
                        return make();
                }
            }

            var newResolved = newDerefed.Resolve();
            if (!force && (newResolved != null) && newResolved.IsInterface) {
                var currentResolved = currentDerefed.Resolve();

                if (currentResolved != null) {
                    // HACK: Because Arrays are IEnumerable, normally we would suppress the cast.
                    // In this case we need to ensure it happens so that an array overlay is created.
                    if (
                        currentType.IsArray && 
                        (newType.FullName == "System.Collections.IEnumerable")
                    )
                        return make();

                    foreach (var iface in currentResolved.Interfaces) {
                        if (TypeUtil.TypesAreEqual(newType, iface, false))
                            return JSChangeTypeExpression.New(inner, newType, typeSystem);
                    }
                }
            }

            var nullLiteral = inner as JSNullLiteral;
            if (nullLiteral != null)
                return new JSNullLiteral(newType);

            if (TypeUtil.IsPointer(newType)) {
                if (TypeUtil.IsPointer(currentType)) {
                    return new JSPointerCastExpression(inner, newType);
                } else {
                    while (inner is JSReferenceExpression)
                        inner = ((JSReferenceExpression)inner).Referent;

                    var innerType = inner.GetActualType(typeSystem);
                    var indexer = inner as JSIndexerExpression;
                    var invocation = inner as JSInvocationExpression;
                    if (indexer != null) {
                        return new JSPinExpression(indexer.Target, indexer.Index, newType);
                    } else if (
                        (invocation != null) && 
                        (invocation.ThisReference != null) &&
                        (invocation.JSMethod != null) &&
                        (
                            invocation.JSMethod.Method.Name.EndsWith("Get") ||
                            invocation.JSMethod.Method.Name.EndsWith("GetReference")
                        ) &&
                        PackedArrayUtil.IsPackedArrayType(invocation.ThisReference.GetActualType(typeSystem))
                    ) {
                        var offsetInElements = invocation.Arguments[0];

                        return new JSPinExpression(
                            invocation.ThisReference, 
                            offsetInElements, 
                            newType
                        );
                    } else if (TypeUtil.IsArray(innerType)) {
                        return new JSPinExpression(inner, null, newType);
                    } else if (TypeUtil.IsIntegral(innerType)) {
                        var literal = inner as JSIntegerLiteral;
                        if (literal != null) {
                            return new JSPointerLiteral(literal.Value, newType);
                        } else if (TypeUtil.IsIntPtr(newType)) {
                            // IntPtr has stupid coercion rules.
                            return JSChangeTypeExpression.New(inner, newType, typeSystem);
                        } else {
                            return new JSUntranslatableExpression("Conversion of non-constant integral expression '" + inner + "' to pointer");
                        }
                    } else if (PackedArrayUtil.IsPackedArrayType(innerType)) {
                        return new JSPinExpression(inner, null, newType);
                    } else {
                        return new JSUntranslatableExpression("Conversion of expression '" + inner + "' to pointer");
                    }
                }
            }

            return make();
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

        public override string ToString () {
            return String.Format("({0}){1}", NewType, Expression);
        }
    }

    public class JSTruncateExpression : JSExpression {
        public JSTruncateExpression (JSExpression inner) 
            : base (inner) {
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
            return typeSystem.Int32;
        }

        public override string ToString () {
            return String.Format("(int){0}", Expression);
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

        public static JSExpression New (JSExpression inner, TypeReference newType, TypeSystem typeSystem) {
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

        public override string ToString () {
            return String.Format("reinterpret_cast<{0}>({1})", NewType, Expression);
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

    public class JSConditionalStructCopyExpression : JSStructCopyExpression {
        public readonly GenericParameter Parameter;

        public JSConditionalStructCopyExpression (GenericParameter parameter, JSExpression @struct)
            : base(@struct) {
            Parameter = parameter;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Struct.GetActualType(typeSystem);
        }
    }

    public class JSCommaExpression : JSExpression {
        public JSCommaExpression (params JSExpression[] subExpressions) 
            : base (subExpressions) {
        }

        public IEnumerable<JSExpression> SubExpressions {
            get {
                return Values;
            }
        }

        public override bool Equals (object obj) {
            return EqualsImpl(obj, true);
        }

        public override string ToString () {
            return "(" + String.Join(",", (object[])Values) + ")";
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Values.Last().GetActualType(typeSystem);
        }
    }

    public class JSWriteThroughReferenceExpression : JSBinaryOperatorExpression {
        public JSWriteThroughReferenceExpression (JSVariable referenceVariable, JSExpression rhs)
            : base(
                JSOperator.Assignment, referenceVariable, rhs, referenceVariable.IdentifierType
            ) {
        }

        public override string ToString () {
            return String.Format("{0}.set({1})", Left, Right);
        }
    }

    public class JSReadThroughReferenceExpression : JSExpression {
        public JSReadThroughReferenceExpression (JSVariable variable)
            : base (variable) {
        }

        public JSVariable Variable {
            get {
                return (JSVariable)Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return DeReferenceType(Variable.GetActualType(typeSystem), true);
        }

        public override string ToString () {
            return String.Format("{0}.get()", Variable);
        }
    }

    public class JSPinExpression : JSExpression {
        public readonly TypeReference PointerType;

        public JSPinExpression (JSExpression array, JSExpression arrayIndex, TypeReference pointerType)
            : base (array, arrayIndex) {
            // FIXME: Verify that the pointer type is valid for the array.
            PointerType = pointerType;
        }

        public JSExpression Array {
            get {
                return Values[0];
            }
        }

        public JSExpression ArrayIndex {
            get {
                return Values[1];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return PointerType;
        }

        public override string ToString () {
            return String.Format("({1}){0}[{2}]", Array, PointerType, ArrayIndex);
        }
    }

    public static class JSPointerExpressionUtil {
        public static int? GetNativeSizeOfElement (TypeReference elementType) {
            switch (elementType.FullName) {
                case "System.Byte":
                case "System.SByte":
                    return 1;

                case "System.Int16":
                case "System.UInt16":
                    return 2;

                case "System.Int32":
                case "System.UInt32":
                case "System.Single":
                    return 4;

                case "System.Int64":
                case "System.UInt64":
                case "System.Double":
                    return 8;
            }

            return null;
        }

        public static JSExpression UnwrapExpression (JSExpression e) {
            while (true) {
                var cte = e as JSChangeTypeExpression;
                var cast = e as JSCastExpression;
                var truncation = e as JSTruncateExpression;

                if (cte != null)
                    e = cte.Expression;
                else if (cast != null)
                    e = cast.Expression;
                else if (truncation != null)
                    e = truncation.Expression;
                else
                    break;
            }

            return e;
        }

        public static JSExpression OffsetFromBytesToElements (JSExpression offsetInBytes, TypeReference elementType) {
            if (offsetInBytes == null)
                return null;

            int depth;
            elementType = TypeUtil.FullyDereferenceType(elementType, out depth);

            offsetInBytes = UnwrapExpression(offsetInBytes);

            // pBytes + sizeof(T) -> pT[1]
            var sizeofE = offsetInBytes as JSSizeOfExpression;
            if (sizeofE != null) {
                if (TypeUtil.TypesAreEqual(elementType, sizeofE.Type.Type))
                    return JSLiteral.New(1);
            }

            var boe = offsetInBytes as JSBinaryOperatorExpression;
            if (boe == null)
                return null;

            var right = UnwrapExpression(boe.Right);

            var rightLiteral = right as JSIntegerLiteral;
            var rightSizeof = right as JSSizeOfExpression;

            if (boe.Operator == JSOperator.Multiply) {
                if (rightLiteral != null) {
                    // pBytes + (M * N) -> pT[N] if M == sizeof(T)

                    var elementSize = GetNativeSizeOfElement(elementType);
                    if (elementSize.HasValue && (rightLiteral.Value == elementSize.Value))
                        return boe.Left;
                } else if (rightSizeof != null) {
                    // pBytes + (sizeof(T) * N) -> pT[N]

                    if (TypeUtil.TypesAreEqual(elementType, rightSizeof.Type.Type))
                        return boe.Left;
                }
            }

            return null;
        }
    }

    public class JSWriteThroughPointerExpression : JSBinaryOperatorExpression {
        static JSWriteThroughPointerExpression () {
            SetValueNames(
                typeof(JSWriteThroughPointerExpression),
                "Left",
                "Right",
                "OffsetInBytes",
                "OffsetInElements"
            );
        }

        public JSWriteThroughPointerExpression (JSExpression pointer, JSExpression rhs, TypeReference actualType, JSExpression offsetInBytes = null)
            : base(
                JSOperator.Assignment, pointer, rhs, 
                actualType, offsetInBytes, 
                JSPointerExpressionUtil.OffsetFromBytesToElements(offsetInBytes, actualType)
            ) {
        }

        public JSExpression OffsetInBytes {
            get {
                return Values[2];
            }
        }

        public JSExpression OffsetInElements {
            get {
                return Values[3];
            }
        }

        public TypeReference ElementType {
            get {
                return ActualType;
            }
        }

        public override string ToString () {
            return String.Format("*({0} + {2}) = {1}", Left, Right, ((object)OffsetInBytes ?? "0"));
        }
    }

    public class JSReadThroughPointerExpression : JSExpression {
        public readonly TypeReference ElementType;

        public JSReadThroughPointerExpression (JSExpression pointer, TypeReference elementType, JSExpression offsetInBytes = null)
            : base(
                pointer, offsetInBytes, 
                JSPointerExpressionUtil.OffsetFromBytesToElements(offsetInBytes, elementType)
            ) {

            ElementType = elementType;
        }

        public JSExpression Pointer {
            get {
                return Values[0];
            }
        }

        public JSExpression OffsetInBytes {
            get {
                return Values[1];
            }
        }

        public JSExpression OffsetInElements {
            get {
                return Values[2];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            var pointerType = Pointer.GetActualType(typeSystem);
            return pointerType.GetElementType();
        }

        public override string ToString () {
            return String.Format("*({0} + {1})", Pointer, ((object)OffsetInBytes ?? "0"));
        }
    }

    public class JSPointerCastExpression : JSExpression {
        public JSPointerCastExpression (JSExpression pointer, TypeReference newType)
            : base (pointer, new JSType(newType)) {
        }

        public JSExpression Pointer {
            get {
                return Values[0];
            }
        }

        public JSType NewType {
            get {
                return (JSType)Values[1];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return NewType.Type;
        }

        public override string ToString () {
            return String.Format("({0}){1}", NewType, Pointer);
        }
    }

    public class JSPointerAddExpression : JSExpression {
        public readonly bool MutateInPlace;

        public JSPointerAddExpression (JSExpression pointer, JSExpression delta, bool mutateInPlace)
            : base(pointer, delta) {

            MutateInPlace = mutateInPlace;
        }

        public JSExpression Pointer {
            get {
                return Values[0];
            }
        }

        public JSExpression Delta {
            get {
                return Values[1];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Pointer.GetActualType(typeSystem);
        }

        public override string ToString () {
            return String.Format("({0} + {1})", Pointer, Delta);
        }
    }

    // Subtracts one pointer from another and produces the number of element(s) separating them.
    public class JSPointerDeltaExpression : JSBinaryOperatorExpression {
        public JSPointerDeltaExpression (JSExpression lhs, JSExpression rhs, TypeReference actualType)
            : base(JSOperator.Subtract, lhs, rhs, actualType) {
        }
    }

    public class JSPointerComparisonExpression : JSBinaryOperatorExpression {
        public JSPointerComparisonExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeReference actualType)
            : base(op, lhs, rhs, actualType) {
        }
    }

    public class JSOverflowCheckExpression : JSExpression {
        public static JSExpression New (JSExpression inner, TypeSystem typeSystem) {
            var ioce = inner as JSOverflowCheckExpression;
            if (ioce != null)
                return ioce;

            var innerType = inner.GetActualType(typeSystem);

            if (TypeUtil.IsPointer(innerType))
                return inner;

            return new JSOverflowCheckExpression(inner, new JSType(innerType));
        }

        protected JSOverflowCheckExpression (JSExpression expression, JSType type)
            : base (expression, type) {
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }

        public JSType Type {
            get {
                return (JSType)Values[1];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Expression.GetActualType(typeSystem);
        }
    }

    public class JSSizeOfExpression : JSExpression {
        public JSSizeOfExpression (JSType type) 
            : base (type) {

            if (type == null)
                throw new ArgumentNullException("type");
        }

        public JSType Type {
            get {
                return (JSType)Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Int32;
        }

        public override string ToString () {
            return String.Format("sizeof({0})", Type.Type.FullName);
        }
    }

    public class JSNewPackedArrayExpression : JSNewArrayExpression {
        public readonly TypeReference ActualType;

        public JSNewPackedArrayExpression (TypeReference actualType, TypeReference elementType, JSExpression sizeOrArrayInitializer)
            : base(elementType, sizeOrArrayInitializer) {

            ActualType = actualType;
        }

        public JSNewPackedArrayExpression (TypeReference actualType, TypeReference elementType, JSExpression[] dimensions, JSExpression initializer = null)
            : base(elementType, dimensions, initializer) {

            ActualType = actualType;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return ActualType;
        }
    }

    public class JSLocalCachedSignatureExpression : JSExpression {
        public readonly TypeReference Type;
        public readonly MethodReference Reference;
        public readonly MethodSignature Signature;
        public readonly bool IsConstructor;

        public JSLocalCachedSignatureExpression (TypeReference type, MethodReference reference, MethodSignature signature, bool isConstructor) {
            Type = type;
            Reference = reference;
            Signature = signature;
            IsConstructor = isConstructor;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Type;
        }
    }

    public class JSCachedInterfaceMemberExpression : JSExpression {
        public readonly int Index;

        public JSCachedInterfaceMemberExpression (JSType type, JSIdentifier member, int index)
            : base(type, member) {
            Index = index;
        }

        public JSType Type {
            get {
                return (JSType)Values[0];
            }
        }

        public JSIdentifier Identifier {
            get {
                return (JSIdentifier)Values[1];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            // FIXME: Return constructed method type?
            return typeSystem.Object;
        }
    }

    public class JSLocalCachedInterfaceMemberExpression : JSExpression {
        public readonly TypeReference TypeOfExpression;
        public readonly TypeReference InterfaceType;
        public readonly string MemberName;

        public JSLocalCachedInterfaceMemberExpression (TypeReference typeOfExpression, TypeReference interfaceType, string memberName) {
            TypeOfExpression = typeOfExpression;
            InterfaceType = interfaceType;
            MemberName = memberName;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return TypeOfExpression;
        }
    }

    public class JSNewBoxedVariable : JSExpression {
        public readonly TypeReference ValueType;

        public JSNewBoxedVariable (JSExpression initialValue, TypeReference valueType)
            : base (initialValue) {
            ValueType = valueType;
        }

        public JSExpression InitialValue {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return new ByReferenceType(ValueType);
        }
    }
}
