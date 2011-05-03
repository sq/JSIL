using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Ast {
    public abstract class JSOperator {
        public readonly string Token;

        public JSOperator (string token) {
            Token = token;
        }

        public override bool Equals (object obj) {
            var op = obj as JSOperator;
            var str = obj as string;

            if (op != null) {
                return String.Equals(Token, op.Token) || 
                    base.Equals(obj);
            } else if (str != null) {
                return String.Equals(Token, str);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode () {
            return Token.GetHashCode();
        }

        public abstract override string ToString ();

        public static readonly JSBinaryOperator Assignment = "=";

        public static readonly JSBinaryOperator Equal = "===";
        public static readonly JSBinaryOperator NotEqual = "!==";
        public static readonly JSBinaryOperator LessThan = "<";
        public static readonly JSBinaryOperator GreaterThan = ">";
        public static readonly JSBinaryOperator LessThanOrEqual = "<=";
        public static readonly JSBinaryOperator GreaterThanOrEqual = ">=";

        public static readonly JSBinaryOperator Add = "+";
        public static readonly JSBinaryOperator Subtract = "-";
        public static readonly JSBinaryOperator Multiply = "*";
        public static readonly JSBinaryOperator Divide = "/";
        public static readonly JSBinaryOperator ShiftLeft = "<<";
        public static readonly JSBinaryOperator ShiftRight = ">>";

        public static readonly JSBinaryOperator BitwiseAnd = "&";
        public static readonly JSBinaryOperator BitwiseOr = "|";

        public static readonly JSBinaryOperator LogicalAnd = "&&";
        public static readonly JSBinaryOperator LogicalOr = "||";

        public static readonly JSUnaryOperator Increment = "++";
        public static readonly JSUnaryOperator Decrement = "--";
        public static readonly JSUnaryOperator LogicalNot = "!";
        public static readonly JSUnaryOperator Negation = "-";
    }

    public sealed class JSUnaryOperator : JSOperator {
        public JSUnaryOperator (string token)
            : base(token) {
        }

        public override string ToString () {
            return String.Format("UnaryOperator {0}", Token);
        }

        public static implicit operator JSUnaryOperator (string token) {
            return new JSUnaryOperator(token);
        }
    }

    public sealed class JSBinaryOperator : JSOperator {
        public JSBinaryOperator (string token)
            : base(token) {
        }

        public override string ToString () {
            return String.Format("BinaryOperator {0}", Token);
        }

        public static implicit operator JSBinaryOperator (string token) {
            return new JSBinaryOperator(token);
        }
    }
}
