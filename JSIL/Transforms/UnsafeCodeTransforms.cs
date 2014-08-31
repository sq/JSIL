using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class UnsafeCodeTransforms : JSAstVisitor {
        public readonly Configuration Configuration;
        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;

        public UnsafeCodeTransforms (
            Configuration configuration,
            TypeSystem typeSystem, 
            MethodTypeFactory methodTypes
        ) {
            Configuration = configuration;
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;
        }

        public void VisitNode (JSResultReferenceExpression rre) {
            var invocation = (rre.Referent as JSInvocationExpression);

            // When passing a packed array element directly to a function, use a proxy instead.
            // This allows hoisting to operate when the call is inside a loop, and it reduces GC pressure (since we basically make the struct unpack occur on-demand).
            if (
                (ParentNode is JSInvocationExpression) &&
                (invocation != null) &&
                (invocation.JSMethod != null) &&
                invocation.JSMethod.Reference.FullName.Contains("JSIL.Runtime.IPackedArray") &&
                invocation.JSMethod.Reference.FullName.Contains("get_Item(") &&
                Configuration.CodeGenerator.AggressivelyUseElementProxies.GetValueOrDefault(false)
            ) {
                var elementType = invocation.GetActualType(TypeSystem);
                var replacement = new JSNewPackedArrayElementProxy(
                    invocation.ThisReference, invocation.Arguments.First(),
                    elementType
                );

                ParentNode.ReplaceChild(rre, replacement);
                VisitReplacement(replacement);
                return;
            }

            VisitChildren(rre);
        }

        public void VisitNode (JSReadThroughPointerExpression rtpe) {
            JSExpression newPointer, offset;

            if (ExtractOffsetFromPointerExpression(rtpe.Pointer, TypeSystem, out newPointer, out offset)) {
                var replacement = new JSReadThroughPointerExpression(newPointer, rtpe.ElementType, offset);
                ParentNode.ReplaceChild(rtpe, replacement);
                VisitReplacement(replacement);
            } else if (JSPointerExpressionUtil.UnwrapExpression(rtpe.Pointer) is JSBinaryOperatorExpression) {
                var replacement = new JSUntranslatableExpression("Read through confusing pointer " + rtpe.Pointer);
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
            } else if (JSPointerExpressionUtil.UnwrapExpression(wtpe.Pointer) is JSBinaryOperatorExpression) {
                var replacement = new JSUntranslatableExpression("Write through confusing pointer " + wtpe.Pointer);
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
                    boe.ReplaceChild(boe.Left, JSLiteral.New(leftPointer.Value));
                if (rightPointer != null)
                    boe.ReplaceChild(boe.Right, JSLiteral.New(rightPointer.Value));
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
            pointer = JSPointerExpressionUtil.UnwrapExpression(pointer);

            offset = null;
            newPointer = pointer;

            var boe = pointer as JSBinaryOperatorExpression;
            if (boe == null)
                return false;

            if (boe.Right.IsNull)
                return false;

            var right = JSPointerExpressionUtil.UnwrapExpression(boe.Right);

            var rightType = right.GetActualType(typeSystem);
            var resultType = boe.GetActualType(typeSystem);

            if (!resultType.IsPointer)
                return false;

            if (
                !TypeUtil.IsIntegral(rightType) &&
                // Adding pointers together shouldn't be valid but ILSpy generates it. Pfft.
                !TypeUtil.IsPointer(rightType)
            )
                return false;

            if (boe.Operator == JSOperator.Subtract) {
                newPointer = boe.Left;
                offset = new JSUnaryOperatorExpression(JSOperator.Negation, right, rightType);
                return true;
            } else if (boe.Operator == JSOperator.Add) {
                newPointer = boe.Left;
                offset = right;
                return true;
            } else {
                return false;
            }
        }
    }
}
