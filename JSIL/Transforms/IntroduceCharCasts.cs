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
    public class IntroduceCharCasts : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;

        public IntroduceCharCasts (TypeSystem typeSystem, JSSpecialIdentifiers js) {
            TypeSystem = typeSystem;
            JS = js;
        }

        protected JSInvocationExpression CastToChar (JSExpression integerExpression) {
            return JSInvocationExpression.InvokeStatic(
                JS.fromCharCode, new[] { integerExpression }, true
            );
        }

        protected JSInvocationExpression CastToInteger (JSExpression charExpression) {
            return JSInvocationExpression.InvokeMethod(
                JS.charCodeAt, charExpression, new[] { JSLiteral.New(0) }, true
            );
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var leftType = boe.Left.GetActualType(TypeSystem);
            var rightType = boe.Right.GetActualType(TypeSystem);

            bool isArithmetic = !(boe.Operator is JSAssignmentOperator);

            if ((leftType.FullName == "System.Char") && isArithmetic)
                boe.ReplaceChild(boe.Left, CastToInteger(boe.Left));

            if ((rightType.FullName == "System.Char") && isArithmetic)
                boe.ReplaceChild(boe.Right, CastToInteger(boe.Right));

            var parentInvocation = ParentNode as JSInvocationExpression;
            JSDotExpressionBase parentInvocationDot = (parentInvocation != null) ? parentInvocation.Method as JSDotExpressionBase : null;

            if (
                isArithmetic && 
                (boe.GetActualType(TypeSystem).FullName == "System.Char") &&
                !(
                    (parentInvocation != null) && 
                    (parentInvocationDot != null) &&
                    (parentInvocationDot.Target is JSStringIdentifier) &&
                    (((JSStringIdentifier)parentInvocationDot.Target).Identifier == "String") &&
                    (parentInvocationDot.Member is JSFakeMethod) &&
                    (((JSFakeMethod)parentInvocationDot.Member).Name == "fromCharCode")
                )
            ) {
                var castBoe = CastToChar(boe);
                ParentNode.ReplaceChild(boe, castBoe);

                VisitReplacement(castBoe);
            } else {    
                VisitChildren(boe);
            }
        }
    }
}
