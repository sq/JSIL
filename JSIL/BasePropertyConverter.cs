using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace JSIL {
    public class BasePropertyConverter : ContextTrackingVisitor<object> {
        public BasePropertyConverter (DecompilerContext context)
            : base(context) {
        }

        public override object VisitAssignmentExpression (AssignmentExpression assignmentExpression, object data) {
            var mre = assignmentExpression.Left as MemberReferenceExpression;
            if ((mre != null) && (mre.Target is BaseReferenceExpression)) {
                var pd = mre.Annotation<PropertyDefinition>();

                if (pd != null) {
                    assignmentExpression.ReplaceWith(new InvocationExpression {
                        Target = new MemberReferenceExpression {
                            Target = mre.Target.Clone(),
                            MemberName = String.Format("set_{0}", pd.Name)
                        },
                        Arguments = {
                            assignmentExpression.Right.Clone()
                        }
                    });

                    return null;
                }
            }

            return base.VisitAssignmentExpression(assignmentExpression, data);
        }

        public override object VisitMemberReferenceExpression (MemberReferenceExpression memberReferenceExpression, object data) {
            if (memberReferenceExpression.Target is BaseReferenceExpression) {
                var pd = memberReferenceExpression.Annotation<PropertyDefinition>();

                if (pd != null) {
                    memberReferenceExpression.ReplaceWith(new InvocationExpression {
                        Target = new MemberReferenceExpression {
                            Target = memberReferenceExpression.Target.Clone(),
                            MemberName = String.Format("get_{0}", pd.Name)
                        },
                    });

                    return null;
                }
            } 
            
            return base.VisitMemberReferenceExpression(memberReferenceExpression, data);
        }
    }
}
