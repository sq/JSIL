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

            var lastStatement = whileLoop.Statements.LastOrDefault();
            while ((lastStatement != null) && (lastStatement.GetType() == typeof(JSBlockStatement)))
                lastStatement = ((JSBlockStatement)lastStatement).Statements.LastOrDefault();

            var lastExpressionStatement = lastStatement as JSExpressionStatement;
            if (lastExpressionStatement != null) {
                var lastUoe = lastExpressionStatement.Expression as JSUnaryOperatorExpression;
                var lastBoe = lastExpressionStatement.Expression as JSBinaryOperatorExpression;

                if ((lastUoe != null) && (lastUoe.Operator is JSUnaryMutationOperator)) {
                    lastVariable = lastUoe.Expression as JSVariable;
                } else if ((lastBoe != null) && (lastBoe.Operator is JSAssignmentOperator)) {
                    lastVariable = lastBoe.Left as JSVariable;
                    if (
                        (lastVariable != null) &&
                        !lastBoe.Right.SelfAndChildrenRecursive.Any(
                            (n) => lastVariable.Equals(n)
                        )
                    ) {
                        lastVariable = null;
                    }
                }
            }

            var lastIfStatement = lastStatement as JSIfStatement;
            if (
                (lastIfStatement != null) && 
                whileLoop.Condition is JSBooleanLiteral &&
                ((JSBooleanLiteral)whileLoop.Condition).Value
            ) {
                var innerStatement = lastIfStatement.TrueClause;
                while (innerStatement is JSBlockStatement) {
                    var bs = (JSBlockStatement)innerStatement;
                    if (bs.Statements.Count != 1) {
                        innerStatement = null;
                        break;
                    }

                    innerStatement = bs.Statements[0];
                }

                var eStmt = innerStatement as JSExpressionStatement;

                if (eStmt != null) {
                    var breakExpr = eStmt.Expression as JSBreakExpression;
                    if ((breakExpr != null) && (breakExpr.TargetLabel == whileLoop.Label)) {
                        whileLoop.ReplaceChildRecursive(lastIfStatement, new JSNullStatement());

                        var doLoop = new JSDoLoop(
                            new JSUnaryOperatorExpression(JSOperator.LogicalNot, lastIfStatement.Condition),
                            whileLoop.Statements.ToArray()
                        );
                        doLoop.Label = whileLoop.Label;

                        ParentNode.ReplaceChild(whileLoop, doLoop);
                        VisitChildren(doLoop);
                        return;
                    }
                }
            }

            bool cantBeFor = false;

            if ((initVariable != null) && (lastVariable != null) &&
                !initVariable.Equals(lastVariable)
            ) {
                cantBeFor = true;
            } else if ((initVariable ?? lastVariable) == null) {
                cantBeFor = true;
            } else if (!whileLoop.Condition.SelfAndChildrenRecursive.Any(
                    (n) => (initVariable ?? lastVariable).Equals(n)
            )) {
                cantBeFor = true;
            }

            if (!cantBeFor) {
                JSStatement initializer = null, increment = null;

                if (initVariable != null) {
                    initializer = PreviousSibling as JSStatement;

                    ParentNode.ReplaceChild(PreviousSibling, new JSNullStatement());
                }

                if (lastVariable != null) {
                    increment = lastExpressionStatement;

                    whileLoop.ReplaceChildRecursive(lastExpressionStatement, new JSNullStatement());
                }

                var forLoop = new JSForLoop(
                    initializer, whileLoop.Condition, increment,
                    whileLoop.Statements.ToArray()
                );
                forLoop.Label = whileLoop.Label;

                ParentNode.ReplaceChild(whileLoop, forLoop);
                VisitChildren(forLoop);
            } else {
                VisitChildren(whileLoop);
            }
        }
    }
}
