using System;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms
{
    public class EmulateInt64 : JSAstVisitor
    {
        private readonly TypeSystem TypeSystem;

        public readonly JSAstBuilder googMathLong;
        public readonly JSExpression fromString;
        public readonly JSExpression fromNumber;
        public readonly JSExpression fromInt;

        public EmulateInt64(TypeSystem typeSystem)
        {
            TypeSystem = typeSystem;
            googMathLong = JSAstBuilder.StringIdentifier("goog").Dot("math").Dot("Long");
            fromString = googMathLong.FakeMethod("fromString", TypeSystem.Int64, TypeSystem.String).GetExpression();
            fromNumber = googMathLong.FakeMethod("fromNumber", TypeSystem.Int64, TypeSystem.Double).GetExpression();
            fromInt = googMathLong.FakeMethod("fromInt", TypeSystem.Int64, TypeSystem.Double).GetExpression();
        }

        public void VisitNode(JSIntegerLiteral literal)
        {
            if (literal.GetExpectedType(TypeSystem) == TypeSystem.Int64)
            {
                JSExpression expression;

                if (literal.Value >= int.MaxValue / 2 ||
                    literal.Value <= int.MinValue / 2)
                {
                    // TODO: use constructor instead of fromString

                    expression = JSInvocationExpression.InvokeStatic(fromString,
                        new[] { new JSStringLiteral(literal.Value.ToString()) });
                }
                else
                {
                    expression = JSInvocationExpression.InvokeStatic(fromInt, new[] { literal });
                }

                ParentNode.ReplaceChild(literal, expression);
            }
        }

        public void VisitNode(JSBinaryOperatorExpression boe)
        {
            var leftType = boe.Left.GetExpectedType(TypeSystem);
            var rightType = boe.Right.GetExpectedType(TypeSystem);

            TypeReference type;

            try
            {
                // GetExpectedType can throw NoExpectedTypeException
                // Shouldn't it return null or something like a NoType instead?
                type = boe.GetExpectedType(TypeSystem);
            }
            catch (NoExpectedTypeException)
            {
                type = null;
            }

            if (type == TypeSystem.Int64 &&
                leftType != TypeSystem.Int64 && rightType != TypeSystem.Int64)
            {
                var replacement = JSInvocationExpression.InvokeStatic(fromNumber, new[] { boe });

                ParentNode.ReplaceChild(boe, replacement);
                VisitChildren(boe);
                return;
            }

            if ((leftType == TypeSystem.Int64 || rightType == TypeSystem.Int64)
                && leftType.IsPrimitive && rightType.IsPrimitive)
            {
                var verb = GetVerb(boe.Operator);
                if (verb != null)
                {
                    var left = GetExpression(boe.Left);
                    var right = GetExpression(boe.Right);

                    var invoke = JSInvocationExpression
                        .InvokeMethod(
                            TypeSystem.Int64,
                            new JSFakeMethod(verb, TypeSystem.Int64, TypeSystem.Int64, TypeSystem.Int64),
                            left, new[] { right });

                    if (type == TypeSystem.Int32)
                    {
                        invoke = JSInvocationExpression
                            .InvokeMethod(
                                TypeSystem.Int64,
                                new JSFakeMethod("toInt", TypeSystem.Int32),
                                invoke);
                    }

                    ParentNode.ReplaceChild(boe, invoke);
                    VisitReplacement(invoke);
                    return;
                }
            }

            VisitChildren(boe);
        }

        private JSExpression GetExpression(JSExpression expression)
        {
            var type = expression.GetExpectedType(TypeSystem);

            if (type == TypeSystem.Int64)
            {
                return expression;
            }

            if (type == TypeSystem.Int16 ||
                type == TypeSystem.Int32 ||
                type == TypeSystem.UInt16 ||
                type == TypeSystem.UInt32 ||
                type == TypeSystem.Char)
            {
                return JSInvocationExpression
                    .InvokeStatic(
                        googMathLong.FakeMethod("fromInt", TypeSystem.Int64, type).GetExpression(),
                        new[] { expression });
            }

            if (type == TypeSystem.UInt64 || type == TypeSystem.Double || type == TypeSystem.Single)
            {
                return JSInvocationExpression
                    .InvokeStatic(
                        googMathLong.FakeMethod("fromNumber", TypeSystem.Int64, type).GetExpression(),
                        new[] { expression });
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