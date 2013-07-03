using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class HoistAllocations : StaticAnalysisJSAstVisitor {
        private struct Identifier {
            public readonly string Text;
            public readonly JSRawOutputIdentifier Object;

            public Identifier (string text, JSRawOutputIdentifier @object) {
                Text = text;
                Object = @object;
            }
        }

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

        private readonly Dictionary<TypeReference, Identifier> TemporaryVariables = new 
            Dictionary<TypeReference, Identifier>();
        private readonly List<PendingDeclaration> ToDeclare = new
            List<PendingDeclaration>();

        private FunctionAnalysis1stPass FirstPass = null;

        private JSFunctionExpression Function;

        public HoistAllocations (
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
            Identifier result;

            if (!TemporaryVariables.TryGetValue(type, out result)) {
                string _id = id = String.Format("$temp{0:X2}", Function.TemporaryVariableCount++);
                result = new Identifier(_id, new JSRawOutputIdentifier(
                    (jsf) => jsf.WriteRaw(_id), type
                ));
                ToDeclare.Add(new PendingDeclaration(id, type, result.Object));
                TemporaryVariables.Add(type, result);
            } else {
                id = result.Text;
            }

            return result.Object;
        }

        bool DoesValueEscapeFromInvocation (JSInvocationExpression invocation, JSExpression argumentExpression) {
            if (
                (invocation != null) &&
                (invocation.JSMethod != null) &&
                (invocation.JSMethod.Reference != null)
            ) {
                var methodDef = invocation.JSMethod.Reference.Resolve();
                var secondPass = FunctionSource.GetSecondPass(invocation.JSMethod, Function.Method.QualifiedIdentifier);
                if ((secondPass != null) && (methodDef != null)) {
                    // HACK
                    var argumentIndex = invocation.Arguments.Select(
                        (a, i) => new { argument = a, index = i })
                        .FirstOrDefault((_) => _.argument.SelfAndChildrenRecursive.Contains(argumentExpression));

                    if (argumentIndex != null) {
                        var argumentName = methodDef.Parameters[argumentIndex.index].Name;

                        return secondPass.EscapingVariables.Contains(argumentName);
                    }
                } else if (secondPass != null) {
                    // HACK for methods that do not have resolvable references. In this case, if NONE of their arguments escape, we're probably still okay.
                    if (secondPass.EscapingVariables.Count == 0)
                        return false;
                }
            }

            return true;
        }

        public void VisitNode (JSNewArrayElementReference naer) {
            var isInsideLoop = (Stack.Any((node) => node is JSLoopStatement));
            var parentInvocation = ParentNode as JSInvocationExpression;
            var doesValueEscape = DoesValueEscapeFromInvocation(parentInvocation, naer);

            if (
                isInsideLoop &&
                (parentInvocation != null) &&
                !doesValueEscape
            ) {
            }

            VisitChildren(naer);
        }

        public void VisitNode (JSNewExpression newexp) {
            var type = newexp.GetActualType(TypeSystem);

            var isStruct = TypeUtil.IsStruct(type);
            var isInsideLoop = (Stack.Any((node) => node is JSLoopStatement));
            var parentInvocation = ParentNode as JSInvocationExpression;
            var doesValueEscape = DoesValueEscapeFromInvocation(parentInvocation, newexp);

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

                ParentNode.ReplaceChild(newexp, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(newexp);
            }
        }
    }
}
