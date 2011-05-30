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
    public class SimplifyLoops : JSAstVisitor {
        public readonly TypeSystem TypeSystem;

        public SimplifyLoops (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSWhileLoop whileLoop) {
            JSVariable initVariable = null, lastVariable = null;
            JSBinaryOperator initOperator = null;
            JSExpression initValue = null;

            var prevEStmt = PreviousSibling as JSExpressionStatement;
            var prevVDS = PreviousSibling as JSVariableDeclarationStatement;

            if (prevEStmt != null) {
                var boe = prevEStmt.Expression as JSBinaryOperatorExpression;
                if (
                    (boe != null) &&
                    (boe.Operator is JSAssignmentOperator) &&
                    (boe.Left is JSVariable)
                ) {
                    initVariable = (JSVariable)boe.Left;
                    initOperator = boe.Operator;
                    initValue = boe.Right;
                }
            } else if (prevVDS != null) {
                var decl = prevVDS.Declarations.FirstOrDefault(
                    (d) => !d.IsNull
                );
                if (decl != null) {
                    initVariable = (JSVariable)decl.Left;
                    initOperator = decl.Operator;
                    initValue = decl.Right;
                }
            }

            var lastInnerStatement = whileLoop.AllChildrenRecursive.OfType<JSExpressionStatement>().LastOrDefault();
            if (lastInnerStatement != null) {
                var lastUoe = lastInnerStatement.Expression as JSUnaryOperatorExpression;
                var lastBoe = lastInnerStatement.Expression as JSBinaryOperatorExpression;

                if ((lastUoe != null) && (lastUoe.Operator is JSUnaryMutationOperator)) {
                    lastVariable = lastUoe.Expression as JSVariable;
                } else if ((lastBoe != null) && (lastBoe.Operator is JSAssignmentOperator)) {
                    lastVariable = lastBoe.Left as JSVariable;
                    if (
                        (lastVariable != null) &&
                        !lastBoe.Right.AllChildrenRecursive.Any(
                            (n) => lastVariable.Equals(n)
                        )
                    ) {
                        lastVariable = null;
                    }
                }
            }

            if ((initVariable != null) && (lastVariable != null) &&
                !initVariable.Equals(lastVariable)
            ) {

                VisitChildren(whileLoop);
                return;
            }

            if ((initVariable ?? lastVariable) == null) {
                VisitChildren(whileLoop);
                return;
            }

            if (!whileLoop.Condition.AllChildrenRecursive.Any(
                    (n) => (initVariable ?? lastVariable).Equals(n)
                )
            ) {
                VisitChildren(whileLoop);
                return;
            }

            JSStatement initializer = null, increment = null;

            if (initVariable != null) {
                initializer = PreviousSibling as JSStatement;

                ParentNode.ReplaceChild(PreviousSibling, new JSNullStatement());
            }

            if (lastVariable != null) {
                increment = lastInnerStatement;

                whileLoop.ReplaceChildRecursive(lastInnerStatement, new JSNullStatement());
            }

            var forLoop = new JSForLoop(
                initializer, whileLoop.Condition, increment,
                whileLoop.Statements.ToArray()
            );
            forLoop.Label = whileLoop.Label;

            ParentNode.ReplaceChild(whileLoop, forLoop);
            VisitChildren(forLoop);
        }
    }
}
