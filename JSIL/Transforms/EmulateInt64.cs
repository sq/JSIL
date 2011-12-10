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
                JSExpression expression;

                if (Math.Abs(literal.Value) >= int.MaxValue / 2)
                {
                    // TODO: use constructor instead of fromString

                    expression = JSInvocationExpression
                        .InvokeStatic(
                            new JSFakeMethod("goog.math.Long.fromString", TypeSystem.Int64, TypeSystem.Int64),
                            new[] { new JSStringLiteral(literal.Value.ToString()) });
                }
                else
                {
                    expression = JSInvocationExpression
                        .InvokeStatic(
                            new JSFakeMethod("goog.math.Long.fromInt", TypeSystem.Int64, TypeSystem.Int64),
                            new[] { literal });
                }

                ParentNode.ReplaceChild(literal, expression);
            }
        }

        public void VisitNode(JSBinaryOperatorExpression boe)
        {
            // TODO: shouldn't really depend on the expected type of the expression.. right?
            if (boe.GetExpectedType(TypeSystem) == TypeSystem.Int64)
            {
                var verb = GetVerb(boe.Operator);
                if (verb != null)
                {
                    var left = GetLeft(boe.Left);

                    var invoke = JSInvocationExpression
                        .InvokeMethod(
                            TypeSystem.Int64,
                            new JSFakeMethod(verb, TypeSystem.Int64, TypeSystem.Int64, TypeSystem.Int64),
                            left, new[] { boe.Right });
                    
                    ParentNode.ReplaceChild(boe, invoke);
                    VisitReplacement(invoke);
                    return;
                }
            }

            VisitChildren(boe);
        }

        private JSExpression GetLeft(JSExpression expression)
        {
            var type = expression.GetExpectedType(TypeSystem);

            if (type == TypeSystem.Int64)
            {
                return expression;
            }

            if (type == TypeSystem.Int16 || type == TypeSystem.Int32 )
            {
                return JSInvocationExpression
                    .InvokeStatic(
                        new JSFakeMethod("goog.math.Long.fromInt", TypeSystem.Int64, type),
                        new [] { expression });
            }

            throw new NotImplementedException();
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
