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
    public class CollapseNulls : JSAstVisitor {
        protected void RemoveNullStatements (JSBlockStatement bs) {
            bs.Statements.RemoveAll(
                (s) => {
                    var es = s as JSExpressionStatement;
                    if ((es != null) && es.Expression.IsNull)
                        return true;

                    if (s.IsNull)
                        return true;

                    return false;
                }
            );
        }

        public void VisitNode (JSBlockStatement bs) {
            RemoveNullStatements(bs);

            if ((bs.Statements.Count == 0) && (bs.Label == null)) {
                var newNull = new JSNullStatement();
                ParentNode.ReplaceChild(bs, newNull);
                VisitReplacement(newNull);
                return;
            }

            VisitChildren(bs);

            // Some of the children may have replaced themselves with nulls
            RemoveNullStatements(bs);
        }

        public void VisitNode (JSContinueExpression cont) {
            var parentLoop = Stack.OfType<JSLoopStatement>().FirstOrDefault();
            if (
                (parentLoop != null) &&
                (cont.TargetLoop == parentLoop.Index)
            ) {
                if (
                    Stack.Skip(1).TakeWhile((s) => s != parentLoop)
                        .All((s) => {
                            var l = s.Children.LastOrDefault();
                            if (l != null)
                                return l.SelfAndChildrenRecursive.Contains(cont);
                            else
                                return false;
                        })
                ) {
                    var newNull = new JSNullExpression();
                    ParentNode.ReplaceChild(cont, newNull);
                    VisitReplacement(newNull);
                    return;
                }
            }

            VisitChildren(cont);
        }

        public void VisitNode (JSLabelGroupStatement lgs) {
            var nonNull = (from kvp in lgs.Labels where !kvp.Value.IsNull select kvp.Value).ToArray();

            if (nonNull.Length == 0) {
                var theNull = new JSNullStatement();
                ParentNode.ReplaceChild(lgs, theNull);
                VisitReplacement(theNull);
            } else if (
                (nonNull.Length == 1) && 
                !nonNull[0].AllChildrenRecursive.OfType<JSGotoExpression>().Any(
                    (g) => g.TargetLabel == nonNull[0].Label
                )
            ) {
                var onlyLabel = nonNull[0];
                var labelGroupExit = onlyLabel.AllChildrenRecursive.OfType<JSExitLabelGroupExpression>().FirstOrDefault();

                if (labelGroupExit != null) {
                    var enclosingFunction = Stack.OfType<JSFunctionExpression>().First();
                    var loops = enclosingFunction.AllChildrenRecursive.OfType<JSLoopStatement>();

                    int nextLoopIndex = 0;
                    if (loops.Any((l) => true))
                        nextLoopIndex = loops.Max((l) => l.Index.GetValueOrDefault(0)) + 1;

                    var replacement = new JSDoLoop(
                        JSLiteral.New(false),
                        onlyLabel
                    );
                    replacement.Index = nextLoopIndex;

                    var newBreak = new JSBreakExpression();
                    newBreak.TargetLoop = nextLoopIndex;

                    onlyLabel.ReplaceChildRecursive(labelGroupExit, newBreak);

                    ParentNode.ReplaceChild(lgs, replacement);
                    VisitReplacement(replacement);
                } else {
                    ParentNode.ReplaceChild(lgs, onlyLabel);
                    VisitReplacement(onlyLabel);
                }
            } else {
                VisitChildren(lgs);
            }
        }
    }
}
