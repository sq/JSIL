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
    public class LabelAnalyzer : JSAstVisitor {
        public class EnclosingBlockEntry {
            public JSStatement Block;
            public JSNode ParentNode;
            public int Depth;
        }

        public class LabelEntry {
            public JSStatement LabelledStatement;
            public EnclosingBlockEntry EnclosingBlock;
            public JSNode ParentNode;
            public JSLabelGroupStatement LabelGroup = null;

            public string Label {
                get {
                    return LabelledStatement.Label;
                }
            }

            public void EnsureLabelGroupExists (Dictionary<JSStatement, JSLabelGroupStatement> labelGroups) {
                if (LabelGroup != null)
                    return;

                if (!labelGroups.TryGetValue(EnclosingBlock.Block, out LabelGroup)) {
                    int index = labelGroups.Count;
                    var entryBlock = new JSBlockStatement {
                        Label = String.Format("$entry{0}", index)
                    };
                    var exitBlock = new JSBlockStatement {
                        Label = String.Format("$exit{0}", index)
                    };

                    labelGroups[EnclosingBlock.Block] = LabelGroup = new JSLabelGroupStatement(
                        index, entryBlock, exitBlock
                    );

                    var bs = EnclosingBlock.Block as JSBlockStatement;
                    if (bs != null) {
                        bool populatingEntryBlock = true, populatingExitBlock = false;

                        // We want to walk through the block's statements and collect them.
                        // Statements before the labelled statement go into the entry block.
                        // Statements after the labelled statement go into the exit block.
                        // FIXME: Is the following rule correct? Without it, FaultBlock breaks.
                        // If we hit another labelled statement while filling the exit block, stop filling it.                        
                        for (var i = 0; i < bs.Statements.Count; i++) {
                            var s = bs.Statements[i];

                            if (s.Label == Label) {
                                populatingEntryBlock = false;
                                populatingExitBlock = true;
                            } else if (populatingEntryBlock) {
                                entryBlock.Statements.Add(s);
                            } else if (populatingExitBlock) {
                                if (s.Label == null)
                                    exitBlock.Statements.Add(s);
                                // HACK: The synthesized switch exit labels generated when hoisting a block out of
                                //  a switch statement shouldn't terminate statement collection. Without this hack,
                                //  ForeachInEnumeratorFunctionMonoBinary fails.
                                else if (s.Label.StartsWith("$switchExit"))
                                    ;
                                else
                                    populatingExitBlock = false;
                            }
                        }

                        bs.Statements.Clear();
                        bs.Statements.Add(LabelGroup);
                    } else {
                        throw new NotImplementedException("Unsupported enclosing block type: " + EnclosingBlock.Block.GetType().Name);
                    }
                }
            }
        }

        public class GotoEntry {
            public JSGotoExpression Goto;
            public EnclosingBlockEntry EnclosingBlock;

            public string TargetLabel {
                get {
                    return Goto.TargetLabel;
                }
            }
        }

        public readonly Dictionary<JSStatement, JSLabelGroupStatement> LabelGroups = new Dictionary<JSStatement, JSLabelGroupStatement>(
            new ReferenceComparer<JSStatement>()
        );
        public readonly Dictionary<string, LabelEntry> Labels = new Dictionary<string, LabelEntry>();
        public readonly List<GotoEntry> Gotos = new List<GotoEntry>();
        public readonly HashSet<string> UsedLabels = new HashSet<string>();
        public readonly Stack<EnclosingBlockEntry> BlockStack = new Stack<EnclosingBlockEntry>();

        protected void CheckLabel (JSStatement s) {
            if (s.Label != null)
                Labels.Add(s.Label, new LabelEntry {
                    LabelledStatement = s,
                    ParentNode = ParentNode,
                    EnclosingBlock = BlockStack.Peek()
                });
        }

        protected void PushBlock (JSStatement s) {
            BlockStack.Push(new EnclosingBlockEntry {
                Block = s,
                ParentNode = ParentNode,
                Depth = BlockStack.Count
            });
        }

        public void VisitNode (JSFunctionExpression fe) {
            BlockStack.Push(new EnclosingBlockEntry {
                Block = fe.Body,
                ParentNode = fe,
                Depth = BlockStack.Count
            });

            try {
                VisitChildren(fe);
            } finally {
                BlockStack.Pop();
            }
        }

        public void VisitNode (JSSwitchStatement ss) {
            CheckLabel(ss);

            PushBlock(ss);

            try {
                VisitChildren(ss);
            } finally {
                BlockStack.Pop();
            }
        }

        public void VisitNode (JSBlockStatement bs) {
            CheckLabel(bs);

            PushBlock(bs);

            try {
                VisitChildren(bs);
            } finally {
                BlockStack.Pop();
            }
        }

        public void VisitNode (JSStatement s) {
            CheckLabel(s);

            VisitChildren(s);
        }

        public void VisitNode (JSGotoExpression ge) {
            Gotos.Add(new GotoEntry {
                Goto = ge,
                EnclosingBlock = BlockStack.Peek()
            });

            UsedLabels.Add(ge.TargetLabel);

            VisitChildren(ge);
        }

        public void BuildLabelGroups (JSFunctionExpression function) {
            // If a label is applied to the first statement in a block, hoist it upward
            //  onto the parent block.
            var lh = new LabelHoister();
            do {
                lh.HoistedALabel = false;
                lh.Visit(function);
            } while (lh.HoistedALabel);

            // Walk the function to build our list of labels and gotos.
            Visit(function);

            // When a goto crosses block boundaries, we need to move the target label
            //  upwards so that the goto can reach it.
            foreach (var g in Gotos) {
                var targetLabel = Labels[g.TargetLabel];

                if (targetLabel.EnclosingBlock.Depth > g.EnclosingBlock.Depth)
                    targetLabel.EnclosingBlock = g.EnclosingBlock;
            }

            foreach (var l in Labels.Values) {
                l.EnsureLabelGroupExists(LabelGroups);

                var replacementGoto = new JSExpressionStatement(
                    new JSGotoExpression(l.LabelledStatement.Label)
                );
                l.EnclosingBlock.Block.ReplaceChildRecursive(l.LabelledStatement, replacementGoto);

                l.LabelGroup.Add(l.LabelledStatement);
            }

            // If a label group only contains one label (plus an entry label),
            //  and it has a parent label group, hoist the label up.
            var lgs = new LabelGroupFlattener();
            do {
                lgs.FlattenedAGroup = false;
                lgs.Visit(function);
            } while (lgs.FlattenedAGroup);

            // Remove any labels within a label group that contain no statements (as long
            //  as no goto targets that label directly). This will prune empty entry/exit labels.
            var elr = new EmptyLabelRemover(UsedLabels);
            elr.Visit(function);
        }
    }

    public class LabelHoister : JSAstVisitor {
        public bool HoistedALabel = false;

        protected void MaybeHoist (JSStatement enclosingStatement, IEnumerable<JSStatement> children) {
            var firstChildStatement = children.FirstOrDefault();

            if (firstChildStatement == null)
                return;

            if (firstChildStatement.Label == null)
                return;

            if (enclosingStatement.Label == null) {
                HoistLabel(enclosingStatement, firstChildStatement);
            }
        }

        public void VisitNode (JSBlockStatement bs) {
            MaybeHoist(bs, bs.Children.OfType<JSStatement>());

            VisitChildren(bs);
        }

        public void VisitNode (JSTryCatchBlock tcb) {
            // Hoisting a label out of the try {} body is safe since it always runs.
            MaybeHoist(tcb, tcb.Body.SelfAndChildren.OfType<JSStatement>());

            VisitChildren(tcb);
        }

        // Crossing these kinds of control flow boundaries would change behavior.
        public void VisitNode (JSSwitchCase sc) {
            VisitChildren(sc);
        }

        public void VisitNode (JSIfStatement ifs) {
            VisitChildren(ifs);
        }

        public void VisitNode (JSLoopStatement ls) {
            VisitChildren(ls);
        }

        public void HoistLabel (JSStatement parentBlock, JSStatement labelledStatement) {
            HoistedALabel = true;

            parentBlock.Label = labelledStatement.Label;
            labelledStatement.Label = null;
        }
    }

    public class LabelGroupFlattener : JSAstVisitor {
        public bool FlattenedAGroup = false;
        public Stack<JSLabelGroupStatement> GroupStack = new Stack<JSLabelGroupStatement>();

        protected bool MaybeFlatten (JSLabelGroupStatement lgs, LinkedListNode<string> keyNode) {
            var labelledStatement = lgs.Labels[keyNode.Value];

            var checkStatement = labelledStatement;
            JSLabelGroupStatement labelledGroup = null;

            // If a label contains a single-statement block, recurse down to find a label group.
            while (checkStatement != null) {
                labelledGroup = checkStatement as JSLabelGroupStatement;
                if (labelledGroup != null)
                    break;

                var checkBlock = checkStatement as JSBlockStatement;
                if (checkBlock == null)
                    break;

                if (checkBlock.Statements.Count != 1)
                    break;

                checkStatement = checkBlock.Statements[0];
            }

            if (
                (labelledGroup != null) &&
                (labelledGroup.Labels.Count == 3)
            ) {
                var labels = labelledGroup.Labels.ToArray();

                // Hoist the contents of the entry label into the label that contains this label group.
                var entryLabel = labels[0];
                labelledStatement.ReplaceChildRecursive(labelledGroup, entryLabel.Value);

                // Hoist the single label from this label group into the containing label group, 
                //  after the label that contains this label group.
                var hoistedLabel = labels[1];
                var hoistedKeyNode = lgs.Labels.EnqueueAfter(
                    keyNode, hoistedLabel.Key, hoistedLabel.Value
                );

                var exitLabel = labels[2];
                // Hoist the contents of the exit label into the label that contains this label group.
                lgs.Labels.EnqueueAfter(
                    hoistedKeyNode, hoistedLabel.Key + "_after", exitLabel.Value
                );

                return FlattenedAGroup = true;
            }

            return false;
        }

        public void VisitNode (JSLabelGroupStatement lgs) {
            GroupStack.Push(lgs);

            bool restart = false;

            do {
                restart = false;

                foreach (var keyNode in lgs.Labels.Keys) {
                    if (MaybeFlatten(lgs, keyNode)) {
                        restart = true;
                        break;
                    }
                }
            } while (restart);

            try {
                VisitChildren(lgs);
            } finally {
                GroupStack.Pop();
            }
        }
    }

    public class EmptyLabelRemover : JSAstVisitor {
        public readonly HashSet<string> UsedLabels;

        public EmptyLabelRemover (HashSet<string> usedLabels) {
            UsedLabels = usedLabels;
        }

        public void VisitNode (JSLabelGroupStatement lgs) {
            foreach (var kvp in lgs.Labels.ToArray()) {
                if (UsedLabels.Contains(kvp.Key))
                    continue;

                var labelledBlock = kvp.Value as JSBlockStatement;
                if (labelledBlock == null)
                    continue;

                if (labelledBlock.Statements.Count < 1)
                    lgs.Labels.Remove(kvp.Key);
            }
        }
    }
}
