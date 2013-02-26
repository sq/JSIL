using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class IntroduceVariableReferences : JSAstVisitor {
        public const bool Tracing = false;

        public readonly HashSet<string> TransformedVariables = new HashSet<string>();
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly HashSet<string> ParameterNames;
        public readonly JSILIdentifier JSIL;

        protected readonly HashSet<JSPassByReferenceExpression> ReferencesToTransform = new HashSet<JSPassByReferenceExpression>();
        protected readonly Dictionary<JSVariableDeclarationStatement, JSBlockStatement> Declarations = new Dictionary<JSVariableDeclarationStatement, JSBlockStatement>();

        public IntroduceVariableReferences (JSILIdentifier jsil, Dictionary<string, JSVariable> variables, HashSet<string> parameterNames) {
            JSIL = jsil;
            Variables = variables;
            ParameterNames = parameterNames;
        }

        protected bool MatchesConstructedReference (JSExpression lhs, JSVariable rhs) {
            var jsv = lhs as JSVariable;
            if ((jsv != null) && (jsv.Identifier == rhs.Identifier))
                return true;

            return false;
        }

        protected JSVariable GetReferentVariable (JSPassByReferenceExpression pbr) {
            JSVariable referentVariable;
            JSExpression referent;

            if (!JSReferenceExpression.TryMaterialize(JSIL, pbr.Referent, out referent)) {
                if (JSReferenceExpression.TryDereference(JSIL, pbr.Referent, out referent)) {
                    referentVariable = referent as JSVariable;
                    return referentVariable;
                } else
                    return null;
            }

            referentVariable = referent as JSVariable;
            return referentVariable;
        }

        protected JSVariable GetConstructedReference (JSPassByReferenceExpression pbr) {
            JSVariable referentVariable;
            JSExpression referent;

            if (!JSReferenceExpression.TryMaterialize(JSIL, pbr.Referent, out referent)) {
                // If the reference can be dereferenced, but cannot be materialized, it is
                //  a constructed reference.
                if (JSReferenceExpression.TryDereference(JSIL, pbr.Referent, out referent)) {
                    referentVariable = referent as JSVariable;

                    // Ignore variables we previously transformed.
                    if ((referentVariable != null) && TransformedVariables.Contains(referentVariable.Identifier))
                        return null;

                    return referentVariable;
                } else
                    return null;
            }

            referentVariable = referent as JSVariable;
            if (referentVariable == null)
                return null;

            // Ignore variables we previously transformed.
            if (TransformedVariables.Contains(referentVariable.Identifier))
                return null;

            // If the variable does not match the one in the dictionary, it is a constructed
            //  reference to a parameter.
            var theVariable = Variables[referentVariable.Identifier];
            if (!referentVariable.Equals(theVariable)) {

                // If the parameter is a reference, we don't care about it.
                if (theVariable.IsReference) {
                    // Unless it's the 'this' variable...
                    if (theVariable.IsThis) {
                        // But we handle that separately.
                        return referentVariable;
                    } else {
                        return null;
                    }

                } else
                    return referentVariable;
            }

            return null;
        }

        protected void TransformParameterIntoReference (JSVariable parameter, JSBlockStatement block) {
            var newParameter = new JSParameter("$" + parameter.Identifier, parameter.IdentifierType, parameter.Function);
            var newVariable = new JSVariable(parameter.Identifier, new ByReferenceType(parameter.IdentifierType), parameter.Function);
            var newDeclaration = new JSVariableDeclarationStatement(
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment,
                    // We have to use parameter here, not newVariable or newParameter, otherwise the resulting
                    // assignment looks like 'x.value = initializer' instead of 'x = initializer'
                    parameter,
                    JSIL.NewReference(newParameter),
                    newVariable.IdentifierType
                )
            );

            if (Tracing)
                Debug.WriteLine(String.Format("Transformed {0} into {1}={2}", parameter, newVariable, newParameter));

            Variables[newVariable.Identifier] = newVariable;
            Variables.Add(newParameter.Identifier, newParameter);
            ParameterNames.Remove(parameter.Identifier);
            ParameterNames.Add(newParameter.Identifier);

            var enclosingFunction = Stack.OfType<JSFunctionExpression>().First();
            enclosingFunction.Body.Statements.Insert(0, newDeclaration);
        }

        protected void TransformVariableIntoReference (JSVariable variable, JSVariableDeclarationStatement statement, int declarationIndex, JSBlockStatement enclosingBlock) {
            var oldDeclaration = statement.Declarations[declarationIndex];
            var valueType = oldDeclaration.Right.GetActualType(JSIL.TypeSystem);
            var newVariable = variable.Reference();
            var enclosingFunction = Stack.OfType<JSFunctionExpression>().First();

            JSExpression initialValue;
            // If the declaration was in function scope originally we can hoist the initial value
            //  into our new variable declaration. If not, we need to initialize the ref variable
            //  to the default value for its type. It will get the correct value assigned later.
            if (enclosingBlock == enclosingFunction.Body)
                initialValue = oldDeclaration.Right;
            else
                initialValue = new JSDefaultValueLiteral(valueType);

            var newDeclaration = new JSVariableDeclarationStatement(new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                // We have to use a constructed ref to the variable here, otherwise
                //  the declaration will look like 'var x.value = foo'
                new JSVariable(variable.Identifier, variable.IdentifierType, variable.Function),
                JSIL.NewReference(initialValue), 
                newVariable.IdentifierType
            ));

            if (Tracing)
                Debug.WriteLine(String.Format("Transformed {0} into {1} in {2}", variable, newVariable, statement));

            // Insert the new declaration directly before the top-level block containing the original
            //  declaration. This ensures that if its initial value has a dependency on external state,
            //  the declaration will not precede the values it depends on.
            // Note that for declarations that were hoisted out of inner blocks (conditionals, loops)
            //  it doesn't actually matter where the insert occurs, since we initialize with a default
            //  value in that case.
            enclosingFunction.Body.InsertNearChildRecursive(
                statement, newDeclaration, 0
            );

            // If the reference is being declared in function scope, it doesn't need a separate assignment
            //  for its initialization. Otherwise, we need to insert an assignment after the original variable
            //  declaration statement to ensure that the reference variable is initialized to the right value
            //  at the exact right point in the function's execution.
            if (enclosingBlock != enclosingFunction.Body) {
                var newAssignment = new JSExpressionStatement(
                    new JSBinaryOperatorExpression(
                        JSOperator.Assignment, newVariable, oldDeclaration.Right, valueType
                    )
                );
                var insertLocation = enclosingBlock.Statements.IndexOf(statement) + 1;
                enclosingBlock.Statements.Insert(insertLocation, newAssignment);
            }

            Variables[variable.Identifier] = newVariable;
            statement.Declarations.RemoveAt(declarationIndex);
            TransformedVariables.Add(variable.Identifier);
        }

        public void VisitNode (JSPassByReferenceExpression pbr) {
            if (GetConstructedReference(pbr) != null)
                ReferencesToTransform.Add(pbr);

            VisitChildren(pbr);
        }

        public void VisitNode (JSVariableDeclarationStatement vds)  {
            var parentBlock = (JSBlockStatement)ParentNode;

            // FIXME: Is this right?
            JSBlockStatement existing;
            if (Declarations.TryGetValue(vds, out existing)) {
                if (existing != parentBlock)
                    throw new InvalidDataException("Multiple parents for a single declaration statement");
            } else {
                Declarations.Add(vds, parentBlock);
            }

            VisitChildren(vds);
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Create a new visitor for nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null) {
                new IntroduceVariableReferences(JSIL, fn.AllVariables, new HashSet<string>(from p in fn.Parameters select p.Name)).Visit(fn);
                return;
            }

            VisitChildren(fn);

            foreach (var p in fn.Parameters) {
                if (!p.IsReference)
                    continue;

                var vrat = new VariableReferenceAccessTransformer(JSIL, p);
                vrat.Visit(fn);
            }

            /*
            if (!fn.Method.Method.IsStatic) {
                var vrat = new VariableReferenceAccessTransformer(JSIL, Variables["this"]);
                vrat.Visit(fn);
            }
             */

            foreach (var r in ReferencesToTransform) {
                var cr = GetConstructedReference(r);

                if (cr == null) {
                    // We have already done the variable transform for this variable in the past.
                    continue;
                }

                // For 'ref this', we need to replace each individual expression, because we can't
                //  rename the this-variable.
                if (cr.IsThis) {
                    var refThis = JSIL.NewReference(Variables["this"]);
                    fn.ReplaceChildRecursive(r, refThis);
                    continue;
                }

                var parameter = (from p in fn.Parameters
                                 where p.Identifier == cr.Identifier
                                 select p).FirstOrDefault();

                if (parameter != null) {
                    TransformParameterIntoReference(parameter, fn.Body);
                } else {
                    var declaration = (from kvp in Declarations
                                       let vds = kvp.Key
                                       from ivd in vds.Declarations.Select((vd, i) => new { vd = vd, i = i })
                                       where MatchesConstructedReference(ivd.vd.Left, cr)
                                       select new { vds = vds, vd = ivd.vd, i = ivd.i, block = kvp.Value }).FirstOrDefault();

                    if (declaration == null)
                        throw new InvalidOperationException(String.Format("Could not locate declaration for {0}", cr));

                    TransformVariableIntoReference(
                        (JSVariable)declaration.vd.Left, 
                        declaration.vds, 
                        declaration.i,
                        declaration.block
                    );
                }

                var vrat = new VariableReferenceAccessTransformer(JSIL, cr);
                vrat.Visit(fn);
            }
        }
    }

    public class VariableReferenceAccessTransformer : JSAstVisitor {
        public readonly JSVariable Variable;
        public readonly JSILIdentifier JSIL;

        public VariableReferenceAccessTransformer (JSILIdentifier jsil, JSVariable variable) {
            JSIL = jsil;
            Variable = variable;
        }

        public void VisitNode (JSVariable variable) {
            if (ParentNode is JSFunctionExpression) {
                VisitChildren(variable);
                return;
            }

            if (
                (variable.Identifier != Variable.Identifier) ||
                // Don't transform if we're inside a read-through already
                (ParentNode is JSReadThroughReferenceExpression) ||
                (
                    // If we're inside a write-through and on the LHS, don't transform
                    (ParentNode is JSWriteThroughReferenceExpression) &&
                    (this.CurrentName == "Left")
                )
            ) {
                VisitChildren(variable);
                return;
            }

            // Don't transform instances of a variable on the LHS of a variable declaration.
            if (
                Stack.OfType<JSVariableDeclarationStatement>().Any() &&
                NameStack.Contains("Left")
            ) {
                VisitChildren(variable);
                return;
            }

            // If we're inside a pass-by-reference (ref x) then don't transform
            if (
                Stack.OfType<JSPassByReferenceExpression>().Any()
            ) {
                VisitChildren(variable);
                return;
            }

            var replacement = new JSReadThroughReferenceExpression(variable);
            ParentNode.ReplaceChild(variable, replacement);
            VisitReplacement(replacement);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            JSExpression left;

            if (!JSReferenceExpression.TryDereference(JSIL, boe.Left, out left))
                left = boe.Left;

            var leftVar = left as JSVariable;
            var leftChangeType = left as JSChangeTypeExpression;

            if (leftChangeType != null)
                leftVar = leftChangeType.Expression as JSVariable;

            if (
                !(ParentNode is JSVariableDeclarationStatement) &&
                (leftVar != null) && 
                (leftVar.Identifier == Variable.Identifier)
            ) {
                if (boe.Operator is JSAssignmentOperator) {
                    var replacement = new JSWriteThroughReferenceExpression(
                        Variable, boe.Right
                    );
                    ParentNode.ReplaceChild(boe, replacement);
                    VisitReplacement(replacement);
                } else {
                    VisitChildren(boe);
                }
            } else {
                VisitChildren(boe);
            }
        }

        public void VisitNode (JSWriteThroughReferenceExpression wtre) {
            VisitChildren(wtre);
        }
    }
}
