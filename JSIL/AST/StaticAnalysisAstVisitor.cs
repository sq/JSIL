using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;

namespace JSIL.Ast {
    public abstract class StaticAnalysisJSAstVisitor : JSAstVisitor {
        public readonly QualifiedMemberIdentifier Member;
        protected readonly IFunctionSource FunctionSource;

        protected StaticAnalysisJSAstVisitor (QualifiedMemberIdentifier member, IFunctionSource functionSource)
            : base() {

            if (functionSource == null)
                throw new ArgumentNullException("functionSource");

            Member = member;
            FunctionSource = functionSource;

            // Console.WriteLine("Static analysis visitor used in function {0}", new System.Diagnostics.StackFrame(2).GetMethod().Name);
        }

        protected FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method) {
            return FunctionSource.GetFirstPass(method, Member);
        }

        protected FunctionAnalysis2ndPass GetSecondPass (JSMethod method) {
            return FunctionSource.GetSecondPass(method, Member);
        }
    }
}
