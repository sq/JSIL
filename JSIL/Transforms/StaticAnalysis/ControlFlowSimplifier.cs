using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class ControlFlowSimplifier : JSAstVisitor {
        private class LabelGroupLabelData {
            public readonly List<string> ExitTargetLabels = new List<string>();

            public string DirectExitLabel;
            public string RecursiveExitLabel;
            public int TimesUsedAsRecursiveExitTarget;
            public int UntargettedExitCount;
        }

        private class LabelGroupData : Dictionary<string, LabelGroupLabelData> {
        }

        private static int TraceLevel = 0;

        private readonly Stack<JSBlockStatement> BlockStack = new Stack<JSBlockStatement>();
        private readonly List<int> AbsoluteJumpsSeenStack = new List<int>();
        private readonly Stack<JSSwitchCase> SwitchCaseStack = new Stack<JSSwitchCase>();
        private readonly Stack<LabelGroupData> LabelGroupStack = new Stack<LabelGroupData>();
        private readonly Stack<int> LoopIndexStack = new Stack<int>();

        private JSSwitchCase LastSwitchCase = null;

        public bool MadeChanges = false;

        public ControlFlowSimplifier () {
            AbsoluteJumpsSeenStack.Add(0);
        }

        public void VisitNode (JSSwitchStatement ss) {
            AbsoluteJumpsSeenStack.Add(0);

            VisitChildren(ss);

            AbsoluteJumpsSeenStack.RemoveAt(AbsoluteJumpsSeenStack.Count - 1);
        }

        public void VisitNode (JSSwitchCase sc) {
            SwitchCaseStack.Push(sc);
            AbsoluteJumpsSeenStack.Add(0);

            if (TraceLevel >= 2) {
                if (sc.Values != null)
                    Console.WriteLine("// Entering case {0}", sc.Values.FirstOrDefault());
                else
                    Console.WriteLine("// Entering case default");
            }

            VisitChildren(sc);

            if (TraceLevel >= 2)
                Console.WriteLine("// Exiting case");

            AbsoluteJumpsSeenStack.RemoveAt(AbsoluteJumpsSeenStack.Count - 1);
            SwitchCaseStack.Pop();
        }

        public void VisitNode (JSBlockStatement bs) {
            var lastSwitchCase = LastSwitchCase;
            var thisSwitchCase = ParentSwitchCase;
            LastSwitchCase = thisSwitchCase;

            var parentLabelGroup = ParentNode as JSLabelGroupStatement;
            var isControlFlow = bs.IsControlFlow || 
                (thisSwitchCase != lastSwitchCase) || 
                (parentLabelGroup != null);

            if (TraceLevel >= 2)
                Console.WriteLine("// Entering block {0}", bs.Label ?? bs.GetType().Name);

            if (isControlFlow) {
                if (TraceLevel >= 2)
                    Console.WriteLine("// Count reset");

                AbsoluteJumpsSeenStack.Add(0);
            }

            BlockStack.Push(bs);

            VisitChildren(bs);

            BlockStack.Pop();

            if (TraceLevel >= 2)
                Console.WriteLine("// Exiting block");

            if (isControlFlow)
                AbsoluteJumpsSeenStack.RemoveAt(AbsoluteJumpsSeenStack.Count - 1);
        }

        private JSSwitchCase ParentSwitchCase {
            get {
                return SwitchCaseStack.LastOrDefault();
            }
        }

        private int AbsoluteJumpsSeen {
            get {
                if (AbsoluteJumpsSeenStack.Count <= 0)
                    return 0;

                return AbsoluteJumpsSeenStack[AbsoluteJumpsSeenStack.Count - 1];
            }
            set {
                if (AbsoluteJumpsSeenStack.Count <= 0)
                    throw new InvalidOperationException("Stack empty");

                AbsoluteJumpsSeenStack[AbsoluteJumpsSeenStack.Count - 1] = value;
            }
        }

        protected void VisitControlFlowNode (JSNode node) {
            var stackSlice = Stack.Take(3).ToArray();
            var parentEs = stackSlice[1] as JSExpressionStatement;
            var parentBlock = stackSlice[2] as JSBlockStatement;

            if ((parentEs != null) && (parentBlock == BlockStack.Peek())) {
                AbsoluteJumpsSeen += 1;

                if (AbsoluteJumpsSeen > 1) {
                    if (TraceLevel >= 1)
                        Console.WriteLine("// Eliminating {0}", node);

                    var replacement = new JSNullExpression();
                    ParentNode.ReplaceChild(node, replacement);
                    MadeChanges = true;
                    return;
                } else {
                    if (TraceLevel >= 3)
                        Console.WriteLine("// Not eliminating {0}", node);
                }
            }

            VisitChildren(node);
        }

        private void RecordUntargettedExit () {
            if (LabelGroupStack.Count > 0) {
                var enclosingLabelledStatement = Stack.OfType<JSStatement>().LastOrDefault((n) => n.Label != null);

                if (enclosingLabelledStatement != null) {
                    foreach (var lg in LabelGroupStack) {
                        LabelGroupLabelData labelData;

                        if (lg.TryGetValue(enclosingLabelledStatement.Label, out labelData))
                            labelData.UntargettedExitCount += 1;
                    }
                }
            }
        }

        public void VisitNode (JSReturnExpression re) {
            RecordUntargettedExit();

            VisitChildren(re);
        }

        public void VisitNode (JSContinueExpression ce) {
            if (ce.TargetLoop.HasValue && LoopIndexStack.Contains(ce.TargetLoop.Value))
                RecordUntargettedExit();

            VisitControlFlowNode(ce);
        }

        public void VisitNode (JSBreakExpression be) {
            if (be.TargetLoop.HasValue && LoopIndexStack.Contains(be.TargetLoop.Value))
                RecordUntargettedExit();

            VisitControlFlowNode(be);
        }

        public void VisitNode (JSGotoExpression ge) {
            if (LabelGroupStack.Count > 0) {
                var enclosingLabelledStatement = Stack.OfType<JSStatement>().LastOrDefault((n) => n.Label != null);

                if (enclosingLabelledStatement != null) {
                    foreach (var lg in LabelGroupStack) {
                        LabelGroupLabelData labelData;

                        if (lg.TryGetValue(enclosingLabelledStatement.Label, out labelData)) {
                            if (ge is JSExitLabelGroupExpression) {
                                labelData.UntargettedExitCount += 1;
                            } else {
                                labelData.ExitTargetLabels.Add(ge.TargetLabel);
                            }
                        }
                    }
                }
            }

            VisitControlFlowNode(ge);
        }

        private string ComputeRecursiveExitLabel (LabelGroupData data, string label) {
            var labelData = data[label];
            var recursiveExit = labelData.DirectExitLabel;
            if (recursiveExit == null)
                return null;

            while (recursiveExit != null) {
                LabelGroupLabelData targetLabelData;

                if (!data.TryGetValue(recursiveExit, out targetLabelData)) {
                    // The label is part of another label group.
                    return null;
                }

                if (targetLabelData.DirectExitLabel == null) {
                    if (
                        (targetLabelData.ExitTargetLabels.Count == 0) /* && 
                        FIXME: Is this right?
                        (targetLabelData.UntargettedExitCount <= 1) */
                    )
                        return recursiveExit;
                    else
                        return null;
                } else {
                    recursiveExit = targetLabelData.DirectExitLabel;
                }

                // Cycle detected
                if (recursiveExit == label)
                    return null;
            }

            return null;
        }

        private void ExtractExitLabel (JSLabelGroupStatement lgs) {
            var exitLabel = lgs.ExitLabel;
            var originalLabelName = exitLabel.Label;

            if (exitLabel.AllChildrenRecursive.OfType<JSGotoExpression>().Any()) {
                if (TraceLevel >= 1)
                    Console.WriteLine("// Cannot extract exit label '{0}' from label group because it contains a goto or exit", originalLabelName);

                return;
            }

            // The label before this label may have fallen through, so we need to append an ExitLabelGroup
            var previousLabel = lgs.BeforeExitLabel;

            if (previousLabel != null) {
                var exitStatement = new JSExpressionStatement(new JSExitLabelGroupExpression(lgs));

                var previousBlock = previousLabel as JSBlockStatement;
                if (previousBlock != null) {
                    previousBlock.Statements.Add(exitStatement);
                } else {
                    var replacement = new JSBlockStatement(
                        previousLabel, exitStatement
                    );

                    replacement.Label = previousLabel.Label;
                    replacement.IsControlFlow = true;
                    previousLabel.Label = null;

                    lgs.ReplaceChild(previousLabel, replacement);
                }
            }

            lgs.Labels.Remove(originalLabelName);
            exitLabel.Label = null;
            exitLabel.IsControlFlow = false;

            {
                var replacement = new JSBlockStatement(
                    lgs,
                    exitLabel
                );

                // Extract the exit label so it directly follows the label group
                ParentNode.ReplaceChild(lgs, replacement);

                // Find and convert all the gotos so that they instead break out of the label group
                var gotos = DeoptimizeSwitchStatements.FindGotos(lgs, originalLabelName);
                foreach (var g in gotos)
                    lgs.ReplaceChildRecursive(g, new JSExitLabelGroupExpression(lgs));
            }

            if (TraceLevel >= 1)
                Console.WriteLine("// Extracted exit label '{0}' from label group", originalLabelName);
            MadeChanges = true;
        }

        public void VisitNode (JSLabelGroupStatement lgs) {
            var data = new LabelGroupData();
            LabelGroupStack.Push(data);

            foreach (var key in lgs.Labels.Keys)
                data.Add(key.Value, new LabelGroupLabelData());

            VisitChildren(lgs);

            // Scan all the labels to determine their direct exit label, if any
            foreach (var kvp in data) {
                var targetLabels = kvp.Value.ExitTargetLabels.Distinct().ToArray();

                if (
                    (targetLabels.Length == 1) && 
                    (kvp.Value.UntargettedExitCount == 0)
                ) {
                    kvp.Value.DirectExitLabel = targetLabels[0];
                } else {
                    kvp.Value.DirectExitLabel = null;
                }
            }
            
            // Scan all the labels again to determine their recursive exit label
            foreach (var kvp in data) {
                var rel = kvp.Value.RecursiveExitLabel = ComputeRecursiveExitLabel(data, kvp.Key);

                if (rel != null)
                    data[rel].TimesUsedAsRecursiveExitTarget += 1;
            }

            // If we have one label that is the recursive exit target for all other labels, we can turn it into the exit label
            var recursiveExitTargets = data.Where(
                (kvp) => kvp.Value.TimesUsedAsRecursiveExitTarget > 0
            ).ToArray();
            if (recursiveExitTargets.Length == 1) {
                var onlyRecursiveExitTarget = recursiveExitTargets[0].Key;
                var exitLabel = lgs.ExitLabel;
                var newExitLabel = lgs.Labels[onlyRecursiveExitTarget];
                var newExitLabelData = data[newExitLabel.Label];

                if (
                    (newExitLabelData.ExitTargetLabels.Count == 0) && 
                    (newExitLabelData.UntargettedExitCount == 0) &&
                    (newExitLabel != lgs.Labels.LastOrDefault().Value)
                ) {
                    if (TraceLevel >= 1)
                        Console.WriteLine("// Cannot mark label '{0}' as exit label because it falls through and is not the last label", onlyRecursiveExitTarget);
                } else if (exitLabel != null) {
                    if (exitLabel != newExitLabel) {
                        if (TraceLevel >= 1)
                            Console.WriteLine("// Cannot mark label '{0}' as exit label because this labelgroup already has one", onlyRecursiveExitTarget);
                    }
                } else {
                    if (TraceLevel >= 1)
                        Console.WriteLine("// Marking label '{0}' as exit label", onlyRecursiveExitTarget);

                    lgs.ExitLabel = newExitLabel;
                    MadeChanges = true;
                }
            }

            if ((lgs.ExitLabel != null) && (lgs.Labels.Count > 1))
                ExtractExitLabel(lgs);

            LabelGroupStack.Pop();
        }

        public void VisitNode (JSLoopStatement ls) {
            LoopIndexStack.Push(ls.Index.GetValueOrDefault(-1));

            VisitChildren(ls);

            LoopIndexStack.Pop();
        }

        public void VisitNode (JSWhileLoop wl) {
            // Extract the last non-block statement from the body of the while loop
            var lastChild = wl.Children.LastOrDefault();
            while (lastChild is JSBlockStatement) {
                var bs = lastChild as JSBlockStatement;
                if (bs.IsControlFlow)
                    break;
                else
                    lastChild = bs.Statements.LastOrDefault();
            }

            // Is it a continue expression?
            var lastES = lastChild as JSExpressionStatement;
            if (lastES != null) {
                var lastContinue = lastES.Expression as JSContinueExpression;
                if (
                    (lastContinue != null) &&
                    (lastContinue.TargetLoop == wl.Index)
                ) {
                    // Spurious continue, so murder it
                    wl.ReplaceChildRecursive(lastContinue, new JSNullExpression());

                    if (TraceLevel >= 1)
                        Console.WriteLine("// Pruning spurious continue expression {0}", lastContinue);
                    MadeChanges = true;
                }
            }

            LoopIndexStack.Push(wl.Index.GetValueOrDefault(-1));

            VisitChildren(wl);

            LoopIndexStack.Pop();
        }
    }
}
