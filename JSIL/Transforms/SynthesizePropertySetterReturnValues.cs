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

        private JSFunctionExpression Function;

        public SynthesizePropertySetterReturnValues (TypeSystem typeSystem, ITypeInfoSource typeInfo) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
        }

        public void VisitNode (JSFunctionExpression function) {
            Function = function;

            VisitChildren(function);
        }

        public void VisitNode (JSPropertySetterInvocation psi) {
            if (ParentNode is JSExpressionStatement) {
                VisitChildren(psi);
                return;
            }

            var valueType = psi.GetActualType(TypeSystem);
            var tempVariable = new JSRawOutputIdentifier(
                valueType,
                "$temp{0:X2}", Function.TemporaryVariableCount++
            );

            var value = psi.Value;
            var filteredInvocation = psi.Invocation.FilterArguments(
                (i, v) => 
                    (v == value)
                        ? tempVariable
                        : v
            );

            var replacement = new JSCommaExpression(
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, tempVariable, value, valueType
                ),
                filteredInvocation,
                tempVariable
            );

            ParentNode.ReplaceChild(psi, replacement);
            VisitReplacement(replacement);
        }
    }
}
