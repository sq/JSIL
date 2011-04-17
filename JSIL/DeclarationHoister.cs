using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class DeclarationHoister : ContextTrackingVisitor<object> {
        public readonly BlockStatement Output;
        public VariableDeclarationStatement Statement = null;

        public DeclarationHoister (DecompilerContext context, BlockStatement output)
            : base(context) {

            Output = output;
        }

        public override object VisitVariableDeclarationStatement (VariableDeclarationStatement variableDeclarationStatement, object data) {
            if (Statement == null) {
                Statement = new VariableDeclarationStatement();
                Output.InsertChildAfter(Output.FirstChild, Statement, (Role<Statement>)Output.FirstChild.Role);
            }

            foreach (var variable in variableDeclarationStatement.Variables)
                Statement.Variables.Add(new VariableInitializer(
                    variable.Name
                ));

            var replacement = new BlockStatement();
            foreach (var variable in variableDeclarationStatement.Variables) {
                replacement.Add(new ExpressionStatement(new AssignmentExpression {
                    Left = new IdentifierExpression(variable.Name),
                    Right = variable.Initializer.Clone()
                }));
            }

            if (replacement.Statements.Count == 1) {
                var firstChild = replacement.FirstChild;
                firstChild.Remove();
                variableDeclarationStatement.ReplaceWith(firstChild);
            } else if (replacement.Statements.Count > 1) {
                variableDeclarationStatement.ReplaceWith(replacement);
            }

            return null;
        }
    }
}
