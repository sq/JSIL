using System;
using JSIL.Ast;
using Mono.Cecil;
using System.Diagnostics;
using JSIL.Internal;

namespace JSIL.Transforms
{
    public class EmulateInt64 : JSAstVisitor
    {
        public const bool Tracing = true;

        private readonly TypeSystem TypeSystem;
        private readonly MethodTypeFactory MethodTypeFactory;

        private readonly JSType int64;
        private readonly JSExpression int64Create;
        private readonly JSDotExpression int64FromNumber;
        private readonly JSDotExpression int64FromInt;

        public EmulateInt64(MethodTypeFactory methodTypeFactory, TypeSystem typeSystem)
        {
            TypeSystem = typeSystem;
            MethodTypeFactory = methodTypeFactory;

            int64 = new JSType(TypeSystem.Int64);
            
            int64Create = new JSDotExpression(int64, 
                new JSFakeMethod("Create", TypeSystem.Int64,
                    new[] { TypeSystem.UInt32, TypeSystem.UInt32, TypeSystem.UInt32 }, MethodTypeFactory));
            
            int64FromNumber = new JSDotExpression(int64,
                new JSFakeMethod("FromNumber", TypeSystem.Int64,
                    new[] { TypeSystem.Double }, MethodTypeFactory));

            int64FromInt = new JSDotExpression(int64,
                new JSFakeMethod("FromNumber", TypeSystem.Int64,
                    new[] { TypeSystem.Int32 }, MethodTypeFactory));
        }

        private JSInvocationExpression GetLiteral(long number)
        {
            uint a = (uint)(number & 0xffffff);
            uint b = (uint)((number >> 24) & 0xffffff);
            uint c = (uint)((number >> 48) & 0xffff);
            return JSInvocationExpression
                .InvokeStatic(int64Create,
                    new JSExpression[]{ 
                        new JSIntegerLiteral((long)a, typeof(uint)),
                        new JSIntegerLiteral((long)b, typeof(uint)),
                        new JSIntegerLiteral((long)c, typeof(uint))
                    });
        }

        public void VisitNode(JSIntegerLiteral literal)
        {
            if (literal.GetActualType(TypeSystem) == TypeSystem.Int64)
            {
                JSExpression expression;

                expression = GetLiteral(literal.Value);

                ParentNode.ReplaceChild(literal, expression);
            }
        }

        public void VisitNode(JSUnaryOperatorExpression uoe)
        {
            var exType = uoe.Expression.GetActualType(TypeSystem);
            var opType = uoe.ActualType;
            if (exType == TypeSystem.Int64 && opType == TypeSystem.Int64)
            {
                string verb = null;
                switch (uoe.Operator.Token)
                {
                    case "-":
                        verb = "op_UnaryNegation";
                        break;
                    case "~":
                        verb = "op_OnesComplement";
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (verb != null)
                {
                    var method = new JSDotExpression(int64, new JSFakeMethod(verb, TypeSystem.Int64, new[] { TypeSystem.Int64 }, MethodTypeFactory));
                    ParentNode.ReplaceChild(uoe,
                        JSInvocationExpression.InvokeStatic(method, new [] { uoe.Expression }));
                    return;
                }
            }
        }

        public void VisitNode(JSBinaryOperatorExpression boe)
        {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

            var int64Type = TypeSystem.Int64;

            TypeReference expectedType;
            try
            {
                // GetExpectedType can throw NoExpectedTypeException
                // Shouldn't it return null or something like a NoType instead?
                expectedType = boe.GetActualType(TypeSystem);
            }
            catch (NoExpectedTypeException)
            {
                expectedType = null;
            }

            if (boe.Operator.Token == "=" || (leftType != int64Type && rightType != int64Type && expectedType != int64Type))
            {
                // we have nothing to do

                VisitChildren(boe);

                return; // just to make sure
            }
            else if (leftType != int64Type && rightType != int64Type)
            {
                // expectedType is int64, but not the operands -> convert 

                var replacement = JSInvocationExpression.InvokeStatic(int64FromNumber, new[] { boe });
                ParentNode.ReplaceChild(boe, replacement);
                VisitChildren(boe);
            }
            else if (leftType == int64Type || rightType == int64Type)
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
                    method = new JSFakeMethod(verb, TypeSystem.Boolean, new [] { TypeSystem.Int64, TypeSystem.Int64 }, MethodTypeFactory);
                else
                    method = new JSFakeMethod(verb, TypeSystem.Int64, new[] { TypeSystem.Int64, TypeSystem.Int64 }, MethodTypeFactory);

                var left = GetExpression(boe.Left);
                var right = GetExpression(boe.Right);

                var replacement = JSInvocationExpression
                    .InvokeStatic(int64, method, new[] { left, right }, true);

                // TODO: investigate why this is still needed
                if (IsLesserIntegral(expectedType))
                {
                    throw new NotImplementedException();
                    //replacement = JSInvocationExpression
                    //    .InvokeMethod(TypeSystem.Int64, new JSFakeMethod("toInt", expectedType, new TypeReference[] {}, MethodTypeFactory), replacement);
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
            var type = expression.GetActualType(TypeSystem);

            if (type == TypeSystem.Int64)
            {
                return expression;
            }

            JSExpression conversionMethod = null;

            if (IsLesserIntegral(type))
            {
                conversionMethod = int64FromInt;
            }
            else if (type == TypeSystem.UInt64 || type == TypeSystem.Double || type == TypeSystem.Single)
            {
                conversionMethod = int64FromNumber;
            }
            else
            {
                // TODO: handle cases like these, for example pass-by-ref

                if (Tracing)
                    Debug.WriteLine("Operand type not supported in Int64 emulation: {0}", expression);

                return expression;
            }

            return JSInvocationExpression
                .InvokeStatic(conversionMethod, new[] { expression }, true);
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
                case "+": return "op_Addition";
                case "-": return "op_Subtraction";
                case "/": return "op_Division";
                case "*": return "op_Multiplication";
                case "%": return "op_Modulus";
                case "|": return "op_BitwiseOr";
                case "^": return "op_ExclusiveOr";
                case "&": return "op_BitwiseAnd";
                case "<<": return "op_LeftShift";
                case ">>":
                case ">>>":
                    return "op_RightShift";

                case "===": return "op_Equality";
                case "!==": return "op_Inequality";
                case "<": return "op_LessThan";
                case "<=": return "op_LessThanOrEqual";
                case ">": return "op_GreaterThan";
                case ">=": return "op_GreaterThanOrEqual";

                default:
                    return null;
            }
        }
    }
}