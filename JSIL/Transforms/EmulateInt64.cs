using System;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms
{
    public class EmulateInt64: JSAstVisitor
    {
        public readonly TypeSystem TypeSystem;

        public EmulateInt64(TypeSystem typeSystem)
        {
            TypeSystem = typeSystem;
        }

        public void VisitNode(JSIntegerLiteral literal)
        {
            if (literal.GetExpectedType(TypeSystem) == TypeSystem.Int64)
            {
                var newLong = JSInvocationExpression
                    .InvokeStatic(
                        new JSFakeMethod("goog.math.Long.fromString", TypeSystem.Int64, TypeSystem.Int64),
                        new[] { new JSStringLiteral(literal.Value.ToString()) });
                
                ParentNode.ReplaceChild(literal, newLong);
                VisitReplacement(newLong);
            }
        }

        public void VisitNode(JSBinaryOperatorExpression boe)
        {
            if (boe.GetExpectedType(TypeSystem) == TypeSystem.Int64)
            {
                var verb = GetVerb(boe.Operator);
                if (verb != null)
                {
                    var invoke = JSInvocationExpression
                        .InvokeMethod(
                            TypeSystem.Int64,
                            new JSFakeMethod(verb, TypeSystem.Int64, TypeSystem.Int64, TypeSystem.Int64),
                            boe.Left, new[] { boe.Right });
                    
                    ParentNode.ReplaceChild(boe, invoke);
                    VisitReplacement(invoke);
                    return;
                }
            }

            VisitChildren(boe);
        }

        private string GetVerb(JSBinaryOperator op)
        {
            switch (op.Token)
            {
                case "+": return "add";
                case "-": return "subtract";
                case "/": return "div";
                case "*": return "multiply";
                case "%": return "modulo";
                case "|": return "or";
                case "&": return "and";
                case "<<": return "shiftLeft";
                case ">>": return "shiftRight";
                default:
                    return null;
            }
        }
    }
}
