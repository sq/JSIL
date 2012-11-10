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
    public class FoldStructConstructorInvocations : JSAstVisitor {
        private class InitializationInfo {
            public TypeReference Type;
            public JSNewExpression NewExpression;
            public JSDefaultValueLiteral DefaultValueLiteral;
            public JSBinaryOperatorExpression ParentBinaryExpression;
            public JSNode BinaryExpressionParent;
        }

        private readonly List<InitializationInfo> Initializations = new List<InitializationInfo>();
        public readonly TypeSystem TypeSystem;

        public FoldStructConstructorInvocations (
            TypeSystem typeSystem
        ) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSDefaultValueLiteral dvl) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;

            if (CurrentName != "DefaultValue") {
                var thisReferenceType = dvl.GetActualType(TypeSystem);
                if (TypeUtil.IsStruct(thisReferenceType) && (parentBoe != null))
                    Initializations.Add(new InitializationInfo {
                        Type = thisReferenceType,
                        DefaultValueLiteral = dvl,
                        ParentBinaryExpression = parentBoe,
                        BinaryExpressionParent = Stack.Skip(2).First()
                    });
            }

            VisitChildren(dvl);
        }

        public void VisitNode (JSNewExpression ne) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;

            var thisReferenceType = ne.GetActualType(TypeSystem);
            if (TypeUtil.IsStruct(thisReferenceType) && (parentBoe != null))
                Initializations.Add(new InitializationInfo {
                    Type = thisReferenceType,
                    NewExpression = ne,
                    ParentBinaryExpression = parentBoe,
                    BinaryExpressionParent = Stack.Skip(2).First()
                });

            VisitChildren(ne);
        }

        public void VisitNode (JSExpressionStatement es) {
            var invocationExpression = es.Expression as JSInvocationExpression;
            if (invocationExpression != null) {
                var replacement = MaybeReplaceInvocation(invocationExpression);
                if (replacement != null) {
                    ParentNode.ReplaceChild(es, replacement);
                    VisitReplacement(replacement);
                    return;
                }
            }

            VisitChildren(es);
        }

        public JSStatement MaybeReplaceInvocation (JSInvocationExpression invocation) {
            var jsm = invocation.JSMethod;
            if (
                (jsm != null) && 
                (jsm.Method.Name == ".ctor") && 
                TypeUtil.IsStruct(jsm.Method.DeclaringType.Definition) &&
                !Stack.OfType<JSStatement>().Any((n) => n.IsControlFlow)
            ) {
                var previousInitialization = Initializations.LastOrDefault(
                    (ne) => 
                        ne.ParentBinaryExpression.Left.Equals(invocation.ThisReference)
                );

                if (previousInitialization != null)
                    return FoldInvocation(invocation, previousInitialization);
            }

            return null;
        }

        private JSStatement FoldInvocation (JSInvocationExpression invocation, InitializationInfo ii) {
            var arguments = invocation.Arguments.ToArray();
            var newExpression = new JSNewExpression(
                ii.Type, invocation.JSMethod.Reference, invocation.JSMethod.Method, arguments
            );

            ii.BinaryExpressionParent.ReplaceChild(ii.ParentBinaryExpression, new JSNullExpression());
            Initializations.Remove(ii);

            var newBoe = new JSBinaryOperatorExpression(
                JSOperator.Assignment, 
                ii.ParentBinaryExpression.Left, newExpression, 
                ii.ParentBinaryExpression.ActualType
            );

            return new JSVariableDeclarationStatement(newBoe);
        }
    }
}
