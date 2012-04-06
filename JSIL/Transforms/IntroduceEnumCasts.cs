using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class IntroduceEnumCasts : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly TypeInfoProvider TypeInfo;
        public readonly MethodTypeFactory MethodTypes;

        private readonly HashSet<JSOperator> LogicalOperators;

        public IntroduceEnumCasts (TypeSystem typeSystem, TypeInfoProvider typeInfo, MethodTypeFactory methodTypes) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            MethodTypes = methodTypes;

            LogicalOperators = new HashSet<JSOperator>() {
                JSOperator.LogicalAnd,
                JSOperator.LogicalOr,
                JSOperator.LogicalNot
            };
        }

        public void VisitNode (JSIndexerExpression ie) {
            var indexType = ie.Index.GetExpectedType(TypeSystem);

            if (
                !ILBlockTranslator.IsIntegral(indexType) &&
                ILBlockTranslator.IsEnum(indexType)
            ) {
                var cast = JSInvocationExpression.InvokeMethod(
                    new JSFakeMethod("valueOf", TypeSystem.Int32, new[] { indexType }, MethodTypes), ie.Index, null, true
                );

                ie.ReplaceChild(ie.Index, cast);
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var type = uoe.Expression.GetExpectedType(TypeSystem);
            var isEnum = ILBlockTranslator.IsEnum(type);

            if (isEnum) {
                var cast = JSInvocationExpression.InvokeMethod(
                    new JSFakeMethod("valueOf", TypeSystem.Int32, new[] { type }, MethodTypes), uoe.Expression, null, true
                );

                if (LogicalOperators.Contains(uoe.Operator)) {
                    uoe.ReplaceChild(uoe.Expression, cast);
                } else if (uoe.Operator == JSOperator.Negation) {
                    uoe.ReplaceChild(uoe.Expression, cast);
                }
            }

            VisitChildren(uoe);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetExpectedType(TypeSystem);
            var leftIsEnum = ILBlockTranslator.IsEnum(leftType);
            var rightType = boe.Right.GetExpectedType(TypeSystem);
            var rightIsEnum = ILBlockTranslator.IsEnum(rightType);

            if ((leftIsEnum || rightIsEnum) && LogicalOperators.Contains(boe.Operator)) {
                if (leftIsEnum) {
                    var cast = JSInvocationExpression.InvokeMethod(
                        new JSFakeMethod("valueOf", TypeSystem.Int32, new[] { leftType }, MethodTypes), boe.Left, null, true
                    );

                    boe.ReplaceChild(boe.Left, cast);
                }

                if (rightIsEnum) {
                    var cast = JSInvocationExpression.InvokeMethod(
                        new JSFakeMethod("valueOf", TypeSystem.Int32, new[] { rightType }, MethodTypes), boe.Right, null, true
                    );

                    boe.ReplaceChild(boe.Right, cast);
                }
            }

            VisitChildren(boe);
        }

        public void VisitNode (JSSwitchStatement ss) {
            var conditionType = ss.Condition.GetExpectedType(TypeSystem);

            if (
                !ILBlockTranslator.IsIntegral(conditionType) &&
                ILBlockTranslator.IsEnum(conditionType)
            ) {
                var cast = JSInvocationExpression.InvokeMethod(
                    new JSFakeMethod("valueOf", TypeSystem.Int32, new[] { conditionType }, MethodTypes), ss.Condition, null, true
                );

                ss.ReplaceChild(ss.Condition, cast);
            }

            VisitChildren(ss);
        }
    }
}
