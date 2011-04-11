using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.PatternMatching;
using JSIL.Expressions;
using ICSharpCode.Decompiler.Ast.Transforms;

namespace JSIL.Transforms {
    public enum CallSiteType {
        InvokeMember,
        GetMember,
        SetMember,
        Convert,
        BinaryOperator
    };

    /// <summary>
    /// Converts uses of System.Runtime.CompilerServices.CallSite back into equivalent 'dynamic' syntax.
    /// </summary>
    public class DynamicCallSites : ContextTrackingVisitor<object> {
        public struct TemporaryValue {
            public readonly string Name;
            public readonly AstType Type;
            public readonly Expression Value;

            public TemporaryValue (string name, AstType type, Expression value) {
                Name = name;
                Type = type;
                Value = value;
            }

            public TemporaryValue Replace (Expression newValue) {
                return new TemporaryValue(Name, Type, newValue);
            }
        }

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

        public readonly Dictionary<string, TemporaryValue> TemporaryValues = new Dictionary<string, TemporaryValue>();
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
                            new OptionalNode(new AnyNode("name")),
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

        static readonly VariableDeclarationStatement temporaryValuePattern = new VariableDeclarationStatement {
            Type = new AnyNode("type"),
            Variables = {
                new Repeat(new NamedNode("initializer", new VariableInitializer(
                    "", new AnyNode("value")
                )))
            }
        };

        // Remove site container type declarations
        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            var typeText = typeDeclaration.Name;
            if (typeText.Contains("__SiteContainer"))
                typeDeclaration.Remove();

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitIdentifierExpression (IdentifierExpression identifierExpression, object data) {
            TemporaryValue tv;
            if (TemporaryValues.TryGetValue(identifierExpression.Identifier, out tv)) {
                bool[] a = data as bool[];
                a[0] = true;
                identifierExpression.ReplaceWith(tv.Value);
                return null;
            }

            return base.VisitIdentifierExpression(identifierExpression, data);
        }

        public override object VisitVariableDeclarationStatement (VariableDeclarationStatement variableDeclarationStatement, object data) {
            var match = temporaryValuePattern.Match(variableDeclarationStatement);
            if (match != null) {
                foreach (var initializer in match.Get<VariableInitializer>("initializer")) {
                    var name = initializer.Name;
                    var type = variableDeclarationStatement.Type;
                    var value = initializer.Initializer;

                    if (!value.ToString().Contains("__SiteContainer"))
                        continue;

                    if (!type.ToString().Contains("CallSite")) {
                        continue;
                    }

                    TemporaryValue tv;
                    if (TemporaryValues.TryGetValue(name, out tv))
                        TemporaryValues[name] = tv.Replace(value.Clone());
                    else
                        TemporaryValues[name] = new TemporaryValue(name, type, value.Clone());

                    initializer.Remove();
                }

                if (variableDeclarationStatement.Variables.Count == 0) {
                    variableDeclarationStatement.Remove();
                    return null;
                }
            }

            return base.VisitVariableDeclarationStatement(variableDeclarationStatement, data);
        }

        public override object VisitAssignmentExpression (AssignmentExpression assignmentExpression, object data) {
            var match = siteConstructorPattern.Match(assignmentExpression);
            if (match != null) {
                var typeArguments = match.Get<Expression>("typeArguments");
                if ((typeArguments.Count() > 0) && (typeArguments.First().Descendants.Count() > 0))
                    throw new InvalidOperationException("Type arguments to dynamic invocations not implemented");

                var callSite = (MemberReferenceExpression)match.Get<MemberReferenceExpression>("callSite").First().Clone();
                var targetType = match.Get<Expression>("targetType").First().Clone();
                string memberName = null;
                {
                    var temp = match.Get("name").FirstOrDefault() as PrimitiveExpression;
                    if (temp != null)
                        memberName = temp.Value as string;
                }

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
                    case "Convert":
                        callSiteType = CallSiteType.Convert;
                        break;
                    case "BinaryOperation":
                        callSiteType = CallSiteType.BinaryOperator;
                        memberName = match.Get<MemberReferenceExpression>("name").First().MemberName;
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
            bool[] needSecondPass = new bool[] { false };

            base.VisitBlockStatement(blockStatement, needSecondPass);

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

            // Do a second pass because we may have replaced variables
            if (needSecondPass[0]) {
                foreach (var statement in blockStatement.Statements)
                    statement.AcceptVisitor(this, data);
            }

            return null;
        }

        public override object VisitInvocationExpression (InvocationExpression invocationExpression, object data) {
            var match = siteInvocationPattern.Match(invocationExpression);
            if (match != null) {
                var callSite = match.Get<MemberReferenceExpression>("callSite").First();

                var callSiteName = callSite.ToString();
                if (!callSiteName.Contains("__SiteContainer"))
                    return base.VisitInvocationExpression(invocationExpression, data);

                CallSiteInfo info;
                if (!CallSites.TryGetValue(callSiteName, out info)) {
                    Debug.WriteLine("Unknown call site: " + callSiteName);
                    return base.VisitInvocationExpression(invocationExpression, data);
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
                return null;
            }
            
            return base.VisitInvocationExpression(invocationExpression, data);
        }
    }
}
