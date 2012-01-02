using System;
using JSIL.Ast;
using Mono.Cecil;
using System.Diagnostics;

namespace JSIL.Transforms
{
    public class EmulateInt64 : JSAstVisitor
    {
        public const bool Tracing = true;

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

            var int64 = TypeSystem.Int64;

            TypeReference expectedType;
            try
            {
                // GetExpectedType can throw NoExpectedTypeException
                // Shouldn't it return null or something like a NoType instead?
                expectedType = boe.GetExpectedType(TypeSystem);
            }
            catch (NoExpectedTypeException)
            {
                expectedType = null;
            }

            if (boe.Operator.Token == "=" || (leftType != int64 && rightType != int64 && expectedType != int64))
            {
                // we have nothing to do

                VisitChildren(boe);

                return; // just to make sure
            }
            else if (leftType != int64 && rightType != int64)
            {
                // expectedType is int64, but not the operands -> convert 

                var replacement = JSInvocationExpression.InvokeStatic(fromNumber, new[] { boe });
                ParentNode.ReplaceChild(boe, replacement);
                VisitChildren(boe);
            }
            else if (leftType == int64 || rightType == int64)
            {
                var verb = GetVerb(boe.Operator);

                if (verb == null)
                {
                    if (Tracing)
                        Debug.WriteLine("Operator not yet supported: " + boe.Operator.Token);

                    // TODO: this should probably generate an error

                    VisitChildren(boe);
                    return;
                }

                JSIdentifier method;

                if (expectedType == TypeSystem.Boolean)
                    method = new JSFakeMethod(verb, TypeSystem.Boolean, TypeSystem.Int64, TypeSystem.Int64);
                else
                    method = new JSFakeMethod(verb, TypeSystem.Int64, TypeSystem.Int64, TypeSystem.Int64);

                var left = GetExpression(boe.Left);
                var right = GetExpression(boe.Right);

                var replacement = JSInvocationExpression
                    .InvokeMethod(TypeSystem.Int64, method, left, new[] { right });

                // TODO: investigate why this is still needed
                if (IsLesserIntegral(expectedType))
                {
                    replacement = JSInvocationExpression
                        .InvokeMethod(TypeSystem.Int64, new JSFakeMethod("toInt", expectedType), replacement);
                }

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            }
            else
            {
                throw new NotSupportedException("Coult not translate expression: " + boe);
            }
        }

        private JSExpression GetExpression(JSExpression expression)
        {
            var type = expression.GetExpectedType(TypeSystem);

            if (type == TypeSystem.Int64)
            {
                return expression;
            }

            JSAstBuilder conversionMethod = null;

            if (IsLesserIntegral(type))
            {
                conversionMethod = googMathLong.FakeMethod("fromInt", TypeSystem.Int64, type);
            }
            else if (type == TypeSystem.UInt64 || type == TypeSystem.Double || type == TypeSystem.Single)
            {
                conversionMethod = googMathLong.FakeMethod("fromNumber", TypeSystem.Int64, type);
            }
            else
            {
                // TODO: handle cases like these, for example pass-by-ref

                if (Tracing)
                    Debug.WriteLine("Operand type not supported in Int64 emulation: {0}", type);

                return expression;
            }

            return JSInvocationExpression
                .InvokeStatic(conversionMethod.GetExpression(), new[] { expression });
        }

        private bool IsLesserIntegral(TypeReference type)
        {
            return
                type == TypeSystem.Int16 ||
                type == TypeSystem.Int32 ||
                type == TypeSystem.UInt16 ||
                type == TypeSystem.UInt32 ||
                type == TypeSystem.Char;
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
                case ">>":
                case ">>>":
                    return "shiftRight";

                case "===": return "equals";
                case "!==": return "notEquals";
                case "<": return "lessThan";
                case "<=": return "lessThanOrEqual";
                case ">": return "greaterThan";
                case ">=": return "greaterThanOrEqual";

                default:
                    return null;
            }
        }
    }
}