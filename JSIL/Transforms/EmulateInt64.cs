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
        
        private readonly JSType uint64;
        private readonly JSDotExpression uint64Create;

        public EmulateInt64(MethodTypeFactory methodTypeFactory, TypeSystem typeSystem)
        {
            TypeSystem = typeSystem;
            MethodTypeFactory = methodTypeFactory;

            int64 = new JSType(TypeSystem.Int64);

            uint64 = new JSType(TypeSystem.UInt64);

            int64Create = new JSDotExpression(int64,
                new JSFakeMethod("Create", TypeSystem.Int64,
                    new[] { TypeSystem.UInt32, TypeSystem.UInt32, TypeSystem.UInt32 }, MethodTypeFactory));

            uint64Create = new JSDotExpression(uint64,
                new JSFakeMethod("Create", TypeSystem.UInt64,
                    new[] { TypeSystem.UInt32, TypeSystem.UInt32, TypeSystem.UInt32 }, MethodTypeFactory));
        }

        private JSInvocationExpression GetLiteral(long number, bool unsigned = false)
        {
            uint a = (uint)(number & 0xffffff);
            uint b = (uint)((number >> 24) & 0xffffff);
            uint c = (uint)((number >> 48) & 0xffff);
            return JSInvocationExpression
                .InvokeStatic(unsigned ? uint64Create : int64Create,
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
                ParentNode.ReplaceChild(literal, GetLiteral(literal.Value));
            }

            if (literal.GetActualType(TypeSystem) == TypeSystem.UInt64)
            {
                ParentNode.ReplaceChild(literal, GetLiteral(literal.Value, unsigned: true));
            }
        }

        public void VisitNode(JSUnaryOperatorExpression uoe)
        {
            var exType = uoe.Expression.GetActualType(TypeSystem);
            var opType = uoe.ActualType;
            if (IsLongOrULong(exType) && IsLongOrULong(opType)) //exType == TypeSystem.Int64 && opType == TypeSystem.Int64)
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
                    var type = exType == TypeSystem.Int64 ? int64 : uint64;
                    var method = new JSFakeMethod(verb, TypeSystem.Int64, new[] { TypeSystem.Int64 }, MethodTypeFactory);
                    ParentNode.ReplaceChild(uoe,
                        JSInvocationExpression.InvokeStatic(exType, method, new [] { uoe.Expression }, true));
                    return;
                }
            }

            VisitChildren(uoe);
        }

        public void VisitNode(JSBinaryOperatorExpression boe)
        {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

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

            if (boe.Operator.Token != "=" && (IsLongOrULong(leftType) || IsLongOrULong(rightType)))
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

                JSIdentifier method = new JSFakeMethod(verb, expectedType, new[] { leftType, rightType }, MethodTypeFactory);

                var replacement = JSInvocationExpression
                    .InvokeStatic(leftType, method, new[] { boe.Left, boe.Right }, true);

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            }
            else
            {
                VisitChildren(boe);
            }
        }

        private bool IsLongOrULong(TypeReference type)
        {
            return type == TypeSystem.Int64 || type == TypeSystem.UInt64;
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