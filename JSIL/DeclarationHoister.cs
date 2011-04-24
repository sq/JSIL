using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class DeclarationHoister : ContextTrackingVisitor<object> {
        public readonly BlockStatement Output;
        public readonly HashSet<string> HoistedNames = new HashSet<string>();

        public DeclarationHoister (DecompilerContext context, BlockStatement output)
            : base(context) {

            Output = output;
        }

        public override object VisitVariableDeclarationStatement (VariableDeclarationStatement variableDeclarationStatement, object data) {
            var statement = new VariableDeclarationStatement {
                Type = variableDeclarationStatement.Type.Clone()
            };

            foreach (var variable in variableDeclarationStatement.Variables) {
                if (!HoistedNames.Contains(variable.Name)) {
                    statement.Variables.Add(new VariableInitializer(
                        variable.Name
                    ));
                    HoistedNames.Add(variable.Name);
                }
            }

            var replacement = new BlockStatement();
            foreach (var variable in variableDeclarationStatement.Variables) {
                if (variable.IsNull)
                    continue;
                if (variable.Initializer.IsNull)
                    continue;

                var newStmt = new ExpressionStatement(new AssignmentExpression {
                    Left = new IdentifierExpression(variable.Name),
                    Right = variable.Initializer.Clone()
                });

                replacement.Add(newStmt);
            }

            if (replacement.Statements.Count == 1) {
                var firstChild = replacement.FirstChild;
                firstChild.Remove();
                variableDeclarationStatement.ReplaceWith(firstChild);
            } else if (replacement.Statements.Count > 1) {
                variableDeclarationStatement.ReplaceWith(replacement);
            } else {
                variableDeclarationStatement.Remove();
            }

            if (statement.Variables.Count > 0)
                Output.Add(statement);

            return null;
        }
    }
}
