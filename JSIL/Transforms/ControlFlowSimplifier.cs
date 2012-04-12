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
    public class ControlFlowSimplifier : JSAstVisitor {
        private static int TraceLevel = 0;

        private readonly Stack<JSBlockStatement> BlockStack = new Stack<JSBlockStatement>();
        private readonly List<int> AbsoluteJumpsSeenStack = new List<int>();
        private readonly Stack<JSSwitchCase> SwitchCaseStack = new Stack<JSSwitchCase>();

        private JSSwitchCase LastSwitchCase = null;

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

            if (TraceLevel >= 2) {
                if (sc.Values != null)
                    Console.WriteLine("// Entering case {0}", sc.Values.FirstOrDefault());
                else
                    Console.WriteLine("// Entering case default");
            }

            VisitChildren(sc);

            if (TraceLevel >= 2)
                Console.WriteLine("// Exiting case");

            SwitchCaseStack.Pop();
        }

        public void VisitNode (JSBlockStatement bs) {
            var thisSwitchCase = ParentSwitchCase;
            var parentLabelGroup = ParentNode as JSLabelGroupStatement;
            var isControlFlow = bs.IsControlFlow || 
                (thisSwitchCase != LastSwitchCase) || 
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

            LastSwitchCase = thisSwitchCase;
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

        public void VisitNode (JSContinueExpression ce) {
            var stackSlice = Stack.Take(3).ToArray();
            var parentEs = stackSlice[1] as JSExpressionStatement;
            var parentBlock = stackSlice[2] as JSBlockStatement;

            if ((parentEs != null) && (parentBlock == BlockStack.Peek())) {
                AbsoluteJumpsSeen += 1;

                if (AbsoluteJumpsSeen > 1) {
                    if (TraceLevel >= 1)
                        Console.WriteLine("// Eliminating {0}", ce);

                    var replacement = new JSNullExpression();
                    ParentNode.ReplaceChild(ce, replacement);
                    return;
                } else {
                    if (TraceLevel >= 3)
                        Console.WriteLine("// Not eliminating {0}", ce);
                }
            }

            VisitChildren(ce);
        }

        public void VisitNode (JSBreakExpression be) {
            var stackSlice = Stack.Take(3).ToArray();
            var parentEs = stackSlice[1] as JSExpressionStatement;
            var parentBlock = stackSlice[2] as JSBlockStatement;

            if ((parentEs != null) && (parentBlock == BlockStack.Peek())) {
                AbsoluteJumpsSeen += 1;

                if (AbsoluteJumpsSeen > 1) {
                    if (TraceLevel >= 1)
                        Console.WriteLine("// Eliminating {0}", be);

                    var replacement = new JSNullExpression();
                    ParentNode.ReplaceChild(be, replacement);
                    return;
                } else {
                    if (TraceLevel >= 3)
                        Console.WriteLine("// Not eliminating {0}", be);
                }
            }

            VisitChildren(be);
        }

        public void VisitNode (JSGotoExpression ge) {
            var stackSlice = Stack.Take(3).ToArray();
            var parentEs = stackSlice[1] as JSExpressionStatement;
            var parentBlock = stackSlice[2] as JSBlockStatement;

            if ((parentEs != null) && (parentBlock == BlockStack.Peek())) {
                AbsoluteJumpsSeen += 1;

                if (AbsoluteJumpsSeen > 1) {
                    if (TraceLevel >= 1)
                        Console.WriteLine("// Eliminating {0}", ge);

                    var replacement = new JSNullExpression();
                    ParentNode.ReplaceChild(ge, replacement);
                    return;
                } else {
                    if (TraceLevel >= 3)
                        Console.WriteLine("// Not eliminating {0}", ge);
                }
            }

            VisitChildren(ge);
        }
    }
}
