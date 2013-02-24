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

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);
            var resultType = boe.GetActualType(TypeSystem);

            if (!resultType.IsPointer || !leftType.IsPointer) {
                VisitChildren(boe);
                return;
            }

            if (boe.Operator == JSOperator.Add) {
                var replacement = JSInvocationExpression.InvokeMethod(
                    new JSFakeMethod("add", leftType, new[] { rightType }, MethodTypes),
                    boe.Left, new[] { boe.Right }, true
                );
                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);

            } else {
                VisitChildren(boe);
                return;
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
