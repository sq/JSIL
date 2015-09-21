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
        public readonly IFunctionSource FunctionSource;

        public UnsafeCodeTransforms (
            Configuration configuration,
            TypeSystem typeSystem, 
            MethodTypeFactory methodTypes,
            IFunctionSource functionSource
        ) {
            Configuration = configuration;
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;
            FunctionSource = functionSource;
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
                // HACK: Is this right?
                if (wtpe.OffsetInBytes != null)
                    offset = new JSBinaryOperatorExpression(JSOperator.Add, wtpe.OffsetInBytes, offset, TypeSystem.Int32);

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

        public static bool ExtractOffsetFromPointerExpression (JSExpression pointer, TypeSystem typeSystem, out JSExpression newPointer, out JSExpression offset) {
            pointer = JSPointerExpressionUtil.UnwrapExpression(pointer);

            var addExpression = pointer as JSPointerAddExpression;
            if (addExpression != null) {
                newPointer = addExpression.Pointer;
                offset = addExpression.Delta;
                return true;
            }

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

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

            JSVariable leftVar;
            JSPointerAddExpression rightAdd;

            if (
                TypeUtil.IsPointer(leftType) && 
                TypeUtil.IsPointer(rightType) &&
                (boe.Operator is JSAssignmentOperator) &&
                ((leftVar = boe.Left as JSVariable) != null) &&
                ((rightAdd = boe.Right as JSPointerAddExpression) != null) &&
                rightAdd.Pointer.Equals(leftVar) &&
                !rightAdd.MutateInPlace
            ) {
                var replacement = new JSPointerAddExpression(
                    leftVar, rightAdd.Delta, true
                );

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
                return;
            } else {
                VisitChildren(boe);
            }
        }

        private bool UnpackUnaryMutation (
            JSUnaryOperatorExpression uoe,
            out JSUnaryMutationOperator op,
            out JSExpression target,
            out TypeReference type
        ) {
            type = uoe.Expression.GetActualType(TypeSystem);

            if (
                TypeUtil.IsPointer(type) &&
                (uoe.Operator is JSUnaryMutationOperator)
            ) {
                target = uoe.Expression;
                op = (JSUnaryMutationOperator)uoe.Operator;
                return true;
            }

            target = null;
            op = null;
            return false;
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            JSUnaryMutationOperator op;
            JSExpression target;
            TypeReference type;

            if (UnpackUnaryMutation(uoe, out op, out target, out type)) {
                var tempVar = TemporaryVariable.ForFunction(
                    Stack.Last() as JSFunctionExpression, type, FunctionSource
                );
                var store = new JSBinaryOperatorExpression(
                    JSOperator.Assignment, tempVar, target, type
                );

                var delta = (
                    (op == JSOperator.PostIncrement) ||
                    (op == JSOperator.PreIncrement)
                )
                    ? 1
                    : -1;

                JSExpression replacement;
                if (
                    (op == JSOperator.PostIncrement) ||
                    (op == JSOperator.PostDecrement)
                ) {
                    var mutated = new JSPointerAddExpression(target, JSLiteral.New(delta), true);
                    replacement = new JSCommaExpression(store, mutated, tempVar);
                } else {
                    replacement = new JSPointerAddExpression(target, JSLiteral.New(delta), true);
                }

                ParentNode.ReplaceChild(uoe, replacement);
                VisitReplacement(replacement);
                return;
            }

            VisitChildren(uoe);
        }
    }
}
