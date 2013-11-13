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
        }

        public void VisitNode (JSFunctionExpression fn) {
            Function = fn;
            FirstPass = GetFirstPass(Function.Method.QualifiedIdentifier);

            VisitChildren(fn);
        }

        public void VisitNode (JSRetargetPackedArrayElementProxy rpaep) {
            Console.WriteLine("Is " + rpaep.ElementProxy + " constant in scope? " + IsEffectivelyConstantInScope(rpaep.ElementProxy, Stack.OfType<JSBlockStatement>().First()));

            VisitChildren(rpaep);
        }

        protected bool IsEffectivelyConstantInScope (JSExpression variable, JSBlockStatement scope) {
            if (variable.IsConstant)
                return true;

            return false;
        }
    }
}
