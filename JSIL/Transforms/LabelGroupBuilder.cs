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

                    labelGroups[EnclosingBlock.Block] = LabelGroup = new JSLabelGroupStatement(index, entryBlock);

                    var bs = EnclosingBlock.Block as JSBlockStatement;
                    if (bs != null) {
                        entryBlock.Statements.AddRange(bs.Statements);
                        bs.Statements.Clear();
                        bs.Statements.Add(LabelGroup);
                    } else {
                        throw new NotImplementedException("Unsupported enclosing block type");
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
            var lh = new LabelHoister();

            // If a label is applied to the first statement in a block, hoist it upward
            //  onto the parent block.
            do {
                lh.HoistedALabel = false;
                lh.Visit(function);
            } while (lh.HoistedALabel);

            Visit(function);

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

            var lgs = new LabelGroupFlattener();

            // If a label group only contains one label (plus an entry label),
            //  and it has a parent label group, hoist the label up.
            do {
                lgs.FlattenedAGroup = false;
                lgs.Visit(function);
            } while (lgs.FlattenedAGroup);
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
            MaybeHoist(tcb, tcb.Body.SelfAndChildren.OfType<JSStatement>());

            VisitChildren(tcb);
        }

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

            while (checkStatement != null) {
                labelledGroup = checkStatement as JSLabelGroupStatement;
                if (labelledGroup != null)
                    break;

                var childStatements = checkStatement.Children.OfType<JSStatement>().ToArray();
                if (childStatements.Length != 1)
                    break;

                checkStatement = childStatements[0];
            }

            if (
                (labelledGroup != null) &&
                (labelledGroup.Labels.Count == 2)
            ) {
                var entryLabel = labelledGroup.Labels.First();
                var hoistedLabel = labelledGroup.Labels.Last();

                labelledStatement.ReplaceChildRecursive(labelledGroup, entryLabel.Value);

                lgs.Labels.EnqueueAfter(
                    keyNode, hoistedLabel.Key, hoistedLabel.Value
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
}
