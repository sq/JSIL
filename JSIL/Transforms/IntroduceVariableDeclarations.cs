using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;

namespace JSIL.Transforms {
    public class IntroduceVariableDeclarations : JSAstVisitor {
        public readonly List<ILVariable> Variables = new List<ILVariable>();

        public IntroduceVariableDeclarations (IEnumerable<ILVariable> allVariables) {
            Variables.AddRange(allVariables);
        }

        public void VisitNode (JSFunctionExpression fn) {
            if (Variables.Count > 0)
                fn.Body.Statements.Insert(
                    0, new JSVariableDeclarationStatement(
                        (from v in Variables select new JSBinaryOperatorExpression(
                            JSOperator.Assignment, new JSVariable(v.Name, v.Type), JSLiteral.DefaultValue(v.Type)
                        )).ToArray()
                    )
                );
        }
    }
}
