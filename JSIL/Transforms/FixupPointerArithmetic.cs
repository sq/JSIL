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
    public class FixupPointerArithmetic : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;

        public FixupPointerArithmetic (TypeSystem typeSystem, MethodTypeFactory methodTypes) {
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;
        }

        public void VisitNode (JSReadThroughPointerExpression rtpe) {
            JSExpression newPointer, offset;

            if (ExtractOffsetFromPointerExpression(rtpe.Pointer, TypeSystem, out newPointer, out offset)) {
                var replacement = new JSReadThroughPointerExpression(newPointer, offset);
                ParentNode.ReplaceChild(rtpe, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(rtpe);
            }
        }

        public void VisitNode (JSWriteThroughPointerExpression wtpe) {
            JSExpression newPointer, offset;

            if (ExtractOffsetFromPointerExpression(wtpe.Left, TypeSystem, out newPointer, out offset)) {
                var replacement = new JSWriteThroughPointerExpression(newPointer, wtpe.Right, wtpe.ActualType, offset);
                ParentNode.ReplaceChild(wtpe, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(wtpe);
            }
        }

        public void VisitNode (JSPointerAddExpression pae) {
            VisitChildren(pae);
        }

        public void VisitNode (JSPointerDeltaExpression pde) {
            VisitChildren(pde);
        }

        public void VisitNode (JSPointerComparisonExpression pce) {
            VisitChildren(pce);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

            // We can end up with a pointer literal in an arithmetic expression.
            // In this case we want to switch it back to a normal integer literal so that the math operations work.
            var leftPointer = boe.Left as JSPointerLiteral;
            var rightPointer = boe.Right as JSPointerLiteral;
            if (!(boe.Operator is JSAssignmentOperator)) {
                if (leftPointer != null)
                    boe.ReplaceChild(boe.Left, JSIntegerLiteral.New(leftPointer.Value));
                if (rightPointer != null)
                    boe.ReplaceChild(boe.Right, JSIntegerLiteral.New(rightPointer.Value));
            }

            JSExpression replacement = null;
            if (leftType.IsPointer && TypeUtil.IsIntegral(rightType)) {
                if (
                    (boe.Operator == JSOperator.Add) ||
                    (boe.Operator == JSOperator.AddAssignment)
                ) {
                    replacement = new JSPointerAddExpression(
                        boe.Left, boe.Right,
                        boe.Operator == JSOperator.AddAssignment
                    );
                } else if (
                    (boe.Operator == JSOperator.Subtract) ||
                    (boe.Operator == JSOperator.SubtractAssignment)
                ) {
                    // FIXME: Int32 is probably wrong
                    replacement = new JSPointerAddExpression(
                        boe.Left,
                        new JSUnaryOperatorExpression(JSOperator.Negation, boe.Right, TypeSystem.Int32),
                        boe.Operator == JSOperator.SubtractAssignment
                    );
                }
            } else if (leftType.IsPointer && rightType.IsPointer) {
                if (boe.Operator == JSOperator.Subtract) {
                    // FIXME: Int32 is probably wrong
                    replacement = new JSPointerDeltaExpression(
                        boe.Left, boe.Right, TypeSystem.Int32
                    );
                } else if (boe.Operator is JSComparisonOperator) {
                    replacement = new JSPointerComparisonExpression(boe.Operator, boe.Left, boe.Right, boe.ActualType);
                }
            }

            if (replacement != null) {
                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(boe);
            }
        }

        public static bool ExtractOffsetFromPointerExpression (JSExpression pointer, TypeSystem typeSystem, out JSExpression newPointer, out JSExpression offset) {
            offset = null;
            newPointer = pointer;

            var boe = pointer as JSBinaryOperatorExpression;
            if (boe == null)
                return false;

            var leftType = boe.Left.GetActualType(typeSystem);
            var rightType = boe.Right.GetActualType(typeSystem);
            var resultType = boe.GetActualType(typeSystem);

            if (!resultType.IsPointer)
                return false;

            if (!TypeUtil.IsIntegral(rightType))
                return false;

            if (boe.Operator != JSOperator.Add)
                return false;

            newPointer = boe.Left;
            offset = boe.Right;
            return true;
        }
    }
}
