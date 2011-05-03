using System;
using System.Collections.Generic;
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
    }

    public abstract class JSStatement : JSNode {
    }

    public class JSBlockStatement : JSStatement {
        public readonly IList<JSStatement> Statements;

        public JSBlockStatement (params JSStatement[] statements) {
            Statements = statements;
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

    // Technically, this should be a statement. But in IL, it's an expression...
    public class JSReturnExpression : JSExpression {
        public readonly JSExpression Value;

        public JSReturnExpression (JSExpression value = null) {
            Value = value;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Value;
            }
        }
    }

    public class JSIfStatement : JSStatement {
        public readonly JSExpression Condition;
        public readonly JSStatement TrueClause;
        public readonly JSStatement FalseClause;

        public JSIfStatement (JSExpression condition, JSStatement trueClause, JSStatement falseClause = null) {
            Condition = condition;
            TrueClause = trueClause;
            FalseClause = falseClause;
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield return Condition;
                yield return TrueClause;
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

    public abstract class JSExpression : JSNode {
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

    public static class JSLiteral {
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

    public abstract class JSLiteral<T> : JSExpression {
        public readonly T Value;

        protected JSLiteral (T value) {
            Value = value;
        }
    }

    public class JSNullLiteral : JSLiteral<object> {
        public JSNullLiteral ()
            : base(null) {
        }
    }

    public class JSBooleanLiteral : JSLiteral<bool> {
        public JSBooleanLiteral (bool value)
            : base(value) {
        }
    }

    public class JSStringLiteral : JSLiteral<string> {
        public JSStringLiteral (string value)
            : base(value) {
        }
    }

    public class JSIntegerLiteral : JSLiteral<long> {
        public JSIntegerLiteral (long value)
            : base(value) {
        }
    }

    public class JSNumberLiteral : JSLiteral<double> {
        public JSNumberLiteral (double value)
            : base(value) {
        }
    }

    public class JSTypeNameLiteral : JSLiteral<TypeReference> {
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
    }

    public class JSVariable : JSIdentifier {
        public readonly TypeReference Type;

        public JSVariable (string name, TypeReference type)
            : base(name) {

            Type = type;
        }

        public override TypeReference ExpectedType {
            get {
                return Type;
            }
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
            : base (
                (new [] { new JSType(type) }).Concat(arguments).ToArray()
            ) {
        }

        public JSType Type {
            get {
                return (JSType)Values[0];
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
