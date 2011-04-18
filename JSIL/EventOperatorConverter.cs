using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace JSIL {
    public class EventOperatorConverter : ContextTrackingVisitor<object> {
        public EventOperatorConverter (DecompilerContext context)
            : base(context) {
        }

        public override object VisitExpressionStatement (ExpressionStatement expressionStatement, object data) {
            var ae = expressionStatement.Expression as AssignmentExpression;

            if (ae == null)
                return base.VisitExpressionStatement(expressionStatement, data);

            var leftEvent = ae.Left.Annotation<EventDefinition>();
            var mre = ae.Left as MemberReferenceExpression;
            var op = ae.Operator;

            if ((leftEvent != null) && (mre != null) && (
                (op == AssignmentOperatorType.Add) || (op == AssignmentOperatorType.Subtract)
            )) {
                string prefix;
                if (op == AssignmentOperatorType.Add)
                    prefix = "add_";
                else if (op == AssignmentOperatorType.Subtract)
                    prefix = "remove_";
                else
                    throw new NotImplementedException();

                var replacement = new ExpressionStatement(new InvocationExpression {
                    Target = new MemberReferenceExpression {
                        Target = mre.Target.Clone(),
                        MemberName = prefix + mre.MemberName
                    },
                    Arguments = {
                        ae.Right.Clone()
                    }
                });

                expressionStatement.ReplaceWith(replacement);

                return base.VisitExpressionStatement(replacement, data);
            }

            return base.VisitExpressionStatement(expressionStatement, data);
        }
    }
}
