using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Internal;
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
        public readonly IList<JSVariable> Parameters;
        public readonly JSStatement Body;

        public JSFunctionExpression (JSIdentifier functionName, IList<JSVariable> parameters, JSStatement body)
            : this (parameters, body) {
            FunctionName = functionName;
        }

        public JSFunctionExpression (IList<JSVariable> parameters, JSStatement body) {
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

    // Technically, this should be a statement. But in ILAst, it's an expression...
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

    // Same as above.
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

        public readonly IList<JSExpression> Values;

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

        public virtual TypeReference ExpectedType {
            get {
                return null;
            }
        }
    }

    // Indicates that the contained expression is a constructed reference to a JS value.
    public class JSReferenceExpression : JSExpression {
        protected JSReferenceExpression (JSExpression referent)
            : base (referent) {

            if (referent is JSReferenceExpression)
                throw new InvalidOperationException("Nested references are not allowed");
        }

        public static bool TryDereference (JSExpression reference, out JSExpression referent) {
            var variable = reference as JSVariable;
            var refe = reference as JSReferenceExpression;

            if ((variable != null) && (variable.IsReference)) {
                referent = variable;
                return true;
            } else if (refe != null) {
                referent = refe.Referent;
                return true;
            }

            referent = null;
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

        public override TypeReference ExpectedType {
            get {
                return Referent.ExpectedType;
            }
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

        public override TypeReference ExpectedType {
            get {
                return Referent.ExpectedType;
            }
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

        public static JSIntegerLiteral New (long value) {
            return new JSIntegerLiteral(value);
        }

        public static JSIntegerLiteral New (ulong value) {
            return new JSIntegerLiteral((long)value);
        }

        public static JSNumberLiteral New (double value) {
            return new JSNumberLiteral(value);
        }

        public static JSNumberLiteral New (decimal value) {
            return new JSNumberLiteral((double)value);
        }

        public static JSNullLiteral Null () {
            return new JSNullLiteral();
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
    }

    public class JSNullLiteral : JSLiteralBase<object> {
        public JSNullLiteral ()
            : base(null) {
        }
    }

    public class JSBooleanLiteral : JSLiteralBase<bool> {
        public JSBooleanLiteral (bool value)
            : base(value) {
        }
    }

    public class JSStringLiteral : JSLiteralBase<string> {
        public JSStringLiteral (string value)
            : base(value) {
        }
    }

    public class JSIntegerLiteral : JSLiteralBase<long> {
        public JSIntegerLiteral (long value)
            : base(value) {
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
    }

    public class JSNumberLiteral : JSLiteralBase<double> {
        public JSNumberLiteral (double value)
            : base(value) {
        }
    }

    public class JSTypeNameLiteral : JSLiteralBase<TypeReference> {
        public JSTypeNameLiteral (TypeReference value)
            : base(value) {
        }
    }

    public class JSIdentifier : JSExpression {
        public readonly string Identifier;

        public JSIdentifier (string identifier) {
            Identifier = identifier;
        }

        public static implicit operator JSIdentifier (string identifier) {
            return new JSIdentifier(identifier);
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
    }

    public class JSMethod : JSIdentifier {
        public readonly MethodReference Method;

        public JSMethod (MethodReference method)
            : base(GetMethodName(method)) {
            Method = method;
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
        public readonly TypeReference Type;
        public readonly bool IsReference;

        public JSVariable (string name, TypeReference type)
            : base(name) {

            if (type is ByReferenceType) {
                type = type.GetElementType();
                IsReference = true;
            } else {
                IsReference = false;
            }

            Type = type;
        }

        public override TypeReference ExpectedType {
            get {
                return Type;
            }
        }

        public override string ToString () {
            if (IsReference)
                return String.Format("ref {0} {1}", Type, Identifier);
            else
                return String.Format("{0} {1}", Type, Identifier);
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
    }

    public class JSIndexerExpression : JSExpression {
        public JSIndexerExpression (JSExpression target, JSExpression index)
            : base (target, index) {
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

        public IList<JSExpression> Arguments {
            get {
                return Values.Skip(1);
            }
        }
    }

    public class JSArrayExpression : JSExpression {
        public JSArrayExpression (params JSExpression[] values)
            : base (values) {
        }
    }

    public abstract class JSOperatorExpression<TOperator> : JSExpression
        where TOperator : JSOperator {

        public readonly TOperator Operator;

        protected JSOperatorExpression (TOperator op, params JSExpression[] values)
            : base(values) {
            Operator = op;
        }
    }

    public class JSBinaryOperatorExpression : JSOperatorExpression<JSBinaryOperator> {
        public JSBinaryOperatorExpression (JSBinaryOperator op, JSExpression lhs, JSExpression rhs)
            : base(op, lhs, rhs) {
        }

        public JSExpression Left {
            get {
                return Values[0];
            }
        }

        public JSExpression Right {
            get {
                return Values[1];
            }
        }
    }

    public class JSUnaryOperatorExpression : JSOperatorExpression<JSUnaryOperator> {
        public readonly bool Postfix;

        public JSUnaryOperatorExpression (JSUnaryOperator op, JSExpression expression, bool postfix = false)
            : base(op, expression) {

            Postfix = postfix;
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }
    }
}
