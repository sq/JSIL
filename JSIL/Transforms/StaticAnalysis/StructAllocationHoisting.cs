using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class HoistStructAllocations : StaticAnalysisJSAstVisitor {
        private struct PendingDeclaration {
            public readonly string Name;
            public readonly TypeReference Type;
            public readonly JSExpression Expression;

            public PendingDeclaration (string name, TypeReference type, JSExpression expression) {
                Name = name;
                Type = type;
                Expression = expression;
            }
        }

        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;

        private readonly List<PendingDeclaration> ToDeclare = new
            List<PendingDeclaration>();

        private FunctionAnalysis1stPass FirstPass = null;

        private JSFunctionExpression Function;

        public HoistStructAllocations (
            QualifiedMemberIdentifier member, 
            IFunctionSource functionSource, 
            TypeSystem typeSystem,
            MethodTypeFactory methodTypes
        ) 
            : base (member, functionSource) {
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;
        }

        public void VisitNode (JSFunctionExpression fn) {
            Function = fn;
            FirstPass = GetFirstPass(Function.Method.QualifiedIdentifier);

            VisitChildren(fn);

            if (ToDeclare.Count > 0) {
                int i = 0;

                foreach (var pd in ToDeclare) {
                    var es = new JSExpressionStatement(
                        new JSBinaryOperatorExpression(
                            JSOperator.Assignment, pd.Expression,
                            new JSDefaultValueLiteral(pd.Type),
                            pd.Type
                    ));

                    fn.Body.Statements.Insert(i++, es);
                }
            }
        }

        private JSRawOutputIdentifier MakeTemporaryVariable (TypeReference type, out string id) {
            string _id = id = String.Format("$temp{0:X2}", Function.TemporaryVariableCount++);
            return new JSRawOutputIdentifier(
                (jsf) => jsf.WriteRaw(_id), type
            );
        }

        public void VisitNode (JSNewExpression newexp) {
            var type = newexp.GetActualType(TypeSystem);

            var isStruct = TypeUtil.IsStruct(type);
            var isInsideLoop = (Stack.Any((node) => node is JSLoopStatement));
            var parentInvocation = ParentNode as JSInvocationExpression;

            var doesValueEscape = true;
            if (isStruct && (parentInvocation != null)) {
                var methodDef = parentInvocation.JSMethod.Reference.Resolve();
                var secondPass = FunctionSource.GetSecondPass(parentInvocation.JSMethod, Function.Method.QualifiedIdentifier);
                if ((secondPass != null) && (methodDef != null)) {
                    // HACK
                    var argumentIndex = parentInvocation.Arguments.Select(
                        (a, i) => new { argument = a, index = i })
                        .FirstOrDefault((_) => _.argument.SelfAndChildrenRecursive.Contains(newexp));

                    if (argumentIndex != null) {
                        var argumentName = methodDef.Parameters[argumentIndex.index].Name;

                        doesValueEscape = secondPass.EscapingVariables.Contains(argumentName);
                    }
                }
            }

            if (isStruct && 
                isInsideLoop && 
                (parentInvocation != null) &&
                !doesValueEscape
            ) {
                string id;
                var hoistedVariable = MakeTemporaryVariable(type, out id);
                var constructorInvocation = JSInvocationExpression.InvokeMethod(
                    type, new JSMethod(newexp.ConstructorReference, newexp.Constructor, MethodTypes, null), hoistedVariable, newexp.Arguments.ToArray(), false
                );
                var replacement = new JSCommaExpression(
                    constructorInvocation, hoistedVariable
                );

                ToDeclare.Add(new PendingDeclaration(id, type, hoistedVariable));

                ParentNode.ReplaceChild(newexp, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(newexp);
            }
        }
    }
}
