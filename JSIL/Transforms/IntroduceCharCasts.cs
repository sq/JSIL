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
    public class IntroduceCharCasts : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;

        public IntroduceCharCasts (TypeSystem typeSystem, JSSpecialIdentifiers js) {
            TypeSystem = typeSystem;
            JS = js;
        }

        protected JSInvocationExpression CastToChar (JSExpression integerExpression) {
            return JSInvocationExpression.InvokeStatic(
                JS.fromCharCode, new[] { integerExpression }, true
            );
        }

        protected JSInvocationExpression CastToInteger (JSExpression charExpression) {
            return JSInvocationExpression.InvokeMethod(
                JS.charCodeAt, charExpression, new[] { JSLiteral.New(0) }, true
            );
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

            bool isArithmetic = !(boe.Operator is JSAssignmentOperator);

            if (!isArithmetic && boe.Operator != JSOperator.Assignment && leftType.FullName == "System.Char")
            {
                JSBinaryOperator newOperator;
                if (boe.Operator == JSOperator.AddAssignment)
                {
                    newOperator = JSOperator.Add;
                }
                else if (boe.Operator == JSOperator.BitwiseAndAssignment)
                {
                    newOperator = JSOperator.BitwiseAnd;
                }
                else if (boe.Operator == JSOperator.BitwiseOrAssignment)
                {
                    newOperator = JSOperator.BitwiseOr;
                }
                else if (boe.Operator == JSOperator.BitwiseXorAssignment)
                {
                    newOperator = JSOperator.BitwiseXor;
                }
                else if (boe.Operator == JSOperator.DivideAssignment)
                {
                    newOperator = JSOperator.Divide;
                }
                else if (boe.Operator == JSOperator.MultiplyAssignment)
                {
                    newOperator = JSOperator.Multiply;
                }
                else if (boe.Operator == JSOperator.RemainderAssignment)
                {
                    newOperator = JSOperator.Remainder;
                }
                else if (boe.Operator == JSOperator.ShiftLeftAssignment)
                {
                    newOperator = JSOperator.ShiftLeft;
                }
                else if (boe.Operator == JSOperator.ShiftRightAssignment)
                {
                    newOperator = JSOperator.ShiftRight;
                }
                else if (boe.Operator == JSOperator.ShiftRightUnsignedAssignment)
                {
                    newOperator = JSOperator.ShiftRightUnsigned;
                }
                else if (boe.Operator == JSOperator.SubtractAssignment)
                {
                    newOperator = JSOperator.Subtract;
                }
                else
                {
                    throw new InvalidOperationException("Unknown assigment operator");
                }

                var newBoe = new JSBinaryOperatorExpression(JSOperator.Assignment, boe.Left,
                    new JSBinaryOperatorExpression(newOperator, boe.Left, boe.Right, boe.ActualType), boe.ActualType);
                ParentNode.ReplaceChild(boe, newBoe);
                VisitReplacement(newBoe);
                return;
            }

            if (boe.Operator == JSOperator.Assignment && (leftType.FullName == "System.Char") && (rightType.FullName != "System.Char"))
            {
                boe.ReplaceChild(boe.Right, CastToChar(boe.Right));
            }
            if (boe.Operator == JSOperator.Assignment && (leftType.FullName != "System.Char") && (rightType.FullName == "System.Char"))
            {
                boe.ReplaceChild(boe.Right, CastToInteger(boe.Right));
            }

            if ((leftType.FullName == "System.Char") && isArithmetic)
                boe.ReplaceChild(boe.Left, CastToInteger(boe.Left));

            if ((rightType.FullName == "System.Char") && isArithmetic)
                boe.ReplaceChild(boe.Right, CastToInteger(boe.Right));

            var parentInvocation = ParentNode as JSInvocationExpression;
            JSDotExpressionBase parentInvocationDot = (parentInvocation != null) ? parentInvocation.Method as JSDotExpressionBase : null;

            if (
                isArithmetic && 
                (boe.GetActualType(TypeSystem).FullName == "System.Char") &&
                !(
                    (parentInvocation != null) && 
                    (parentInvocationDot != null) &&
                    (parentInvocationDot.Target is JSStringIdentifier) &&
                    (((JSStringIdentifier)parentInvocationDot.Target).Identifier == "String") &&
                    (parentInvocationDot.Member is JSFakeMethod) &&
                    (((JSFakeMethod)parentInvocationDot.Member).Name == "fromCharCode")
                )
            ) {
                var castBoe = CastToChar(boe);
                ParentNode.ReplaceChild(boe, castBoe);

                VisitReplacement(castBoe);
            } else {    
                VisitChildren(boe);
            }
        }
    }
}
