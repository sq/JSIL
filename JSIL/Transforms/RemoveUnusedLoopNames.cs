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
    public class LoopNameDetector : JSAstVisitor {
        public readonly Dictionary<int, JSLoopStatement> Loops = new Dictionary<int, JSLoopStatement>();
        public readonly HashSet<int> UsedLoops = new HashSet<int>();

        public void VisitNode (JSLoopStatement ls) {
            if (!Loops.ContainsKey(ls.Index.Value))
                Loops.Add(ls.Index.Value, ls);
            else
                throw new InvalidDataException(String.Format("Found two loops numbered {0}", ls.Index.Value));

            VisitChildren(ls);
        }

        public void VisitNode (JSBreakExpression be) {
            if (be.TargetLoop.HasValue)
                UsedLoops.Add(be.TargetLoop.Value);

            VisitChildren(be);
        }

        public void VisitNode (JSContinueExpression ce) {
            if (ce.TargetLoop.HasValue)
                UsedLoops.Add(ce.TargetLoop.Value);

            VisitChildren(ce);
        }

        public void VisitNode (JSLabelGroupStatement lgs) {
            // HACK: Issue #827 depends on loops containing labelgroups having an index
            //  so that continues and breaks can be targeted.

            foreach (var ls in Stack.OfType<JSLoopStatement>())
                UsedLoops.Add(ls.Index.Value);

            VisitChildren(lgs);
        }

        public void EliminateUnusedLoopNames () {
            foreach (var kvp in Loops) {
                if (!UsedLoops.Contains(kvp.Key))
                    kvp.Value.Index = null;
            }
        }
    }
}
