using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    class OptimizePropertyMutationAssignments : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;

        public OptimizePropertyMutationAssignments (TypeSystem typeSystem, ITypeInfoSource typeInfo) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
        }

        public bool IsPropertyAccess (
            JSBinaryOperatorExpression boe, out JSPropertyAccess pa
        ) {
            pa = boe.Left as JSPropertyAccess;
            return pa != null;
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            JSPropertyAccess pa;
            JSAssignmentOperator assignmentOperator;
            JSBinaryOperator newOperator;

            if (
                IsPropertyAccess(boe, out pa) &&
                ((assignmentOperator = boe.Operator as JSAssignmentOperator) != null) &&
                IntroduceEnumCasts.ReverseCompoundAssignments.TryGetValue(assignmentOperator, out newOperator)
            ) {
                // FIXME: Terrible hack
                var type = pa.GetActualType(TypeSystem);
                var tempVariable = TemporaryVariable.ForFunction(
                    Stack.Last() as JSFunctionExpression, type
                );
                var replacement = new JSCommaExpression(
                    new JSBinaryOperatorExpression(
                        JSOperator.Assignment, tempVariable,
                        new JSBinaryOperatorExpression(
                            newOperator,
                            new JSPropertyAccess(
                                pa.ThisReference, pa.Property, false,
                                pa.TypeQualified, pa.OriginalMethod, pa.IsVirtualCall
                            ), 
                            boe.Right, boe.GetActualType(TypeSystem)
                        ), type
                    ),
                    new JSBinaryOperatorExpression(
                        JSOperator.Assignment, 
                        new JSPropertyAccess(
                            pa.ThisReference, pa.Property, true, 
                            pa.TypeQualified, pa.OriginalMethod, pa.IsVirtualCall
                        ), tempVariable, type
                    ),
                    tempVariable
                );

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(boe);
            }
        }
    }
}
