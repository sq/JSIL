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
    public enum CallSiteType {
        InvokeMember,
        GetMember,
        SetMember
    };

    /// <summary>
    /// Converts uses of System.Runtime.CompilerServices.CallSite back into equivalent 'dynamic' syntax.
    /// </summary>
    public class DynamicCallSites : ContextTrackingVisitor<object> {
        public struct CallSiteInfo {
            public readonly CallSiteType Type;
            public readonly MemberReferenceExpression CallSite;
            public readonly Expression TargetType;
            public readonly string MemberName;

            public CallSiteInfo (CallSiteType type, MemberReferenceExpression callSite, Expression targetType, string memberName) {
                Type = type;
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
            Condition = new BinaryOperatorExpression {
                Operator = BinaryOperatorType.Equality,
                Left = new NamedNode("callSite", new MemberReferenceExpression {
                    Target = new AnyNode()
                }),
                Right = new NullReferenceExpression()
            },
            TrueStatement = new AnyNode("body"),
            FalseStatement = Statement.Null
        };

        static readonly AssignmentExpression siteConstructorPattern = new AssignmentExpression {
            Left = new NamedNode("callSite", new MemberReferenceExpression {
                Target = new AnyNode()
            }),
            Operator = AssignmentOperatorType.Assign,
            Right = new InvocationExpression {
                Target = new MemberReferenceExpression {
                    Target = new AnyNode(),
                    MemberName = "Create"
                },
                Arguments = {
                    new InvocationExpression {
                        Target = new NamedNode("callSiteType", new MemberReferenceExpression {
                            Target = new AnyNode()
                        }),
                        Arguments = {
                            new AnyNode("binderFlags"),
                            new AnyNode("name"),
                            new OptionalNode(new AnyNode("typeArguments")),
                            new AnyNode("targetType"),
                            new AnyNode("argumentInfo")
                        }
                    }
                }
            }
        };

        static readonly InvocationExpression siteInvocationPattern = new InvocationExpression {
            Arguments = {
                new NamedNode("callSite", 
                    new MemberReferenceExpression {
                        Target = new AnyNode()
                    }
                ),
                new AnyNode("instance"),
                new Repeat(new AnyNode("arguments"))
            },
            Target = new AnyNode("siteContainer")
        };

        // Remove site container type declarations
        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            var typeText = typeDeclaration.Name;
            if (typeText.Contains("__SiteContainer"))
                typeDeclaration.Remove();

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitAssignmentExpression (AssignmentExpression assignmentExpression, object data) {
            var match = siteConstructorPattern.Match(assignmentExpression);
            if (match != null) {
                var typeArguments = match.Get<Expression>("typeArguments");
                if ((typeArguments.Count() > 0) && (typeArguments.First().Descendants.Count() > 0))
                    throw new InvalidOperationException("Type arguments to dynamic invocations not implemented");

                var callSite = (MemberReferenceExpression)match.Get<MemberReferenceExpression>("callSite").First().Clone();
                var targetType = match.Get<Expression>("targetType").First().Clone();
                var memberName = match.Get<PrimitiveExpression>("name").First().Value as string;

                var callSiteName = callSite.ToString();
                if (!callSiteName.Contains("__SiteContainer"))
                    return base.VisitAssignmentExpression(assignmentExpression, data);

                CallSiteType callSiteType;
                var invocationType = match.Get<MemberReferenceExpression>("callSiteType").First().MemberName;
                switch (invocationType) {
                    case "InvokeMember":
                        callSiteType = CallSiteType.InvokeMember;
                        break;
                    case "GetMember":
                        callSiteType = CallSiteType.GetMember;
                        break;
                    case "SetMember":
                        callSiteType = CallSiteType.SetMember;
                        break;
                    default:
                        throw new NotImplementedException("Dynamic invocations of type " + invocationType + " are not implemented.");
                }

                CallSites.Add(callSiteName, new CallSiteInfo(
                    callSiteType, callSite, targetType, memberName
                ));

                assignmentExpression.Remove();
                return null;
            }

            return base.VisitAssignmentExpression(assignmentExpression, data);
        }

        public override object VisitBlockStatement (BlockStatement blockStatement, object data) {
            base.VisitBlockStatement(blockStatement, data);

            Match statementMatch;
            // Eliminate the initialization checks for the call sites now that we've found them;
            //  this will also remove the site declarations since they're inside the body of the checks
            foreach (IfElseStatement stmt in blockStatement.Statements.OfType<IfElseStatement>().ToArray()) {
                statementMatch = siteCheckPattern.Match(stmt);
                if (statementMatch != null) {
                    var callSite = statementMatch.Get<MemberReferenceExpression>("callSite").First();
                    var callSiteName = callSite.ToString();

                    if (!callSiteName.Contains("__SiteContainer"))
                        continue;

                    if (!CallSites.ContainsKey(callSiteName)) {
                        Debug.WriteLine("Call site used without definition: " + callSiteName);
                    } else {
                        stmt.Remove();
                    }
                }
            }

            return null;
        }

        public override object VisitInvocationExpression (InvocationExpression invocationExpression, object data) {
            var match = siteInvocationPattern.Match(invocationExpression);
            if (match != null) {
                var callSite = match.Get<MemberReferenceExpression>("callSite").First();

                var callSiteName = callSite.ToString();
                if (!callSiteName.Contains("__SiteContainer"))
                    return null;

                CallSiteInfo info;
                if (!CallSites.TryGetValue(callSiteName, out info)) {
                    Debug.WriteLine("Unknown call site: " + callSiteName);
                    return null;
                    throw new KeyNotFoundException("No information found for call site " + callSiteName);
                }

                var dce = new DynamicExpression {
                    CallSite = callSite.Clone(),
                    CallSiteType = info.Type,
                    MemberName = info.MemberName,
                    TargetType = info.TargetType,
                    Target = match.Get<Expression>("instance").First().Clone()
                };
                dce.Arguments.AddRange(
                    from arg in match.Get<Expression>("arguments")
                    select arg.Clone()
                );

                invocationExpression.ReplaceWith(dce);
            }

            return null;
        }
    }
}
