using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EliminatePointlessFinallyBlocks : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;
        public readonly IFunctionSource FunctionSource;

        public EliminatePointlessFinallyBlocks (TypeSystem typeSystem, ITypeInfoSource typeInfo, IFunctionSource functionSource) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            FunctionSource = functionSource;
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Create a new visitor for nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null) {
                var nested = new EliminatePointlessFinallyBlocks(TypeSystem, TypeInfo, FunctionSource);
                nested.Visit(fn);

                return;
            }

            VisitChildren(fn);
        }

        protected bool IsEffectivelyConstant (JSExpression expression) {
            if (expression.IsConstant)
                return true;

            var invocation = expression as JSInvocationExpression;
            FunctionAnalysis2ndPass secondPass = null;
            if ((invocation != null) && (invocation.JSMethod != null)) {
                secondPass = FunctionSource.GetSecondPass(invocation.JSMethod);

                if ((secondPass != null) && secondPass.IsPure)
                    return true;

                var methodName = invocation.JSMethod.Method.Name;
                if ((methodName == "IDisposable.Dispose") || (methodName == "Dispose")) {
                    var thisType = invocation.ThisReference.GetActualType(TypeSystem);

                    if (thisType != null) {
                        var typeInfo = TypeInfo.GetExisting(thisType);

                        if ((typeInfo != null) && typeInfo.Metadata.HasAttribute("JSIL.Meta.JSPureDispose"))
                            return true;
                    }
                }
            }

            return false;
        }

        public void VisitNode (JSTryCatchBlock tcb) {
            if ((tcb.Finally != null) && (tcb.Catch == null)) {
                do {
                    if (!tcb.Finally.Children.All((n) => n is JSExpressionStatement))
                        break;

                    var statements = tcb.Finally.Children.OfType<JSExpressionStatement>().ToArray();

                    if (statements.Any((es) => es.Expression.HasGlobalStateDependency))
                        break;

                    if (!statements.All((es) => IsEffectivelyConstant(es.Expression)))
                        break;

                    ParentNode.ReplaceChild(tcb, tcb.Body);
                    VisitReplacement(tcb.Body);
                    return;
                } while (false);
            }

            VisitChildren(tcb);
        }
    }
}
