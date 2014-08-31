using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Ast {
    public abstract class JSOperator {
        public readonly string Token;

        protected JSOperator (string token) {
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

        public static readonly JSComparisonOperator Equal = new JSComparisonOperator("===");
        public static readonly JSComparisonOperator NotEqual = new JSComparisonOperator("!==");
        public static readonly JSComparisonOperator LessThan = new JSComparisonOperator("<");
        public static readonly JSComparisonOperator GreaterThan = new JSComparisonOperator(">");
        public static readonly JSComparisonOperator LessThanOrEqual = new JSComparisonOperator("<=");
        public static readonly JSComparisonOperator GreaterThanOrEqual = new JSComparisonOperator(">=");

        public static readonly JSComparisonOperator EqualLoose = new JSComparisonOperator("==");
        public static readonly JSComparisonOperator NotEqualLoose = new JSComparisonOperator("!=");

        public static readonly JSArithmeticOperator Add = new JSArithmeticOperator("+");
        public static readonly JSArithmeticOperator Subtract = new JSArithmeticOperator("-");
        public static readonly JSArithmeticOperator Multiply = new JSArithmeticOperator("*");
        public static readonly JSArithmeticOperator Divide = new JSArithmeticOperator("/");
        public static readonly JSArithmeticOperator Remainder = new JSArithmeticOperator("%");

        public static readonly JSBitwiseOperator ShiftLeft = new JSBitwiseOperator("<<");
        public static readonly JSBitwiseOperator ShiftRight = new JSBitwiseOperator(">>");
        public static readonly JSBitwiseOperator ShiftRightUnsigned = new JSBitwiseOperator(">>>");

        public static readonly JSBitwiseOperator BitwiseAnd = new JSBitwiseOperator("&");
        public static readonly JSBitwiseOperator BitwiseOr = new JSBitwiseOperator("|");
        public static readonly JSBitwiseOperator BitwiseXor = new JSBitwiseOperator("^");

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

    public class JSArithmeticOperator : JSBinaryOperator {
        public JSArithmeticOperator (string token)
            : base(token) {
        }
    }

    public class JSBitwiseOperator : JSBinaryOperator {
        public JSBitwiseOperator (string token)
            : base(token) {
        }
    }

    public class JSComparisonOperator : JSBinaryOperator {
        public JSComparisonOperator (string token)
            : base(token) {
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
