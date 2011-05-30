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

        public static readonly JSAssignmentOperator Assignment = new JSAssignmentOperator("=");
        public static readonly JSAssignmentOperator AddAssignment = new JSAssignmentOperator("+=");
        public static readonly JSAssignmentOperator SubtractAssignment = new JSAssignmentOperator("-=");
        public static readonly JSAssignmentOperator MultiplyAssignment = new JSAssignmentOperator("*=");
        public static readonly JSAssignmentOperator DivideAssignment = new JSAssignmentOperator("/=");
        public static readonly JSAssignmentOperator RemainderAssignment = new JSAssignmentOperator("%=");
        public static readonly JSAssignmentOperator ShiftLeftAssignment = new JSAssignmentOperator("<<=");
        public static readonly JSAssignmentOperator ShiftRightAssignment = new JSAssignmentOperator(">>=");
        public static readonly JSAssignmentOperator ShiftRightUnsignedAssignment = new JSAssignmentOperator(">>>=");
        public static readonly JSAssignmentOperator BitwiseAndAssignment = new JSAssignmentOperator("&=");
        public static readonly JSAssignmentOperator BitwiseOrAssignment = new JSAssignmentOperator("|=");
        public static readonly JSAssignmentOperator BitwiseXorAssignment = new JSAssignmentOperator("^=");

        public static readonly JSBinaryOperator Equal = new JSBinaryOperator("===");
        public static readonly JSBinaryOperator NotEqual = new JSBinaryOperator("!==");
        public static readonly JSBinaryOperator LessThan = new JSBinaryOperator("<");
        public static readonly JSBinaryOperator GreaterThan = new JSBinaryOperator(">");
        public static readonly JSBinaryOperator LessThanOrEqual = new JSBinaryOperator("<=");
        public static readonly JSBinaryOperator GreaterThanOrEqual = new JSBinaryOperator(">=");

        public static readonly JSBinaryOperator Add = new JSBinaryOperator("+");
        public static readonly JSBinaryOperator Subtract = new JSBinaryOperator("-");
        public static readonly JSBinaryOperator Multiply = new JSBinaryOperator("*");
        public static readonly JSBinaryOperator Divide = new JSBinaryOperator("/");
        public static readonly JSBinaryOperator Remainder = new JSBinaryOperator("%");
        public static readonly JSBinaryOperator ShiftLeft = new JSBinaryOperator("<<");
        public static readonly JSBinaryOperator ShiftRight = new JSBinaryOperator(">>");
        public static readonly JSBinaryOperator ShiftRightUnsigned = new JSBinaryOperator(">>>");

        public static readonly JSBinaryOperator BitwiseAnd = new JSBinaryOperator("&");
        public static readonly JSBinaryOperator BitwiseOr = new JSBinaryOperator("|");
        public static readonly JSBinaryOperator BitwiseXor = new JSBinaryOperator("^");

        public static readonly JSLogicalOperator LogicalAnd = new JSLogicalOperator("&&");
        public static readonly JSLogicalOperator LogicalOr = new JSLogicalOperator("||");

        public static readonly JSUnaryOperator PreIncrement = new JSUnaryMutationOperator("++", false);
        public static readonly JSUnaryOperator PreDecrement = new JSUnaryMutationOperator("--", false);
        public static readonly JSUnaryOperator PostIncrement = new JSUnaryMutationOperator("++", true);
        public static readonly JSUnaryOperator PostDecrement = new JSUnaryMutationOperator("--", true);
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

    public class JSUnaryMutationOperator : JSUnaryOperator {
        public JSUnaryMutationOperator (string token, bool isPostfix)
            : base(token, isPostfix) {
        }
    }

    public class JSBinaryOperator : JSOperator {
        public JSBinaryOperator (string token)
            : base(token) {
        }

        public override string ToString () {
            return String.Format("{0}", Token);
        }
    }

    public class JSLogicalOperator : JSBinaryOperator {
        public JSLogicalOperator (string token)
            : base(token) {
        }
    }

    public class JSAssignmentOperator : JSBinaryOperator {
        public JSAssignmentOperator (string token)
            : base(token) {
        }
    }
}
