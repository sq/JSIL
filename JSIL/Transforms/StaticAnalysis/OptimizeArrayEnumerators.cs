using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class OptimizeArrayEnumerators : StaticAnalysisJSAstVisitor {
        public readonly TypeSystem TypeSystem;

        public bool EnableEnumeratorRemoval = true;

        private List<JSBinaryOperatorExpression> SeenAssignments = new List<JSBinaryOperatorExpression>();
        private HashSet<JSVariable> EnumeratorsToKill = new HashSet<JSVariable>();
        private FunctionAnalysis1stPass FirstPass = null;

        private JSFunctionExpression Function;
        private int _NextLoopId = 0;

        public OptimizeArrayEnumerators (QualifiedMemberIdentifier member, IFunctionSource functionSource, TypeSystem typeSystem) 
            : base (member, functionSource) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSFunctionExpression fn) {
            Function = fn;
            FirstPass = GetFirstPass(Function.Method.QualifiedIdentifier);

            VisitChildren(fn);

            if (EnumeratorsToKill.Count > 0) {
                // Rerun the static analyzer since we made major changes
                FunctionSource.InvalidateFirstPass(Function.Method.QualifiedIdentifier);
                FirstPass = GetFirstPass(Function.Method.QualifiedIdentifier);

                // Scan to see if any of the enumerators we eliminated uses of are now
                //  unreferenced. If they are, eliminate them entirely.
                foreach (var variable in EnumeratorsToKill) {
                    var variableName = variable.Name;
                    var accesses = (
                        from a in FirstPass.Accesses
                        where a.Source.Name == variableName
                        select a
                    );

                    if (!accesses.Any()) {
                        var eliminator = new VariableEliminator(
                            variable, new JSNullExpression()
                        );
                        eliminator.Visit(fn);
                    }
                }
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if (boe.Operator == JSOperator.Assignment)
                SeenAssignments.Add(boe);

            VisitChildren(boe);
        }

        public void VisitNode (JSWhileLoop wl) {
            var condInvocation = wl.Condition as JSInvocationExpression;
            JSVariable enumeratorVariable;

            if (
                (condInvocation != null) && 
                (condInvocation.JSMethod != null) &&
                (condInvocation.JSMethod.Identifier == "MoveNext") &&
                (condInvocation.JSMethod.Method.DeclaringType.Interfaces.Any((ii) => ii.Item1.FullName == "System.Collections.IEnumerator")) &&
                ((enumeratorVariable = condInvocation.ThisReference as JSVariable) != null)
            ) {
                var enumeratorType = condInvocation.JSMethod.Method.DeclaringType;

                while (EnableEnumeratorRemoval) {
                    var enumeratorAssignmentBeforeLoop = (
                        from boe in SeenAssignments
                        let boeLeftVar = (boe.Left as JSVariable)
                        where (boeLeftVar != null) && (boeLeftVar.Name == enumeratorVariable.Name)
                        select boe
                    ).LastOrDefault();

                    if (enumeratorAssignmentBeforeLoop == null)
                        break;

                    var enumeratorValue = enumeratorAssignmentBeforeLoop.Right;
                    var assignmentInvocation = enumeratorValue as JSInvocationExpression;
                    if (assignmentInvocation == null) {
                        var rre = enumeratorValue as JSResultReferenceExpression;
                        if (rre != null)
                            assignmentInvocation = rre.Referent as JSInvocationExpression;
                    }

                    if (assignmentInvocation == null)
                        break;

                    var jsm = assignmentInvocation.JSMethod;
                    if (jsm == null)
                        break;

                    var attrParams2 = jsm.Method.Metadata.GetAttributeParameters("JSIL.Meta.JSUnderlyingArray");
                    if (attrParams2 != null) {
                        var arrayMember = (string)attrParams2[0].Value;
                        var lengthMember = (string)attrParams2[1].Value;

                        var replacement = ReplaceWhileLoopAndEnumerator(
                            wl, assignmentInvocation.ThisReference, 
                            condInvocation.ThisReference, condInvocation.JSMethod.Method.DeclaringType,
                            arrayMember, lengthMember
                        );
                        ParentNode.ReplaceChild(wl, replacement);

                        EnumeratorsToKill.Add(enumeratorVariable);

                        VisitReplacement(replacement);

                        return;
                    }

                    break;
                }

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
                    JSOperator.Assignment, arrayVariable, arrayVariable.DefaultValue, arrayVariable.IdentifierType
                ),
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, indexVariable, indexVariable.DefaultValue, indexVariable.IdentifierType
                ),
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, lengthVariable, lengthVariable.DefaultValue, lengthVariable.IdentifierType
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

        private JSForLoop ReplaceWhileLoopAndEnumerator (JSWhileLoop wl, JSExpression backingStore, JSExpression enumerator, TypeInfo enumeratorType, string arrayMember, string lengthMember) {
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
                JSDotExpression.New(backingStore, new JSStringIdentifier(arrayMember, arrayType))
            );
            var indexVariable = new JSVariable(
                indexVariableName, TypeSystem.Int32, Function.Method.Reference,
                JSLiteral.New(0)
            );
            var lengthVariable = new JSVariable(
                lengthVariableName, TypeSystem.Int32, Function.Method.Reference,
                JSDotExpression.New(backingStore, new JSStringIdentifier(lengthMember, TypeSystem.Int32))
            );

            var initializer = new JSVariableDeclarationStatement(
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, arrayVariable, arrayVariable.DefaultValue, arrayVariable.IdentifierType
                ),
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, indexVariable, indexVariable.DefaultValue, indexVariable.IdentifierType
                ),
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment, lengthVariable, lengthVariable.DefaultValue, lengthVariable.IdentifierType
                )
            );

            var condition = new JSBinaryOperatorExpression(
                JSBinaryOperator.LessThan,
                indexVariable, lengthVariable, TypeSystem.Boolean
            );

            var increment = new JSUnaryOperatorExpression(
                JSUnaryOperator.PostIncrement,
                indexVariable, TypeSystem.Int32
            );

            var result = new JSForLoop(
                initializer, condition, new JSExpressionStatement(increment),
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

        private bool ReplacedInvocation = false;

        public PropertyAccessReplacer (JSExpression thisReference, JSProperty property, JSExpression replacement) {
            ThisReference = thisReference;
            Property = property;
            Replacement = replacement;
        }

        public void VisitNode (JSInvocationExpression ie) {
            var jsm = ie.JSMethod;
            // Detect direct invocations of getter methods
            if (
                (jsm != null) && 
                (jsm.Method.Property == Property.Property) && 
                (ie.ThisReference.Equals(ThisReference)) &&
                (ie.Arguments.Count == 0)
            ) {
                ParentNode.ReplaceChild(ie, Replacement);
                VisitReplacement(Replacement);
                ReplacedInvocation = true;
            } else {
                VisitChildren(ie);
            }
        }

        public void VisitNode (JSResultReferenceExpression rre) {
            ReplacedInvocation = false;

            VisitChildren(rre);

            // If a getter invocation that returned a struct was replaced, there's
            //  now a result reference expression that needs to be removed
            if (ReplacedInvocation && !(rre.Children.First() is JSInvocationExpressionBase)) {
                var replacement = rre.Children.First();
                ParentNode.ReplaceChild(rre, replacement);
            }
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
