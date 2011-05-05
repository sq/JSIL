using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL.Ast {
    public abstract class JSNode {
        /// <summary>
        /// Enumerates the children of this node.
        /// </summary>
        public virtual IEnumerable<JSNode> Children {
            get {
                yield break;
            }
        }

        public IEnumerable<JSNode> AllChildrenRecursive {
            get {
                foreach (var child in Children) {
                    yield return child;

                    foreach (var subchild in child.AllChildrenRecursive)
                        yield return subchild;
                }
            }
        }

        public virtual bool IsNull {
            get {
                return false;
            }
        }
    }

    public abstract class JSStatement : JSNode {
        public static readonly JSNullStatement Null = new JSNullStatement();
    }

    public sealed class JSNullStatement : JSStatement {
        public override bool IsNull {
            get {
                return true;
            }
        }

        public override string ToString () {
            return "<Null>";
        }
    }

    public class JSBlockStatement : JSStatement {
        public readonly List<JSStatement> Statements;

        public JSBlockStatement (params JSStatement[] statements) {
            Statements = new List<JSStatement>(statements);
        }

        public override IEnumerable<JSNode> Children {
            get {
                return Statements;
            }
        }
    }

    public class JSVariableDeclarationStatement : JSStatement {
        public readonly List<JSBinaryOperatorExpression> Declarations = new List<JSBinaryOperatorExpression>();

        public JSVariableDeclarationStatement (params JSBinaryOperatorExpression[] declarations) {
            Declarations.AddRange(declarations);
        }

        public override IEnumerable<JSNode> Children {
            get {
                return Declarations;
            }
        }
    }

    public class JSExpressionStatement : JSStatement {
        public readonly JSExpression Expression;

        public JSExpressionStatement (JSExpression expression) {
            Expression = expression;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Expression;
            }
        }
    }

    public class JSFunctionExpression : JSExpression {
        public readonly JSIdentifier FunctionName;
        // This has to be JSVariable[], because 'this' is of type (JSVariableReference<JSThisParameter>) for structs
        public readonly JSVariable[] Parameters;
        public readonly JSBlockStatement Body;

        public JSFunctionExpression (JSIdentifier functionName, JSVariable[] parameters, JSBlockStatement body)
            : this (parameters, body) {
            FunctionName = functionName;
        }

        public JSFunctionExpression (JSVariable[] parameters, JSBlockStatement body) {
            FunctionName = null;
            Parameters = parameters;
            Body = body;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return FunctionName;

                foreach (var parameter in Parameters)
                    yield return parameter;

                yield return Body;
            }
        }
    }

    // Technically, the following expressions should be statements. But in ILAst, they're expressions...
    public class JSReturnExpression : JSExpression {
        public readonly JSExpression Value;

        public JSReturnExpression (JSExpression value = null) {
            Value = value;
        }

        public override IEnumerable<JSNode> Children {
            get {
                if (Value != null)
                    yield return Value;
            }
        }
    }

    public class JSThrowExpression : JSExpression {
        public readonly JSExpression Exception;

        public JSThrowExpression (JSExpression exception = null) {
            Exception = exception;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Exception;
            }
        }
    }

    public class JSBreakExpression : JSExpression {
    }

    public class JSIfStatement : JSStatement {
        public readonly JSExpression Condition;
        public readonly JSStatement TrueClause;
        public JSStatement FalseClause;

        public JSIfStatement (JSExpression condition, JSStatement trueClause, JSStatement falseClause = null) {
            Condition = condition;
            TrueClause = trueClause;
            FalseClause = falseClause;
        }

        public static JSIfStatement New (params KeyValuePair<JSExpression, JSStatement>[] conditions) {
            if ((conditions == null) || (conditions.Length == 0))
                throw new ArgumentException("conditions");

            JSIfStatement result = new JSIfStatement(
                conditions[0].Key, conditions[0].Value
            );
            JSIfStatement next = null, current = result;

            for (int i = 1; i < conditions.Length; i++) {
                var cond = conditions[i].Key;

                if (cond != null) {
                    next = new JSIfStatement(cond, conditions[i].Value);
                    current.FalseClause = next;
                    current = next;
                } else
                    current.FalseClause = conditions[i].Value;
            }

            return result;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Condition;
                yield return TrueClause;

                if (FalseClause != null)
                    yield return FalseClause;
            }
        }
    }

    public class JSWhileLoop : JSStatement {
        public readonly JSExpression Condition;
        public readonly JSStatement Body;

        public JSWhileLoop (JSExpression condition, JSStatement body) {
            Condition = condition;
            Body = body;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Condition;
                yield return Body;
            }
        }
    }

    public class JSTryCatchBlock : JSStatement {
        public readonly JSStatement Body;
        public JSVariable CatchVariable;
        public JSStatement Catch;
        public JSStatement Finally;

        public JSTryCatchBlock (JSStatement body, JSVariable catchVariable = null, JSStatement @catch = null, JSStatement @finally = null) {
            Body = body;
            CatchVariable = catchVariable;
            Catch = @catch;
            Finally = @finally;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Body;

                if (CatchVariable != null)
                    yield return CatchVariable;
                if (Catch != null)
                    yield return Catch;
                if (Finally != null)
                    yield return Finally;
            }
        }
    }

    public abstract class JSExpression : JSNode {
        public static readonly JSNullExpression Null = new JSNullExpression();

        protected readonly IList<JSExpression> Values;

        protected JSExpression (params JSExpression[] values) {
            Values = values;
        }

        public override IEnumerable<JSNode> Children {
            get {
                return Values;
            }
        }

        public override string ToString () {
            return String.Format(
                "{0}[{1}]", GetType().Name,
                String.Join(", ", (from v in Values select v.ToString()).ToArray())
            );
        }

        public virtual TypeReference GetExpectedType (TypeSystem typeSystem) {
            throw new NoExpectedTypeException(this);
        }
    }

    // Indicates that the contained expression is a constructed reference to a JS value.
    public class JSReferenceExpression : JSExpression {
        protected JSReferenceExpression (JSExpression referent)
            : base (referent) {

            if (referent is JSReferenceExpression)
                throw new InvalidOperationException("Nested references are not allowed");
        }

        /// <summary>
        /// Converts a constructed reference into the expression it refers to, turning it back into a regular expression.
        /// </summary>
        public static bool TryDereference (JSExpression reference, out JSExpression referent) {
            var variable = reference as JSVariable;
            var refe = reference as JSReferenceExpression;

            if ((variable != null) && (variable.IsReference)) {
                referent = variable.Dereference();
                return true;
            } else if (refe == null) {
                referent = null;
                return false;
            }

            referent = refe.Referent;
            return true;
        }

        /// <summary>
        /// Converts a constructed reference into an actual reference to the expression it refers to, allowing it to be passed to functions.
        /// </summary>
        public static bool TryMaterialize (JSILIdentifier jsil, JSExpression reference, out JSExpression materialized) {
            var mref = reference as JSMemberReferenceExpression;

            if (mref != null) {
                var dot = (JSDotExpression)mref.Referent;
                materialized = jsil.NewMemberReference(
                    dot.Target, dot.Member.ToLiteral()
                );
                return true;
            }

            var variable = reference as JSVariable;
            var refe = reference as JSReferenceExpression;
            if (refe != null)
                variable = refe.Referent as JSVariable;

            if ((variable != null) && (variable.IsReference)) {
                materialized = variable.Dereference();
                return true;
            }

            if ((variable != null) && EmulateStructAssignment.IsStruct(variable.Type)) {
                materialized = variable;
                return true;
            }

            materialized = null;
            return false;
        }

        public static JSExpression New (JSExpression referent) {
            var variable = referent as JSVariable;

            if ((variable != null) && (variable.IsReference)) {
                return variable;
            } else {
                return new JSReferenceExpression(referent);
            }
        }

        public JSExpression Referent {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Referent.GetExpectedType(typeSystem);
        }

        public override string ToString () {
            return String.Format("&({0})", Referent);
        }
    }

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

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Referent.GetExpectedType(typeSystem);
        }

        public override string ToString () {
            return String.Format("<ref {0}>", Referent);
        }
    }

    public sealed class JSNullExpression : JSExpression {
        public override bool IsNull {
            get {
                return true;
            }
        }

        public override string ToString () {
            return "<Null>";
        }
    }

    public abstract class JSLiteral : JSExpression {
        public abstract object Literal {
            get;
        }

        public static JSTypeNameLiteral New (TypeReference value) {
            return new JSTypeNameLiteral(value);
        }

        public static JSStringLiteral New (string value) {
            return new JSStringLiteral(value);
        }

        public static JSBooleanLiteral New (bool value) {
            return new JSBooleanLiteral(value);
        }

        public static JSIntegerLiteral New (sbyte value) {
            return new JSIntegerLiteral(value, typeof(sbyte));
        }

        public static JSIntegerLiteral New (byte value) {
            return new JSIntegerLiteral(value, typeof(byte));
        }

        public static JSIntegerLiteral New (short value) {
            return new JSIntegerLiteral(value, typeof(short));
        }

        public static JSIntegerLiteral New (ushort value) {
            return new JSIntegerLiteral(value, typeof(ushort));
        }

        public static JSIntegerLiteral New (int value) {
            return new JSIntegerLiteral(value, typeof(int));
        }

        public static JSIntegerLiteral New (uint value) {
            return new JSIntegerLiteral(value, typeof(uint));
        }

        public static JSIntegerLiteral New (long value) {
            return new JSIntegerLiteral(value, typeof(long));
        }

        public static JSIntegerLiteral New (ulong value) {
            return new JSIntegerLiteral((long)value, typeof(ulong));
        }

        public static JSNumberLiteral New (float value) {
            return new JSNumberLiteral(value, typeof(float));
        }

        public static JSNumberLiteral New (double value) {
            return new JSNumberLiteral(value, typeof(double));
        }

        public static JSNumberLiteral New (decimal value) {
            return new JSNumberLiteral((double)value, typeof(decimal));
        }

        public static JSDefaultValueLiteral DefaultValue (TypeReference type) {
            return new JSDefaultValueLiteral(type);
        }

        public static JSNullLiteral Null (TypeReference type) {
            return new JSNullLiteral(type);
        }
    }

    public abstract class JSLiteralBase<T> : JSLiteral {
        public readonly T Value;

        protected JSLiteralBase (T value) {
            Value = value;
        }

        public override object Literal {
            get {
                return this.Value;
            }
        }

        public override string ToString () {
            return String.Format("<{0} {1}>", GetType().Name, Value);
        }
    }

    public class JSDefaultValueLiteral : JSLiteralBase<TypeReference> {
        public JSDefaultValueLiteral (TypeReference type)
            : base(type) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Value;
        }
    }

    public class JSNullLiteral : JSLiteralBase<object> {
        public readonly TypeReference Type;

        public JSNullLiteral (TypeReference type)
            : base(null) {

            Type = type;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            if (Type != null)
                return Type;
            else
                return typeSystem.Object;
        }
    }

    public class JSBooleanLiteral : JSLiteralBase<bool> {
        public JSBooleanLiteral (bool value)
            : base(value) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return typeSystem.Boolean;
        }
    }

    public class JSStringLiteral : JSLiteralBase<string> {
        public JSStringLiteral (string value)
            : base(value) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return typeSystem.String;
        }
    }

    public class JSIntegerLiteral : JSLiteralBase<long> {
        public readonly Type OriginalType;

        public JSIntegerLiteral (long value, Type originalType)
            : base(value) {

            OriginalType = originalType;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            if (OriginalType != null) {
                switch (OriginalType.FullName) {
                    case "System.Byte":
                        return typeSystem.Byte;
                    case "System.SByte":
                        return typeSystem.SByte;
                    case "System.UInt16":
                        return typeSystem.UInt16;
                    case "System.Int16":
                        return typeSystem.Int16;
                    case "System.UInt32":
                        return typeSystem.UInt32;
                    case "System.Int32":
                        return typeSystem.Int32;
                    case "System.UInt64":
                        return typeSystem.UInt64;
                    case "System.Int64":
                        return typeSystem.Int64;
                    default:
                        throw new NotImplementedException();
                }
            } else
                return typeSystem.Int64;
        }
    }

    public class JSEnumLiteral : JSLiteralBase<long> {
        public readonly TypeReference EnumType;
        public readonly string Name;

        public JSEnumLiteral (EnumMemberInfo member)
            : base(member.Value) {

            EnumType = member.DeclaringType;
            Name = member.Name;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return EnumType;
        }
    }

    public class JSNumberLiteral : JSLiteralBase<double> {
        public readonly Type OriginalType;

        public JSNumberLiteral (double value, Type originalType)
            : base(value) {

                OriginalType = originalType;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            if (OriginalType != null) {
                switch (OriginalType.FullName) {
                    case "System.Single":
                        return typeSystem.Single;
                    case "System.Double":
                        return typeSystem.Double;
                    case "System.Decimal":
                        return new TypeReference(typeSystem.Double.Namespace, "Decimal", typeSystem.Double.Module, typeSystem.Double.Scope, true);
                    default:
                        throw new NotImplementedException();
                }
            } else
                return typeSystem.Double;
        }
    }

    public class JSTypeNameLiteral : JSLiteralBase<TypeReference> {
        public JSTypeNameLiteral (TypeReference value)
            : base(value) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return typeSystem.String;
        }
    }

    public class JSIdentifier : JSExpression {
        public readonly string Identifier;
        public readonly TypeReference Type;

        public JSIdentifier (string identifier, TypeReference type = null) {
            Identifier = identifier;
            Type = type;
        }

        public override bool Equals (object obj) {
            var id = obj as JSIdentifier;
            var str = obj as string;
            if (id != null) {
                return String.Equals(Identifier, id.Identifier) ||
                    base.Equals(id);
            } else if (str != null) {
                return String.Equals(Identifier, str);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode () {
            return Identifier.GetHashCode();
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            if (Type != null)
                return Type;
            else
                return base.GetExpectedType(typeSystem);
        }

        public override string ToString () {
            return String.Format("<{0} '{1}'>", GetType().Name, Identifier);
        }

        public virtual JSLiteral ToLiteral () {
            return JSLiteral.New(Identifier);
        }
    }

    public class JSNamespace : JSIdentifier {
        public JSNamespace (string name)
            : base(name) {
        }
    }

    public class JSType : JSIdentifier {
        public readonly TypeReference Type;

        public JSType (TypeReference type)
            : base(type.FullName) {
            Type = type;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Type;
        }
        
        public override JSLiteral ToLiteral () {
            return JSLiteral.New(Type);
        }
    }

    public class JSField : JSIdentifier {
        public readonly FieldReference Field;

        public JSField (FieldReference field)
            : base(field.Name) {
            Field = field;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Field.FieldType;
        }
    }

    public class JSMethod : JSIdentifier {
        public readonly MethodReference Method;

        public JSMethod (MethodReference method)
            : base(GetMethodName(method)) {
            Method = method;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Method.ReturnType;
        }

        protected static string GetMethodName (MethodReference method) {
            var declType = method.DeclaringType.Resolve();

            if ((declType != null) && (declType.IsInterface))
                return String.Format("{0}.{1}", declType.Name, method.Name);
            else
                return method.Name;
        }
    }

    public class JSVariable : JSIdentifier {
        protected readonly TypeReference _Type;
        protected readonly bool _IsReference;

        public JSVariable (string name, TypeReference type)
            : base(name) {

            if (type is ByReferenceType) {
                type = type.GetElementType();
                _IsReference = true;
            } else {
                _IsReference = false;
            }

            _Type = type;
        }

        public virtual TypeReference Type {
            get {
                return _Type;
            }
        }

        public virtual bool IsReference {
            get {
                return _IsReference;
            }
        }

        public virtual bool IsParameter {
            get {
                return false;
            }
        }

        public virtual bool IsThis {
            get {
                return false;
            }
        }

        public static JSVariable New (ILVariable variable) {
            return new JSVariable(variable.Name, variable.Type);
        }

        public static JSVariable New (ParameterReference parameter) {
            return new JSParameter(parameter.Name, parameter.ParameterType);
        }

        public virtual JSVariable Reference () {
            return new JSVariableReference(this);
        }

        public virtual JSVariable Dereference () {
            if (IsReference)
                return new JSVariableDereference(this);
            else
                throw new InvalidOperationException();
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Type;
        }

        public override string ToString () {
            if (IsReference)
                return String.Format("<ref {0} {1}>", Type, Identifier);
            else if (IsThis)
                return String.Format("<this {0}>", Type);
            else if (IsParameter)
                return String.Format("<parameter {0} {1}>", Type, Identifier);
            else
                return String.Format("<var {0} {1}>", Type, Identifier);
        }
    }

    public class JSParameter : JSVariable {
        internal JSParameter (string name, TypeReference type)
            : base(name, type) {
        }

        public override bool IsParameter {
            get {
                return true;
            }
        }
    }

    public class JSThisParameter : JSParameter {
        public JSThisParameter (TypeReference type) : 
            base("this", type) {
        }

        public override bool IsThis {
            get {
                return true;
            }
        }

        public static JSVariable New (TypeReference type) {
            if (type.IsValueType)
                return new JSVariableReference(new JSThisParameter(type));
            else
                return new JSThisParameter(type);
        }
    }

    public class JSVariableDereference : JSVariable {
        public readonly JSVariable Referent;

        public JSVariableDereference (JSVariable referent)
            : base(referent.Identifier, null) {

            Referent = referent;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Referent.GetExpectedType(typeSystem).GetElementType();
        }

        public override TypeReference Type {
            get {
                return Referent.Type.GetElementType();
            }
        }

        public override bool IsReference {
            get {
                return (Referent.Type.GetElementType()) is ByReferenceType;
            }
        }

        public override bool IsParameter {
            get {
                return Referent.IsParameter;
            }
        }

        public override bool IsThis {
            get {
                return Referent.IsThis;
            }
        }

        public override JSVariable Reference () {
            return Referent;
        }
    }

    public class JSIndirectVariable : JSVariable {
        public readonly IDictionary<string, JSVariable> Variables;

        public JSIndirectVariable (IDictionary<string, JSVariable> variables, string identifier)
            : base(identifier, null) {

            Variables = variables;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Variables[Identifier].GetExpectedType(typeSystem);
        }

        public override TypeReference Type {
            get {
                return Variables[Identifier].Type;
            }
        }

        public override bool IsReference {
            get {
                return Variables[Identifier].IsReference;
            }
        }

        public override bool IsParameter {
            get {
                return Variables[Identifier].IsParameter;
            }
        }

        public override bool IsThis {
            get {
                return Variables[Identifier].IsThis;
            }
        }

        public override JSVariable Dereference () {
            return Variables[Identifier].Dereference();
        }

        public override JSVariable Reference () {
            return Variables[Identifier].Reference();
        }
    }

    public class JSVariableReference : JSVariable {
        public readonly JSVariable Referent;

        public JSVariableReference (JSVariable referent)
            : base(referent.Identifier, null) {

            Referent = referent;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return new ByReferenceType(Referent.GetExpectedType(typeSystem));
        }

        public override TypeReference Type {
            get {
                return new ByReferenceType(Referent.Type);
            }
        }

        public override bool IsReference {
            get {
                return true;
            }
        }

        public override bool IsParameter {
            get {
                return Referent.IsParameter;
            }
        }

        public override bool IsThis {
            get {
                return Referent.IsThis;
            }
        }

        public override JSVariable Dereference () {
            return Referent;
        }
    }

    public class JSDotExpression : JSExpression {
        public JSDotExpression (JSExpression target, JSIdentifier member)
            : base (target, member) {
        }

        public static JSDotExpression New (JSExpression leftMost, params JSIdentifier[] memberNames) {
            if ((memberNames == null) || (memberNames.Length == 0))
                throw new ArgumentException("memberNames");

            var result = new JSDotExpression(leftMost, memberNames[0]);
            for (var i = 1; i < memberNames.Length; i++)
                result = new JSDotExpression(result, memberNames[i]);

            return result;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Member.GetExpectedType(typeSystem);
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

        public override string ToString () {
            return String.Format("{0} . {1}", Target, Member);
        }
    }

    public class JSIndexerExpression : JSExpression {
        public JSIndexerExpression (JSExpression target, JSExpression index)
            : base (target, index) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            var targetType = Target.GetExpectedType(typeSystem);

            var at = targetType as ArrayType;
            if (at != null)
                return at.GetElementType();
            else
                return base.GetExpectedType(typeSystem);
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
    }

    public class JSNewExpression : JSExpression {
        public JSNewExpression (TypeReference type, params JSExpression[] arguments)
            : this (new JSType(type), arguments) {
        }

        public JSNewExpression (JSExpression type, params JSExpression[] arguments) : base(
            (new [] { type }).Concat(arguments).ToArray()
        ) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Values[0].GetExpectedType(typeSystem);
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

    public class JSInvocationExpression : JSExpression {
        public JSInvocationExpression (JSExpression target, params JSExpression[] arguments)
            : base ( 
                (new [] { target }).Concat(arguments).ToArray() 
            ) {
        }

        public JSExpression Target {
            get {
                return Values[0];
            }
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            return Target.GetExpectedType(typeSystem);
        }

        public IList<JSExpression> Arguments {
            get {
                return Values.Skip(1);
            }
        }

        public override string ToString () {
            return String.Format(
                "{0} ( {1} )", 
                Target, 
                String.Join(", ", (from a in Arguments select a.ToString()).ToArray())
            );
        }
    }

    public class JSArrayExpression : JSExpression {
        public readonly TypeReference ElementType;

        public JSArrayExpression (TypeReference elementType, params JSExpression[] values)
            : base (values) {

            ElementType = elementType;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            if (ElementType != null)
                return ElementType;
            else
                return base.GetExpectedType(typeSystem);
        }

        new public IEnumerable<JSExpression> Values {
            get {
                return base.Values;
            }
        }
    }

    public class JSPairExpression : JSExpression {
        public JSPairExpression (JSExpression key, JSExpression value)
            : base (key, value) {
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
    }

    public class JSObjectExpression : JSExpression {
        public JSObjectExpression (params JSPairExpression[] pairs) : base(
            pairs
        ) {
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
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
        public readonly TypeReference ExpectedType;

        protected JSOperatorExpression (TOperator op, TypeReference expectedType, params JSExpression[] values)
            : base(values) {

            Operator = op;
            ExpectedType = expectedType;
        }

        public override TypeReference GetExpectedType (TypeSystem typeSystem) {
            if (ExpectedType != null)
                return ExpectedType;

            TypeReference inferredType = null;
            foreach (var value in Values) {
                var valueType = value.GetExpectedType(typeSystem);

                if (inferredType == null)
                    inferredType = valueType;
                else if (valueType.FullName == inferredType.FullName)
                    continue;
                else
                    return base.GetExpectedType(typeSystem);
            }

            return inferredType;
        }
    }

    public class JSBinaryOperatorExpression : JSOperatorExpression<JSBinaryOperator> {
        /// <summary>
        /// Construct a binary operator expression with an explicit expected type.
        /// If the explicit expected type is null, expected type will be inferred to be the type of both sides if they share a type.
        /// </summary>
        public JSBinaryOperatorExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeReference expectedType) : base(
            op, expectedType, lhs, rhs
        ) {
        }

        /// <summary>
        /// Construct a binary operator expression with an implicit expected type.
        /// If the operator is the assignment operator, the expected type will be the expected type of the right hand side.
        /// Otherwise, the expected type will be inferred to be the type of both sides if they share a type.
        /// Otherwise, the expression will have no expected type.
        /// </summary>
        public JSBinaryOperatorExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeSystem typeSystem)
            : base(
            op, 
            InferExpectedType(op, lhs, rhs, typeSystem),
            lhs, rhs
        ) {
        }

        public static TypeReference InferExpectedType (JSBinaryOperator op, JSExpression lhs, JSExpression rhs, TypeSystem typeSystem) {
            if (op == JSOperator.Assignment)
                return rhs.GetExpectedType(typeSystem);
            else
                return null;
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
    }

    public class JSUnaryOperatorExpression : JSOperatorExpression<JSUnaryOperator> {
        public JSUnaryOperatorExpression (JSUnaryOperator op, JSExpression expression, TypeReference expectedType = null)
            : base(op, expectedType, expression) {
        }

        public bool IsPostfix {
            get {
                return ((JSUnaryOperator)Operator).IsPostfix;
            }
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }
    }

    public class NoExpectedTypeException : NotImplementedException {
        public NoExpectedTypeException (JSExpression node)
            : base(String.Format("Node of type {0} has no expected type: {1}", node.GetType().Name, node)) {
        }
    }
}
