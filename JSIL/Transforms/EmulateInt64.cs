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
        
        private readonly JSType uint64;

        public EmulateInt64(MethodTypeFactory methodTypeFactory, TypeSystem typeSystem)
        {
            TypeSystem = typeSystem;
            MethodTypeFactory = methodTypeFactory;

            int64 = new JSType(TypeSystem.Int64);

            uint64 = new JSType(TypeSystem.UInt64);

        }

        public JSInvocationExpression GetLongLiteralExpression(long number, bool unsigned = false) {
            var type = unsigned ? TypeSystem.UInt64 : TypeSystem.Int64;
            uint a = (uint)(number & 0xffffff);
            uint b = (uint)((number >> 24) & 0xffffff);
            uint c = (uint)((number >> 48) & 0xffff);
            return JSInvocationExpression
                .InvokeStatic(
                    new JSType(type),
                    new JSFakeMethod("Create", type, new[] { TypeSystem.UInt32, TypeSystem.UInt32, TypeSystem.UInt32 }, MethodTypeFactory),
                    new JSExpression[]{ 
                        new JSIntegerLiteral((long)a, typeof(uint)),
                        new JSIntegerLiteral((long)b, typeof(uint)),
                        new JSIntegerLiteral((long)c, typeof(uint))
                    });
        }

        public void VisitNode (JSDefaultValueLiteral dvl) {
            var literalType = dvl.GetActualType(TypeSystem);
            if (IsLongOrULong(literalType)) {
                JSNode replacement;

                if (literalType.MetadataType == MetadataType.Int64)
                    replacement = GetLongLiteralExpression(0);
                else
                    replacement = GetLongLiteralExpression(0, unsigned: true);

                ParentNode.ReplaceChild(dvl, replacement);
                VisitReplacement(replacement);
                return;
            }

            VisitChildren(dvl);
        }
        
        public void VisitNode(JSIntegerLiteral literal) {
            var literalType = literal.GetActualType(TypeSystem);
            if (IsLongOrULong(literalType)) {
                JSNode replacement;

                if (literalType.MetadataType == MetadataType.Int64)
                    replacement = GetLongLiteralExpression(literal.Value);
                else
                    replacement = GetLongLiteralExpression(literal.Value, unsigned: true);

                ParentNode.ReplaceChild(literal, replacement);
                VisitReplacement(replacement);
                return;
            }

            VisitChildren(literal);
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
                    var replacement = JSInvocationExpression.InvokeStatic(exType, method, new[] { uoe.Expression }, true);
                    ParentNode.ReplaceChild(uoe, replacement);
                    VisitReplacement(replacement);
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

            var isLongExpression = IsLongOrULong(leftType) || IsLongOrULong(rightType);

            bool isUnsigned = (leftType.MetadataType == MetadataType.UInt64) || (rightType.MetadataType == MetadataType.UInt64);
            var resultType = isUnsigned ? TypeSystem.UInt64 : TypeSystem.Int64;

            var assignmentOperator = boe.Operator as JSAssignmentOperator;
            if ((assignmentOperator != null) && (isLongExpression)) {
                if (assignmentOperator == JSOperator.Assignment) {
                    VisitChildren(boe);
                    return;
                }

                // Deconstruct the mutation assignment so we can insert the appropriate operator call.
                var replacement = IntroduceEnumCasts.DeconstructMutationAssignment(boe, TypeSystem, resultType);
                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            } else if (isLongExpression) {
                var verb = GetVerb(boe.Operator);

                if (verb == null) {
                    throw new NotImplementedException("Int64 operator not yet supported: " + boe.Operator.Token);
                }

                JSIdentifier method = new JSFakeMethod(
                    verb, resultType, 
                    new[] { leftType, rightType }, MethodTypeFactory
                );

                var replacement = JSInvocationExpression
                    .InvokeStatic(leftType, method, new[] { boe.Left, boe.Right }, true);

                ParentNode.ReplaceChild(boe, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(boe);
            }
        }

        private bool IsLongOrULong(TypeReference type) {
            var result = (type.MetadataType == MetadataType.Int64) || 
                (type.MetadataType == MetadataType.UInt64);

            return result;
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