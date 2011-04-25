using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class BlockTranslator : ContextTrackingVisitor<object> {
        public int NextEnumeratorIndex = 0, NextUsingIndex = 0;

        public BlockTranslator (DecompilerContext context)
            : base(context) {
        }

        protected string GetNewEnumeratorName () {
            return String.Format("_enumerator{0}_", NextEnumeratorIndex++);
        }

        protected string GetNewUsingName () {
            return String.Format("_using{0}_", NextUsingIndex++);
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            NextEnumeratorIndex = 0;

            return base.VisitMethodDeclaration(methodDeclaration, data);
        }

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            NextUsingIndex = 0;

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitForeachStatement (ForeachStatement foreachStatement, object data) {
            var enumeratorName = GetNewEnumeratorName();

            var variable = new VariableDeclarationStatement(
                new SimpleType("IEnumerator"), enumeratorName,
                new InvocationExpression {
                    Target = new MemberReferenceExpression {
                        Target = foreachStatement.InExpression.Clone(),
                        MemberName = "GetEnumerator"
                    }
                }
            );

            var body = foreachStatement.EmbeddedStatement as BlockStatement;
            body.Remove();

            var newStmt = new VariableDeclarationStatement(
                foreachStatement.VariableType.Clone(), 
                foreachStatement.VariableName,
                new MemberReferenceExpression {
                    Target = new IdentifierExpression(enumeratorName),
                    MemberName = "Current"
                }
            );
            if (body.FirstChild != null)
                body.InsertChildBefore(
                    body.FirstChild, newStmt, (Role<Statement>)body.FirstChild.Role
                );
            else
                body.Add(newStmt);

            var result = new TryCatchStatement {
                TryBlock = new BlockStatement {
                    Statements = { 
                        new WhileStatement {
                            Condition = new InvocationExpression {
                                Target = new MemberReferenceExpression {
                                    Target = new IdentifierExpression(enumeratorName),
                                    MemberName = "MoveNext"
                                }
                            },
                            EmbeddedStatement = body
                        }
                    }
                },
                FinallyBlock = new BlockStatement {
                    Statements = {
                        new ExpressionStatement(new InvocationExpression(
                            new MemberReferenceExpression(
                                new IdentifierExpression(variable.Variables.First().Name), 
                                "Dispose"
                            )
                        ))
                    }
                }
            };

            foreachStatement.ReplaceWith(variable);
            variable.Parent.InsertChildAfter(variable, result, (Role<Statement>)(variable.Role));

            return result.AcceptVisitor(this, data);
        }

        public override object VisitUsingStatement (UsingStatement usingStatement, object data) {
            var variable = usingStatement.ResourceAcquisition as VariableDeclarationStatement;
            if (variable == null)
                variable = new VariableDeclarationStatement(
                    new SimpleType("object"), GetNewUsingName(), (Expression)usingStatement.ResourceAcquisition.Clone()
                );
            else
                variable = (VariableDeclarationStatement)variable.Clone();

            var embedded = usingStatement.EmbeddedStatement as BlockStatement;
            if (embedded == null)
                embedded = new BlockStatement {
                    Statements = {
                        usingStatement.EmbeddedStatement.Clone()
                    }
                };
            else
                embedded = (BlockStatement)embedded.Clone();

            var result = new TryCatchStatement {
                TryBlock = embedded,
                FinallyBlock = new BlockStatement {
                    Statements = {
                        new ExpressionStatement(new InvocationExpression(
                            new MemberReferenceExpression(
                                new IdentifierExpression(variable.Variables.First().Name), 
                                "Dispose"
                            )
                        ))
                    }
                }
            };

            usingStatement.ReplaceWith(variable);
            variable.Parent.InsertChildAfter(variable, result, (Role<Statement>)(variable.Role));

            return result.AcceptVisitor(this, data);
        }
    }
}
