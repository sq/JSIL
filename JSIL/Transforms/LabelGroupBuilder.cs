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

        public void VisitNode (JSDoLoop dl) {
            CheckLabel(dl);

            BlockStack.Push(new EnclosingBlockEntry {
                Block = dl,
                ParentNode = ParentNode,
                Depth = BlockStack.Count
            });

            try {
                VisitChildren(dl);
            } finally {
                BlockStack.Pop();
            }
        }

        public void VisitNode (JSWhileLoop wl) {
            CheckLabel(wl);

            BlockStack.Push(new EnclosingBlockEntry {
                Block = wl,
                ParentNode = ParentNode,
                Depth = BlockStack.Count
            });

            try {
                VisitChildren(wl);
            } finally {
                BlockStack.Pop();
            }
        }

        public void VisitNode (JSForLoop fl) {
            CheckLabel(fl);

            BlockStack.Push(new EnclosingBlockEntry {
                Block = fl,
                ParentNode = ParentNode,
                Depth = BlockStack.Count
            });

            try {
                VisitChildren(fl);
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

        public void BuildLabelGroups () {
            foreach (var g in Gotos) {
                var targetLabel = Labels[g.TargetLabel];
                if (targetLabel.EnclosingBlock.Depth > g.EnclosingBlock.Depth)
                    targetLabel.EnclosingBlock = g.EnclosingBlock;
            }

            foreach (var l in Labels.Values) {
                l.EnsureLabelGroupExists(LabelGroups);

                l.EnclosingBlock.Block.ReplaceChildRecursive(l.LabelledStatement, new JSNullStatement());
                l.LabelGroup.Add(l.LabelledStatement);
            }
        }
    }
}
