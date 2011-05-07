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

        public static readonly JSAssignmentOperator Assignment = "=";
        public static readonly JSAssignmentOperator AddAssignment = "+=";
        public static readonly JSAssignmentOperator SubtractAssignment = "-=";
        public static readonly JSAssignmentOperator MultiplyAssignment = "*=";
        public static readonly JSAssignmentOperator DivideAssignment = "/=";
        public static readonly JSAssignmentOperator RemainderAssignment = "%=";
        public static readonly JSAssignmentOperator ShiftLeftAssignment = "<<=";
        public static readonly JSAssignmentOperator ShiftRightAssignment = ">>=";
        public static readonly JSAssignmentOperator ShiftRightUnsignedAssignment = ">>>=";
        public static readonly JSAssignmentOperator BitwiseAndAssignment = "&=";
        public static readonly JSAssignmentOperator BitwiseOrAssignment = "|=";
        public static readonly JSAssignmentOperator BitwiseXorAssignment = "^=";

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
        public static readonly JSBinaryOperator Remainder = "%";
        public static readonly JSBinaryOperator ShiftLeft = "<<";
        public static readonly JSBinaryOperator ShiftRight = ">>";
        public static readonly JSBinaryOperator ShiftRightUnsigned = ">>>";

        public static readonly JSBinaryOperator BitwiseAnd = "&";
        public static readonly JSBinaryOperator BitwiseOr = "|";
        public static readonly JSBinaryOperator BitwiseXor = "^";

        public static readonly JSLogicalOperator LogicalAnd = "&&";
        public static readonly JSLogicalOperator LogicalOr = "||";

        public static readonly JSUnaryOperator PreIncrement = new JSUnaryOperator("++", false);
        public static readonly JSUnaryOperator PreDecrement = new JSUnaryOperator("--", false);
        public static readonly JSUnaryOperator PostIncrement = new JSUnaryOperator("++", true);
        public static readonly JSUnaryOperator PostDecrement = new JSUnaryOperator("--", true);
        public static readonly JSUnaryOperator BitwiseNot = new JSUnaryOperator("~", false);
        public static readonly JSUnaryOperator LogicalNot = new JSUnaryOperator("!", false);
        public static readonly JSUnaryOperator Negation = new JSUnaryOperator("-", false);
        public static readonly JSUnaryOperator IsTrue = new JSUnaryOperator("!!", false);
    }

    public class JSUnaryOperator : JSOperator {
        public readonly bool IsPostfix;

        public JSUnaryOperator (string token, bool isPostfix)
            : base(token) {

            IsPostfix = isPostfix;
        }

        public override string ToString () {
            return String.Format("{0}", Token);
        }
    }

    public class JSBinaryOperator : JSOperator {
        public JSBinaryOperator (string token)
            : base(token) {
        }

        public override string ToString () {
            return String.Format("{0}", Token);
        }

        public static implicit operator JSBinaryOperator (string token) {
            return new JSBinaryOperator(token);
        }
    }

    public class JSLogicalOperator : JSBinaryOperator {
        public JSLogicalOperator (string token)
            : base(token) {
        }

        public static implicit operator JSLogicalOperator (string token) {
            return new JSLogicalOperator(token);
        }
    }

    public class JSAssignmentOperator : JSBinaryOperator {
        public JSAssignmentOperator (string token)
            : base(token) {
        }

        public static implicit operator JSAssignmentOperator (string token) {
            return new JSAssignmentOperator(token);
        }
    }
}
