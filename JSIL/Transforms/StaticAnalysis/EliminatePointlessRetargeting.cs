using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EliminatePointlessRetargeting : StaticAnalysisJSAstVisitor {
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
        private readonly Stack<HashSet<RetargetKey>> SeenRetargetsInScope = new Stack<HashSet<RetargetKey>>();

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

            SeenRetargetsInScope.Push(new HashSet<RetargetKey>());
            ScopeNodeIndices.Push(-1);
        }

        public void VisitNode (JSFunctionExpression fn) {
            Function = fn;
            FirstPass = GetFirstPass(Function.Method.QualifiedIdentifier);

            VisitChildren(fn);
        }

        public void VisitNode (JSBlockStatement block) {
            SeenRetargetsInScope.Push(new HashSet<RetargetKey>());
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

            if (!sris.Contains(key)) {
                sris.Add(key);
                VisitChildren(rpaep);
                return;
            }

            var arrayIsConstant = IsConstantInCurrentScope(rpaep.Array);
            var indexIsConstant = IsConstantInCurrentScope(rpaep.Index);

            Console.WriteLine("Constant: array={0}, index={1}", arrayIsConstant, indexIsConstant);

            VisitChildren(rpaep);
        }

        protected bool IsConstantInCurrentScope (JSExpression expression) {
            if (expression.IsConstant)
                return true;

            var variable = expression as JSVariable;
            if (variable != null) {
                var scopeNodeIndices = NodeIndexStack.ToArray();
                var relevantAssignments = FirstPass.Assignments
                    .Where((a) => a.Target.Identifier == variable.Identifier)
                    .ToArray();

                Debugger.Break();
            }

            return false;
        }
    }
}
