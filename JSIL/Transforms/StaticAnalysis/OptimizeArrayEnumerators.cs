using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class OptimizeArrayEnumerators : JSAstVisitor {
        public readonly TypeSystem TypeSystem;

        private JSFunctionExpression Function;
        private int _NextLoopId = 0;

        public OptimizeArrayEnumerators (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Create a new visitor for nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null) {
                var nested = new OptimizeArrayEnumerators(TypeSystem);
                nested.Visit(fn);

                return;
            }

            Function = fn;
            VisitChildren(fn);
        }

        public void VisitNode (JSWhileLoop wl) {
            var condInvocation = wl.Condition as JSInvocationExpression;
            if (
                (condInvocation != null) && 
                (condInvocation.JSMethod != null) &&
                (condInvocation.JSMethod.Identifier == "MoveNext") &&
                (condInvocation.JSMethod.Method.DeclaringType.Interfaces.Any((ii) => ii.Item1.FullName == "System.Collections.IEnumerator"))
            ) {
                var enumeratorType = condInvocation.JSMethod.Method.DeclaringType;
                var attrParams = enumeratorType.Metadata.GetAttributeParameters("JSIL.Meta.JSIsArrayEnumerator");

                if (attrParams != null) {
                    var arrayMember = (string)attrParams[0].Value;
                    var indexMember = (string)attrParams[1].Value;
                    var lengthMember = (string)attrParams[2].Value;

                    var replacement = ReplaceWhileLoop(
                        wl, condInvocation.ThisReference, condInvocation.JSMethod.Method.DeclaringType,
                        arrayMember, indexMember, lengthMember
                    );
                    ParentNode.ReplaceChild(wl, replacement);

                    VisitReplacement(replacement);

                    return;
                }
            }

            VisitChildren(wl);
        }

        private JSForLoop ReplaceWhileLoop (JSWhileLoop wl, JSExpression enumerator, TypeInfo enumeratorType, string arrayMember, string indexMember, string lengthMember) {
            var loopId = _NextLoopId++;
            var arrayVariableName = String.Format("a${0:x}", loopId);
            var indexVariableName = String.Format("i${0:x}", loopId);
            var lengthVariableName = String.Format("l${0:x}", loopId);

            var currentPropertyReference = enumeratorType.Definition.Properties.First((p) => p.Name == "Current");
            var currentPropertyInfo = enumeratorType.Source.GetProperty(currentPropertyReference);

            var itemType = currentPropertyInfo.ReturnType;
            var arrayType = new ArrayType(itemType);

            var arrayVariable = new JSVariable(
                arrayVariableName, arrayType, Function.Method.Reference, 
                JSDotExpression.New(enumerator, new JSStringIdentifier(arrayMember, arrayType))
            );
            var indexVariable = new JSVariable(
                indexVariableName, TypeSystem.Int32, Function.Method.Reference, 
                JSDotExpression.New(enumerator, new JSStringIdentifier(indexMember, TypeSystem.Int32))
            );
            var lengthVariable = new JSVariable(
                lengthVariableName, TypeSystem.Int32, Function.Method.Reference,
                JSDotExpression.New(enumerator, new JSStringIdentifier(lengthMember, TypeSystem.Int32))
            );

            var initializer = new JSVariableDeclarationStatement(
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, arrayVariable, arrayVariable.DefaultValue, arrayVariable.Type
                ),
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, indexVariable, indexVariable.DefaultValue, indexVariable.Type
                ),
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, lengthVariable, lengthVariable.DefaultValue, lengthVariable.Type
                )
            );

            var condition = new JSBinaryOperatorExpression(
                JSBinaryOperator.LessThan, 
                new JSUnaryOperatorExpression(
                    JSUnaryOperator.PreIncrement,
                    indexVariable, TypeSystem.Int32
                ), 
                lengthVariable, TypeSystem.Boolean
            );

            var result = new JSForLoop(
                initializer, condition, new JSNullStatement(),
                wl.Statements.ToArray()
            );
            result.Index = wl.Index;

            new PropertyAccessReplacer(
                enumerator, new JSProperty(currentPropertyReference, currentPropertyInfo),
                new JSIndexerExpression(
                    arrayVariable, indexVariable, 
                    itemType
                )
            ).Visit(result);

            return result;
        }
    }

    public class PropertyAccessReplacer : JSAstVisitor {
        public readonly JSExpression ThisReference;
        public readonly JSProperty Property;
        public readonly JSExpression Replacement;

        public PropertyAccessReplacer (JSExpression thisReference, JSProperty property, JSExpression replacement) {
            ThisReference = thisReference;
            Property = property;
            Replacement = replacement;
        }

        public void VisitNode (JSPropertyAccess pa) {
            if (pa.ThisReference.Equals(ThisReference) && pa.Property.Equals(Property)) {
                ParentNode.ReplaceChild(pa, Replacement);
                VisitReplacement(Replacement);
            } else {
                VisitChildren(pa);
            }
        }
    }
}
