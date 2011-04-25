using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class GotoConverter : ContextTrackingVisitor<object> {
        public struct EnumStatementItem {
            public AstNode Parent;
            public Statement Statement;
            public int Depth;
        }

        public class BlockInfo {
            public readonly int Index;
            public readonly BlockStatement HoistBlock;
            public readonly BlockStatement Block;
            public readonly Dictionary<string, LabelInfo> Labels = new Dictionary<string, LabelInfo>();
            public readonly List<GotoInfo> Gotos = new List<GotoInfo>();
            public readonly HashSet<string> LabelReferences = new HashSet<string>();
            public readonly List<BlockInfo> ChildBlocks = new List<BlockInfo>();

            public BlockInfo (int index, BlockStatement block, BlockStatement hoist) {
                Block = block;
                Index = index;
                HoistBlock = hoist;
            }

            public bool ContainsLabel (string name) {
                if (Labels.ContainsKey(name))
                    return true;

                foreach (var block in ChildBlocks)
                    if (block.ContainsLabel(name))
                        return true;

                return false;
            }

            public IEnumerable<GotoInfo> AllGotos {
                get {
                    foreach (var g in Gotos)
                        yield return g;

                    foreach (var block in ChildBlocks)
                        foreach (var g in block.AllGotos)
                            yield return g;
                }
            }

            public IEnumerable<string> AllReferences {
                get {
                    foreach (var l in LabelReferences)
                        yield return l;

                    foreach (var block in ChildBlocks)
                        foreach (var l in block.AllReferences)
                            yield return l;
                }
            }

            protected static IEnumerable<EnumStatementItem> EnumStatements (AstNode parent, int depth = 0) {
		        AstNode next;
		        for (var child = parent.FirstChild; child != null; child = next) {
			        next = child.NextSibling;

                    var stmt = child as Statement;
                    if (stmt != null)
                        yield return new EnumStatementItem {
                            Statement = stmt,
                            Parent = parent,
                            Depth = depth
                        };
                    
                    foreach (var inner in EnumStatements(child, depth + 1))
                        yield return inner;
		        }
            }

            protected static string Indent (string text) {
                return String.Join(
                    Environment.NewLine,
                    (from line
                     in text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                     select "  " + line).ToArray()
                );
            }

            public override string ToString () {
                var sb = new StringBuilder();
                sb.AppendLine("{");

                if (Labels.Count > 0)
                    sb.AppendFormat(
                        "  labels=({0})\r\n", 
                        String.Join(", ", Labels.Keys.ToArray())
                    );

                if (Gotos.Count > 0)
                    sb.AppendFormat(
                        "  gotos=({0})\r\n", 
                        String.Join(", ", (from g in Gotos select g.LabelName).ToArray())
                    );

                foreach (var block in ChildBlocks)
                    sb.AppendLine(Indent(block.ToString()));

                sb.AppendLine("}");

                return sb.ToString();
            }

            public void PruneLabels (HashSet<string> referencedLabels = null, HashSet<string> gotos = null) {
                if (referencedLabels == null)
                    referencedLabels = new HashSet<string>(AllReferences);
                if (gotos == null)
                    gotos = new HashSet<string>(from g in AllGotos select g.LabelName);

                foreach (var label in this.Labels.ToArray()) {
                    if (!gotos.Contains(label.Key))
                        Labels.Remove(label.Key);
                       
                    if (!referencedLabels.Contains(label.Key))
                        label.Value.Label.Remove();
                }

                foreach (var block in ChildBlocks)
                    block.PruneLabels(referencedLabels);
            }

            public BlockStatement TransformLabels (DecompilerContext context) {
                foreach (var block in ChildBlocks)
                    block.TransformLabels(context);

                if (Labels.Count == 0)
                    return this.Block;

                var labelVariableName = String.Format("_label{0}_", Index);
                var labelVariable = new VariableDeclarationStatement(
                    AstType.Create(typeof(string)), labelVariableName, 
                    new PrimitiveExpression("::enter")
                );
                var labelIdentifier = new IdentifierExpression(labelVariableName);

                var switchStatement = new SwitchStatement {
                    Expression = labelIdentifier,
                };
                var whileStatement = new WhileStatement {
                    Condition = new PrimitiveExpression(true),
                    EmbeddedStatement = new BlockStatement {
                        switchStatement
                    }
                };

                var loopName = String.Format("_step{0}_", Index);
                var nextStepStatement = new TargetedContinueStatement(loopName);

                var result = new BlockStatement {
                    labelVariable,
                    new LabelStatement {
                        Label = loopName
                    },
                    whileStatement
                };

                Block.ReplaceWith(result);

                var hoister = new DeclarationHoister(context, HoistBlock);

                Func<string, BlockStatement> makeNewSection = (name) => {
                    var ss = new SwitchSection {
                        CaseLabels = {
                            new CaseLabel {
                                Expression = new PrimitiveExpression(name)
                            }
                        }
                    };

                    var bs = new BlockStatement();
                    ss.Statements.Add(bs);

                    switchStatement.SwitchSections.Add(ss);

                    return bs;
                };

                Func<string, ExpressionStatement> buildGoto = (label) => {
                    return new ExpressionStatement(new AssignmentExpression {
                        Left = labelIdentifier.Clone(),
                        Right = new PrimitiveExpression(label)
                    });
                };

                var currentSection = makeNewSection("::enter");
                var traversalQueue = new LinkedList<AstNode>();

                var blockClone = Block.Clone();
                currentSection.Add(blockClone);
                blockClone.AcceptVisitor(hoister, null);

                traversalQueue.AddLast(blockClone.FirstChild);

                while (traversalQueue.Count > 0) {
                    var first = traversalQueue.First;
                    var current = first.Value;
                    traversalQueue.RemoveFirst();

                    var next = current.NextSibling;
                    if (next != null)
                        traversalQueue.AddLast(next);

                    var s = current as Statement;
                    var l = current as LabelStatement;
                    var g = current as GotoStatement;

                    if (l != null && !IsSpecialLabel(l.Label)) {
                        var gotoNext = buildGoto(l.Label);
                        current.ReplaceWith(gotoNext);
                        gotoNext.Parent.InsertChildAfter(gotoNext, nextStepStatement.Clone(), (Role<Statement>)gotoNext.Role);
                        var newSection = makeNewSection(l.Label);

                        var transplant = next;
                        while (transplant != null) {
                            var nextTransplant = transplant.NextSibling;

                            var tstmt = (Statement)transplant;

                            tstmt.Remove();
                            newSection.Add(tstmt);

                            transplant = nextTransplant;
                        }

                        currentSection = newSection;
                    } else if (g != null) {
                        var gotoNext = buildGoto(g.Label);
                        current.ReplaceWith(gotoNext);
                        gotoNext.Parent.InsertChildAfter(gotoNext, nextStepStatement.Clone(), (Role<Statement>)gotoNext.Role);
                    } else {
                        var child = current.FirstChild;
                        if (child != null)
                            traversalQueue.AddFirst(child);
                    }
                }

                currentSection.Add(
                    new TargetedBreakStatement(loopName)
                );

                return result;
            }
        }

        public class GotoInfo {
            public readonly BlockInfo Block;
            public readonly GotoStatement Goto;
            public readonly string LabelName;

            public GotoInfo (BlockInfo block, GotoStatement stmt, string label) {
                Block = block;
                Goto = stmt;
                LabelName = label;
            }
        }

        public class LabelInfo {
            public readonly BlockInfo Block;
            public readonly LabelStatement Label;
            public readonly string Name;

            public LabelInfo (BlockInfo block, LabelStatement stmt, string name) {
                Block = block;
                Label = stmt;
                Name = name;
            }
        }

        protected int NextIndex = 0;

        protected readonly Queue<BlockInfo> PendingBlocks = new Queue<BlockInfo>();
        protected readonly Stack<BlockInfo> Blocks = new Stack<BlockInfo>();
        protected readonly Dictionary<string, LabelInfo> Labels = new Dictionary<string, LabelInfo>();

        public GotoConverter (DecompilerContext context)
            : base(context) {
        }

        public static bool IsSpecialLabel (string label) {
            if (label.StartsWith("_block") || label.StartsWith("_step"))
                return true;

            return false;
        }

        public override object VisitLabelStatement (LabelStatement labelStatement, object data) {
            var block = Blocks.Peek();
            var info = new LabelInfo(block, labelStatement, labelStatement.Label);
            block.Labels[info.Name] = info;
            Labels[info.Name] = info;
            return base.VisitLabelStatement(labelStatement, data);
        }

        public override object VisitGotoStatement (GotoStatement gotoStatement, object data) {
            var block = Blocks.Peek();

            var info = new GotoInfo(block, gotoStatement, gotoStatement.Label);
            block.Gotos.Add(info);
            block.LabelReferences.Add(gotoStatement.Label);

            return base.VisitGotoStatement(gotoStatement, data);
        }

        public override object VisitBreakStatement (BreakStatement breakStatement, object data) {
            var tbs = breakStatement as TargetedBreakStatement;

            if (tbs != null)
                Blocks.Peek().LabelReferences.Add(tbs.LabelName);

            return base.VisitBreakStatement(breakStatement, data);
        }

        public override object VisitContinueStatement (ContinueStatement continueStatement, object data) {
            var tcs = continueStatement as TargetedContinueStatement;

            if (tcs != null)
                Blocks.Peek().LabelReferences.Add(tcs.LabelName);

            return base.VisitContinueStatement(continueStatement, data);
        }

        public override object VisitBlockStatement (BlockStatement blockStatement, object data) {
            if (Blocks.Count == 0)
                NextIndex = 0;

            bool hoistBlockIsNew = false;
            BlockInfo parentBlock = null;
            BlockStatement hoistBlock = null;
            if (Blocks.Count != 0) {
                parentBlock = Blocks.Peek();
                hoistBlock = parentBlock.HoistBlock;
            }

            if (hoistBlock == null) {
                hoistBlock = new BlockStatement();
                hoistBlockIsNew = true;
            }
                
            var info = new BlockInfo(NextIndex++, blockStatement, hoistBlock);

            Blocks.Push(info);
            var result = base.VisitBlockStatement(blockStatement, data);
            Blocks.Pop();

            bool selfContained = true;
            int c = 0;
            foreach (var g in info.AllGotos) {
                c += 1;

                if (!info.ContainsLabel(g.LabelName))
                    selfContained = false;
            }

            BlockStatement newBlock = blockStatement;

            if (selfContained && (c > 0)) {
                info.PruneLabels();

                newBlock = info.TransformLabels(context);
            } else if (parentBlock != null) {
                parentBlock.ChildBlocks.Add(info);
            } else if (c > 0) {
                blockStatement.ReplaceWith(new BlockStatement {
                    Statements = {
                        new ThrowStatement(new ObjectCreateExpression(
                            AstType.Create(typeof(NotImplementedException)),
                            new PrimitiveExpression("This function uses goto in a way that cannot be translated to javascript")
                        ))
                    }
                });
            }

            if (hoistBlockIsNew && selfContained) {
                foreach (var statement in hoistBlock.Statements) {
                    statement.Remove();
                    if (newBlock.FirstChild != null)
                        newBlock.InsertChildBefore(newBlock.FirstChild, statement, newBlock.FirstChild.Role as Role<Statement>);
                    else
                        newBlock.Add(statement);
                }
            }

            return result;
        }
    }

    public interface ITargetedControlFlowVisitor<T, S> {
        S VisitTargetedBreakStatement (TargetedBreakStatement targetedBreakStatement, T data);
        S VisitTargetedContinueStatement (TargetedContinueStatement targetedContinueStatement, T data);
    }

    public class TargetedBreakStatement : BreakStatement {
        public string LabelName;

        public TargetedBreakStatement (string name) {
            LabelName = name;
        }

        public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data) {
            var tbv = visitor as ITargetedControlFlowVisitor<T, S>;
            if (tbv != null)
                return tbv.VisitTargetedBreakStatement(this, data);
            else
                return base.AcceptVisitor(visitor, data);
        }
    }

    public class TargetedContinueStatement : ContinueStatement {
        public string LabelName;

        public TargetedContinueStatement (string name) {
            LabelName = name;
        }

        public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data) {
            var tbv = visitor as ITargetedControlFlowVisitor<T, S>;
            if (tbv != null)
                return tbv.VisitTargetedContinueStatement(this, data);
            else
                return base.AcceptVisitor(visitor, data);
        }
    }
}
