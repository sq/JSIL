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
    public class LoopNameDetector : JSAstVisitor {
        public readonly Dictionary<int, JSLoopStatement> Loops = new Dictionary<int, JSLoopStatement>();
        public readonly HashSet<int> UsedLoops = new HashSet<int>();

        public void VisitNode (JSLoopStatement ls) {
            Loops.Add(ls.Index.Value, ls);

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

        public void EliminateUnusedLoopNames () {
            foreach (var kvp in Loops) {
                if (!UsedLoops.Contains(kvp.Key))
                    kvp.Value.Index = null;
            }
        }
    }
}
