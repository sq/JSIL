// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.PatternMatching;
using JSIL.Expressions;
using Mono.Cecil;
using ICSharpCode.Decompiler.Ast.Transforms;

namespace JSIL.Transforms {
    /// <summary>
    /// Converts uses of System.Runtime.CompilerServices.CallSite back into equivalent 'dynamic' syntax.
    /// </summary>
    public class DynamicCallSites : ContextTrackingVisitor<object> {
        public struct CallSiteInfo {
            public readonly MemberReferenceExpression CallSite;
            public readonly Expression TargetType;
            public readonly string MemberName;

            public CallSiteInfo (MemberReferenceExpression callSite, Expression targetType, string memberName) {
                CallSite = callSite;
                TargetType = targetType;
                MemberName = memberName;
            }
        }

        public readonly Dictionary<string, CallSiteInfo> CallSites = new Dictionary<string, CallSiteInfo>();

        public DynamicCallSites (DecompilerContext context)
            : base(context) {
        }

        static readonly IfElseStatement siteCheckPattern = new IfElseStatement {
            Condition = new AnyNode("condition"),
            TrueStatement = new AnyNode("true"),
            FalseStatement = Statement.Null
        };

        static readonly ExpressionStatement siteConstructorPattern = new ExpressionStatement {
            Expression = new AssignmentExpression {
                Left = new NamedNode("callSite", new MemberReferenceExpression {
                    Target = new AnyNode()
                }),
                Operator = AssignmentOperatorType.Assign,
                Right = new InvocationExpression {
                    Target = new AnyNode(),
                    Arguments = {
                        new InvocationExpression {
                            Target = new AnyNode(),
                            Arguments = {
                                new AnyNode("binderFlags"),
                                new AnyNode("name"),
                                new AnyNode("typeArguments"),
                                new AnyNode("targetType"),
                                new AnyNode("argumentInfo")
                            }
                        }
                    }
                }
            }
        };

        // Remove site container type declarations
        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            var typeText = typeDeclaration.Name;
            if (typeText.Contains("__SiteContainer"))
                typeDeclaration.Remove();

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitBlockStatement (BlockStatement blockStatement, object data) {
            base.VisitBlockStatement(blockStatement, data);

            Match statementMatch;

            // Locate site declarations and store the information on each site
            foreach (ExpressionStatement stmt in blockStatement.Statements.OfType<ExpressionStatement>().ToArray()) {
                var stmtText = stmt.ToString();
                if (!stmtText.Contains("__SiteContainer"))
                    continue;

                statementMatch = siteConstructorPattern.Match(stmt);
                if (statementMatch != null) {
                    var typeArguments = statementMatch.Get<Expression>("typeArguments");
                    if (typeArguments.First().Descendants.Count() > 0)
                        throw new InvalidOperationException("Type arguments to dynamic invocations not implemented");

                    var callSite = (MemberReferenceExpression)statementMatch.Get<MemberReferenceExpression>("callSite").First().Clone();
                    var targetType = statementMatch.Get<Expression>("targetType").First().Clone();
                    var memberName = statementMatch.Get<PrimitiveExpression>("name").First().Value as string;

                    var callSiteName = callSite.ToString();

                    CallSites.Add(callSiteName, new CallSiteInfo(
                        callSite, targetType, memberName
                    ));
                }
            }

            // Eliminate the initialization checks for the call sites now that we've found them;
            //  this will also remove the site declarations since they're inside the body of the checks
            foreach (IfElseStatement stmt in blockStatement.Statements.OfType<IfElseStatement>().ToArray()) {
                var stmtText = stmt.ToString();
                if (!stmtText.Contains("__SiteContainer"))
                    continue;

                statementMatch = siteCheckPattern.Match(stmt);
                if (statementMatch != null) {
                    var condition = statementMatch.Get<Expression>("condition");

                    stmt.Remove();
                }
            }

            return null;
        }
    }
}
