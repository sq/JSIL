using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class HandleBooleanAsInteger : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;

        public HandleBooleanAsInteger (TypeSystem typeSystem, JSSpecialIdentifiers js) {
            TypeSystem = typeSystem;
            JS = js;
        }

        protected JSInvocationExpression CastToInteger (JSExpression booleanExpression) {
            return JSInvocationExpression.InvokeMethod(
                JS.Number(TypeSystem.SByte), 
                booleanExpression, null, true
            );
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

            var leftIsBool = (leftType.FullName == "System.Boolean");
            var rightIsBool = (rightType.FullName == "System.Boolean");

            var leftIsNumeric = TypeUtil.IsNumericOrEnum(leftType);
            var rightIsNumeric = TypeUtil.IsNumericOrEnum(rightType);

            if (
                (leftIsBool != rightIsBool) && 
                (leftIsNumeric || rightIsNumeric) &&
                !(boe.Operator is JSAssignmentOperator)
            ) {
                JSBinaryOperatorExpression replacement;

                if (leftIsBool)
                    replacement = new JSBinaryOperatorExpression(
                        boe.Operator, CastToInteger(boe.Left), boe.Right, boe.ActualType
                    );
                else
                    replacement = new JSBinaryOperatorExpression(
                        boe.Operator, boe.Left, CastToInteger(boe.Right), boe.ActualType
                    );

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(boe);
            }
        }
    }
}
