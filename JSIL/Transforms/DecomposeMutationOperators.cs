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
    public class DecomposeMutationOperators : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly TypeInfoProvider TypeInfo;

        public DecomposeMutationOperators (TypeSystem typeSystem, TypeInfoProvider typeInfo) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
        }

        public static JSExpression MakeLhsForAssignment (JSExpression rhs) {
            var fa = rhs as JSFieldAccess;
            var pa = rhs as JSPropertyAccess;
            if ((fa != null) && (fa.IsWrite == false))
                return new JSFieldAccess(fa.ThisReference, fa.Field, true);
            else if ((pa != null) && (pa.IsWrite == false))
                return new JSPropertyAccess(
                    pa.ThisReference, pa.Property, true, pa.TypeQualified, pa.OriginalType, pa.OriginalMethod, pa.IsVirtualCall
                );

            return rhs;
        }

        private JSBinaryOperatorExpression MakeUnaryMutation (
            JSExpression expressionToMutate, JSBinaryOperator mutationOperator,
            TypeReference type
        ) {
            var newValue = new JSBinaryOperatorExpression(
                mutationOperator, expressionToMutate, JSLiteral.New(1),
                type
            );
            var assignment = new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                MakeLhsForAssignment(expressionToMutate), newValue, type
            );

            return assignment;
        }

        // Decomposing a compound assignment can leave us with a read expression on the left-hand side
        //  if the compound assignment was targeting a pointer or a reference.
        // We fix this by converting the mundane binary operator expression representing the assignment
        //  into a specialized one that represents a pointer write or reference write.
        private static JSBinaryOperatorExpression ConvertReadExpressionToWriteExpression (
            JSBinaryOperatorExpression boe, TypeSystem typeSystem
        ) {
            var rtpe = boe.Left as JSReadThroughPointerExpression;

            if (rtpe != null)
                return new JSWriteThroughPointerExpression(rtpe.Pointer, boe.Right, boe.ActualType, rtpe.OffsetInBytes);

            var rtre = boe.Left as JSReadThroughReferenceExpression;
            if (rtre != null)
                return new JSWriteThroughReferenceExpression(rtre.Variable, boe.Right);

            return boe;
        }

        public static JSExpression DeconstructMutationAssignment (
            JSNode parentNode, JSBinaryOperatorExpression boe, TypeSystem typeSystem, TypeReference intermediateType
        ) {
            var assignmentOperator = boe.Operator as JSAssignmentOperator;
            if (assignmentOperator == null)
                return null;

            JSBinaryOperator replacementOperator;
            if (!IntroduceEnumCasts.ReverseCompoundAssignments.TryGetValue(assignmentOperator, out replacementOperator))
                return null;

            var leftType = boe.Left.GetActualType(typeSystem);

            var newBoe = new JSBinaryOperatorExpression(
                JSOperator.Assignment, MakeLhsForAssignment(boe.Left),
                new JSBinaryOperatorExpression(
                    replacementOperator, boe.Left, boe.Right, intermediateType
                ),
                leftType
            );

            var result = ConvertReadExpressionToWriteExpression(newBoe, typeSystem);
            if (parentNode is JSExpressionStatement) {
                return result;
            } else {
                var comma = new JSCommaExpression(
                    newBoe, boe.Left
                );
                return comma;
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var resultType = boe.GetActualType(TypeSystem);
            var resultIsIntegral = TypeUtil.Is32BitIntegral(resultType);
            var resultIsPointer = TypeUtil.IsPointer(resultType);

            JSExpression replacement;

            if (
                (resultIsIntegral) &&
                ((replacement = DeconstructMutationAssignment(ParentNode, boe, TypeSystem, resultType)) != null)
            ) {
                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
                return;
            }

            VisitChildren(boe);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var type = uoe.Expression.GetActualType(TypeSystem);
            var isIntegral = TypeUtil.Is32BitIntegral(type);

            if (isIntegral && (uoe.Operator is JSUnaryMutationOperator)) {
                if (
                    (uoe.Operator == JSOperator.PreIncrement) ||
                    (uoe.Operator == JSOperator.PreDecrement)
                ) {
                    var assignment = MakeUnaryMutation(
                        uoe.Expression,
                        (uoe.Operator == JSOperator.PreDecrement)
                            ? JSOperator.Subtract
                            : JSOperator.Add,
                        type
                    );

                    ParentNode.ReplaceChild(uoe, assignment);
                    VisitReplacement(assignment);
                    return;

                } else if (
                    (uoe.Operator == JSOperator.PostIncrement) ||
                    (uoe.Operator == JSOperator.PostDecrement)
                ) {
                    // FIXME: Terrible hack
                    var tempVariable = TemporaryVariable.ForFunction(
                        Stack.Last() as JSFunctionExpression, type
                    );
                    var makeTempCopy = new JSBinaryOperatorExpression(
                        JSOperator.Assignment, tempVariable, uoe.Expression, type
                    );
                    var assignment = MakeUnaryMutation(
                        uoe.Expression,
                        (uoe.Operator == JSOperator.PostDecrement)
                            ? JSOperator.Subtract
                            : JSOperator.Add,
                        type
                    );

                    var comma = new JSCommaExpression(
                        makeTempCopy,
                        assignment,
                        tempVariable
                    );

                    ParentNode.ReplaceChild(uoe, comma);
                    VisitReplacement(comma);
                    return;

                } else {
                    throw new NotImplementedException("Unary mutation not supported: " + uoe);
                }
            }

            VisitChildren(uoe);
        }
    }
}
