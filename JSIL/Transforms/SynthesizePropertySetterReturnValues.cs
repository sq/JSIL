using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    class SynthesizePropertySetterReturnValues : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;
        public readonly IFunctionSource FunctionSource;

        private JSFunctionExpression Function;

        public SynthesizePropertySetterReturnValues (
            TypeSystem typeSystem, ITypeInfoSource typeInfo,
            IFunctionSource functionSource
        ) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            FunctionSource = functionSource;
        }

        public void VisitNode (JSFunctionExpression function) {
            Function = function;

            VisitChildren(function);
        }

        private JSExpression Hoist (JSExpression expression, TypeReference type, List<JSExpression> commaElements) {
            if (expression is JSVariable)
                return null;
            else if (expression is JSLiteral)
                return null;

            var thisBoe = expression as JSBinaryOperatorExpression;
            if ((thisBoe != null) &&
                (thisBoe.Operator == JSOperator.Assignment) &&
                (thisBoe.Left is JSVariable)
            ) {
                // If the value is (x = y), insert 'x = y' and then set the value to 'x'
                commaElements.Add(thisBoe);
                return thisBoe.Left;
            } else {
                var tempVar = TemporaryVariable.ForFunction(Function, type, FunctionSource);

                commaElements.Add(new JSBinaryOperatorExpression(
                    JSOperator.Assignment, tempVar, expression, type
                ));

                return tempVar;
            }
        }

        public void VisitNode (JSPropertySetterInvocation psi) {
            if (ParentNode is JSExpressionStatement) {
                VisitChildren(psi);
                return;
            }

            var thisReference = psi.Invocation.ThisReference;
            var valueType = psi.GetActualType(TypeSystem);
            var thisType = thisReference.GetActualType(TypeSystem);

            JSExpression tempThis;
            JSExpression[] tempArguments = new JSExpression[psi.Invocation.Arguments.Count];
            var commaElements = new List<JSExpression>();

            tempThis = Hoist(thisReference, thisType, commaElements);

            for (var i = 0; i < tempArguments.Length; i++) {
                var arg = psi.Invocation.Arguments[i];
                var argType = arg.GetActualType(TypeSystem);

                tempArguments[i] = Hoist(arg, argType, commaElements);
            }

            var resultInvocation = psi.Invocation;

            if ((tempThis != null) || tempArguments.Any(ta => ta != null)) {
                resultInvocation = psi.Invocation.FilterArguments(
                    (i, a) => {
                        if ((i >= 0) && (tempArguments[i] != null))
                            return tempArguments[i];
                        else if ((i == -1) && (tempThis != null))
                            return tempThis;
                        else
                            return a;
                    }
                );
            }

            commaElements.Add(resultInvocation);

            if (tempArguments.Last() != null)
                commaElements.Add(tempArguments.Last());
            else
                commaElements.Add(psi.Value);

            var replacement = new JSCommaExpression(commaElements.ToArray());

            ParentNode.ReplaceChild(psi, replacement);
            VisitReplacement(replacement);
        }
    }
}
