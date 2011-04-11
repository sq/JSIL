using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL.Expressions {
    /// <summary>
    /// Target.MemberName
    /// </summary>
    public class DynamicCallExpression : Expression {
        public static readonly Role<Expression> CallSiteRole = new Role<Expression>("CallSite", Null);

        public Expression CallSite {
            get { return GetChildByRole(CallSiteRole); }
            set { SetChildByRole(CallSiteRole, value); }
        }

        public Expression Target {
            get { return GetChildByRole(Roles.TargetExpression); }
            set { SetChildByRole(Roles.TargetExpression, value); }
        }

        public string MemberName {
            get {
                return GetChildByRole(Roles.Identifier).Name;
            }
            set {
                SetChildByRole(Roles.Identifier, new Identifier(value, AstLocation.Empty));
            }
        }

        public CSharpTokenNode LChevronToken {
            get { return GetChildByRole(Roles.LChevron); }
        }

        public AstNodeCollection<AstType> TypeArguments {
            get { return GetChildrenByRole(Roles.TypeArgument); }
        }

        public CSharpTokenNode RChevronToken {
            get { return GetChildByRole(Roles.RChevron); }
        }

        public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data) {
            // return visitor.VisitMemberReferenceExpression(this, data);
            return default(S);
        }

        public override bool DoMatch (AstNode other, ICSharpCode.NRefactory.CSharp.PatternMatching.Match match) {
            DynamicCallExpression o = other as DynamicCallExpression;
            return o != null && 
                this.Target.DoMatch(o.Target, match) && 
                MatchString(this.MemberName, o.MemberName) && 
                this.TypeArguments.DoMatch(o.TypeArguments, match);
        }
    }
}
