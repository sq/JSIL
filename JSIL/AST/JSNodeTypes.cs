using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Internal;

namespace JSIL.AST {
    public abstract class JSNode {
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

    public abstract class JSExpression : JSNode {
        public readonly IList<JSExpression> Values;

        protected JSExpression (params JSExpression[] values) {
            Values = values;
        }

        protected JSExpression (IList<JSExpression> values) {
            Values = values;
        }

        public override IEnumerable<JSNode> Children {
            get {
                return Values;
            }
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
            return Identifier;
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

        public override string ToString () {
            return String.Format("{0}.{1}", Target, Member);
        }
    }

    public class JSInvocationExpression : JSExpression {
        public JSInvocationExpression (JSExpression target, IList<JSExpression> arguments)
            : base ( 
                (new [] { target }).Concat(arguments).ToList() 
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
                return Values[0];
            }
        }
    }

    public class JSUnaryOperatorExpression : JSOperatorExpression<JSUnaryOperator> {
        public JSUnaryOperatorExpression (JSUnaryOperator op, JSExpression expression)
            : base(op, expression) {
        }

        public JSExpression Expression {
            get {
                return Values[0];
            }
        }
    }
}
