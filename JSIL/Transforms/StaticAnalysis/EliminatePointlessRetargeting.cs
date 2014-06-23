using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EliminatePointlessRetargeting : StaticAnalysisJSAstVisitor {
        public const bool Trace = false;

        private struct RetargetKey {
            public readonly JSExpression Variable;
            public readonly JSExpression Array;
            public readonly JSExpression Index;

            public RetargetKey (JSExpression variable, JSExpression array, JSExpression index) {
                Variable = variable;
                Array = array;
                Index = index;
            }

            public override int GetHashCode () {
                return Array.GetHashCode() ^ Index.GetHashCode();
            }

            public override bool Equals (object obj) {
                if (obj is RetargetKey)
                    return Equals((RetargetKey)obj);
                else
                    return false;
            }

            public bool Equals (RetargetKey obj) {
                return Object.Equals(Variable, obj.Variable) && 
                    Object.Equals(Array, obj.Array) &&
                    Object.Equals(Index, obj.Index);
            }
        }

        private readonly Stack<int> ScopeNodeIndices = new Stack<int>();
        private readonly Stack<Dictionary<RetargetKey, int>> SeenRetargetsInScope = new Stack<Dictionary<RetargetKey, int>>();

        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;

        private FunctionAnalysis1stPass FirstPass = null;

        private JSFunctionExpression Function;

        public EliminatePointlessRetargeting (
            QualifiedMemberIdentifier member, 
            IFunctionSource functionSource, 
            TypeSystem typeSystem,
            MethodTypeFactory methodTypes
        ) 
            : base (member, functionSource) {
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;

            SeenRetargetsInScope.Push(new Dictionary<RetargetKey, int>());
            ScopeNodeIndices.Push(-1);
        }

        public void VisitNode (JSFunctionExpression fn) {
            Function = fn;
            FirstPass = GetFirstPass(Function.Method.QualifiedIdentifier);

            VisitChildren(fn);
        }

        public void VisitNode (JSBlockStatement block) {
            SeenRetargetsInScope.Push(new Dictionary<RetargetKey, int>());
            ScopeNodeIndices.Push(NodeIndex);

            VisitChildren(block);

            SeenRetargetsInScope.Pop();
            ScopeNodeIndices.Pop();
        }

        public void VisitNode (JSRetargetPackedArrayElementProxy rpaep) {
            var key = new RetargetKey(
                rpaep.ElementProxy, rpaep.Array, rpaep.Index
            );
            var sris = SeenRetargetsInScope.Peek();
            int cacheLastInitializedNodeIndex;

            if (!sris.TryGetValue(key, out cacheLastInitializedNodeIndex)) {
                sris.Add(key, NodeIndex);
                VisitChildren(rpaep);
                return;
            }

            var arrayIsConstant = IsCachedValueValidForGivenCacheKey(rpaep.Array, cacheLastInitializedNodeIndex);
            var indexIsConstant = IsCachedValueValidForGivenCacheKey(rpaep.Index, cacheLastInitializedNodeIndex);

            if (arrayIsConstant && indexIsConstant) {
                if (Trace)
                    Console.WriteLine("Eliminating retarget for " + rpaep.ElementProxy + " because array and index are constant");

                ParentNode.ReplaceChild(rpaep, rpaep.ElementProxy);
                VisitReplacement(rpaep.ElementProxy);
                return;
            }

            VisitChildren(rpaep);
        }

        protected bool IsCachedValueValidForGivenCacheKey (JSExpression cacheKey, int cachedValueLastInitializationNodeIndex) {
            if (cacheKey.IsConstant)
                return true;

            var variable = cacheKey as JSVariable;
            if (variable != null) {
                var scopeNodeIndex = ScopeNodeIndices.Peek();
                var relevantAssignments = FirstPass.Assignments
                    .Where((a) => a.Target == variable.Identifier)
                    .Where((a) => a.ParentNodeIndices.Contains(scopeNodeIndex))
                    .ToArray();

                // If the variable is never assigned to inside this scope, we can treat it as constant.
                if (relevantAssignments.Length == 0)
                    return true;
                
                // TODO: Take an input index (point of original creation) and determine whether
                //  the variable has changed since the creation point, thus invalidating the cached value
            }

            return false;
        }
    }
}
