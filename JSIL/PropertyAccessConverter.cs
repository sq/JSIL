using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace JSIL {
    public class PropertyAccessConverter : ContextTrackingVisitor<object> {
        public PropertyAccessConverter (DecompilerContext context)
            : base(context) {
        }

        public override object VisitIndexerExpression (IndexerExpression indexerExpression, object data) {
            var propertyDefinition = indexerExpression.Annotation<PropertyDefinition>();

            if (propertyDefinition != null) {
                var target = new MemberReferenceExpression {
                    Target = indexerExpression.Target.Clone(),
                    MemberName = String.Format("get_{0}", propertyDefinition.Name)
                };
                target.AddAnnotation(propertyDefinition);

                var replacement = new InvocationExpression {
                    Target = target
                };

                foreach (var arg in indexerExpression.Arguments)
                    replacement.Arguments.Add(arg.Clone());

                indexerExpression.ReplaceWith(replacement);

                return null;
            }

            return base.VisitIndexerExpression(indexerExpression, data);
        }

        public override object VisitAssignmentExpression (AssignmentExpression assignmentExpression, object data) {
            var mre = assignmentExpression.Left as MemberReferenceExpression;
            var ie = assignmentExpression.Left as IndexerExpression;

            if ((mre != null) && (mre.Target is BaseReferenceExpression)) {
                var pd = mre.Annotation<PropertyDefinition>();

                if (pd != null) {
                    var target = new MemberReferenceExpression {
                        Target = mre.Target.Clone(),
                        MemberName = String.Format("set_{0}", pd.Name)
                    };
                    target.AddAnnotation(pd);

                    assignmentExpression.ReplaceWith(new InvocationExpression {
                        Target = target,
                        Arguments = {
                            assignmentExpression.Right.Clone()
                        }
                    });

                    return null;
                }
            } else if (ie != null) {
                var pd = ie.Annotation<PropertyDefinition>();

                if (pd != null) {
                    var target = new MemberReferenceExpression {
                        Target = ie.Target.Clone(),
                        MemberName = String.Format("set_{0}", pd.Name)
                    };
                    target.AddAnnotation(pd);

                    var replacement = new InvocationExpression {
                        Target = target
                    };

                    foreach (var arg in ie.Arguments)
                        replacement.Arguments.Add(arg.Clone());

                    replacement.Arguments.Add(assignmentExpression.Right.Clone());

                    assignmentExpression.ReplaceWith(replacement);

                    return null;
                }
            }

            return base.VisitAssignmentExpression(assignmentExpression, data);
        }

        public override object VisitMemberReferenceExpression (MemberReferenceExpression memberReferenceExpression, object data) {
            if (memberReferenceExpression.Target is BaseReferenceExpression) {
                var pd = memberReferenceExpression.Annotation<PropertyDefinition>();

                if (pd != null) {
                    var target = new MemberReferenceExpression {
                        Target = memberReferenceExpression.Target.Clone(),
                        MemberName = String.Format("get_{0}", pd.Name)
                    };
                    target.AddAnnotation(pd);

                    memberReferenceExpression.ReplaceWith(new InvocationExpression {
                        Target = target
                    });

                    return null;
                }
            } 
            
            return base.VisitMemberReferenceExpression(memberReferenceExpression, data);
        }
    }
}
