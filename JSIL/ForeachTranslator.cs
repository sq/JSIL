using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class ForeachTranslator : ContextTrackingVisitor<object> {
        public int NextIndex = 0;

        public ForeachTranslator (DecompilerContext context)
            : base(context) {
        }

        protected string GetNewVariableName () {
            return String.Format("_enumerator{0}_", NextIndex++);
        }

        public override object VisitForeachStatement (ForeachStatement foreachStatement, object data) {
            var enumeratorName = GetNewVariableName();
            var initializer = new VariableDeclarationStatement(
                new SimpleType("IEnumerator"), enumeratorName,
                new InvocationExpression {
                    Target = new MemberReferenceExpression {
                        Target = foreachStatement.InExpression.Clone(),
                        MemberName = "GetEnumerator"
                    }
                }
            );

            var whileStatement = new WhileStatement {
                Condition = new InvocationExpression {
                    Target = new MemberReferenceExpression {
                        Target = new IdentifierExpression(enumeratorName),
                        MemberName = "MoveNext"
                    }
                }
            };

            var result = new BlockStatement {
                Statements = {
                    initializer,
                    whileStatement
                }
            };

            var body = foreachStatement.EmbeddedStatement;
            body.Remove();

            body.InsertChildBefore(
                body.FirstChild, new VariableDeclarationStatement(
                    (AstType)(foreachStatement.VariableType.Clone()), 
                    foreachStatement.VariableName,
                    new MemberReferenceExpression {
                        Target = new IdentifierExpression(enumeratorName),
                        MemberName = "Current"
                    }
                ), (Role<Statement>)body.FirstChild.Role
            );
            whileStatement.EmbeddedStatement = body;

            foreachStatement.ReplaceWith(result);

            return null;
        }
    }
}
