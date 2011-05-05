using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class IntroduceVariableReferences : JSAstVisitor {
        public readonly HashSet<string> TransformedVariables = new HashSet<string>();
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly HashSet<string> ParameterNames;
        public readonly JSILIdentifier JSIL;

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

        protected JSVariable GetConstructedReference (JSPassByReferenceExpression pbr) {
            JSVariable referentVariable;
            JSExpression referent;

            if (!JSReferenceExpression.TryMaterialize(JSIL, pbr.Referent, out referent)) {
                // If the reference can be dereferenced, but cannot be materialized, it is
                //  a constructed reference.
                if (JSReferenceExpression.TryDereference(pbr.Referent, out referent)) {
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
            if (!referentVariable.Equals(Variables[referentVariable.Identifier])) {
                if (!referentVariable.IsParameter)
                    throw new InvalidOperationException();

                // If the parameter is a reference, we don't care about it.
                if (Variables[referentVariable.Identifier].IsReference)
                    return null;
                else
                    return referentVariable;
            }

            return null;
        }

        protected void TransformParameterIntoReference (JSVariable parameter, JSBlockStatement block) {
            var newParameter = new JSParameter("_" + parameter.Identifier, parameter.Type);
            var newVariable = new JSVariable(parameter.Identifier, new ByReferenceType(parameter.Type));
            var newDeclaration = new JSVariableDeclarationStatement(
                new JSBinaryOperatorExpression(
                    JSOperator.Assignment,
                    // We have to use parameter here, not newVariable or newParameter, otherwise the resulting
                    // assignment looks like 'x.value = initializer' instead of 'x = initializer'
                    parameter,
                    JSIL.NewReference(newParameter),
                    newVariable.Type
                )
            );

            Debug.WriteLine(String.Format("Transformed {0} into {1}={2}", parameter, newVariable, newParameter));
            Variables[newVariable.Identifier] = newVariable;
            Variables.Add(newParameter.Identifier, newParameter);
            ParameterNames.Remove(parameter.Identifier);
            ParameterNames.Add(newParameter.Identifier);
            block.Statements.Insert(0, newDeclaration);
        }

        protected void TransformVariableIntoReference (JSVariable variable, JSVariableDeclarationStatement statement, int declarationIndex) {
            var oldDeclaration = statement.Declarations[declarationIndex];
            var newVariable = variable.Reference();
            var newDeclaration = new JSBinaryOperatorExpression(
                JSOperator.Assignment,
                // We have to use variable here, not newVariable, otherwise the resulting
                // assignment looks like 'x.value = initializer' instead of 'x = initializer'
                variable, 
                JSIL.NewReference(oldDeclaration.Right), 
                newVariable.Type
            );

            Debug.WriteLine(String.Format("Transformed {0} into {1}", variable, newVariable));
            Variables[variable.Identifier] = newVariable;
            statement.Declarations[declarationIndex] = newDeclaration;
            TransformedVariables.Add(variable.Identifier);
        }

        public void VisitNode (JSFunctionExpression fn) {
            var referencesToTransform =
                (from r in fn.AllChildrenRecursive.OfType<JSPassByReferenceExpression>()
                let cr = GetConstructedReference(r)
                where cr != null
                select r).ToArray();
            var declarations =
                fn.AllChildrenRecursive.OfType<JSVariableDeclarationStatement>().ToArray();

            foreach (var r in referencesToTransform) {
                var cr = GetConstructedReference(r);

                if (cr == null) {
                    // We have already done the variable transform for this variable in the past.
                    continue;
                }

                var parameter = (from p in fn.Parameters
                                 where p.Identifier == cr.Identifier
                                 select p).FirstOrDefault();

                if (parameter != null) {
                    TransformParameterIntoReference(parameter, fn.Body);
                } else {
                    var declaration = (from vds in declarations
                                       from ivd in vds.Declarations.Select((vd, i) => new { vd = vd, i = i })
                                       where MatchesConstructedReference(ivd.vd.Left, cr)
                                       select new { vds = vds, vd = ivd.vd, i = ivd.i }).FirstOrDefault();

                    if (declaration == null)
                        throw new InvalidOperationException(String.Format("Could not locate declaration for {0}", cr));

                    TransformVariableIntoReference(
                        (JSVariable)declaration.vd.Left, 
                        declaration.vds, 
                        declaration.i
                    );
                }
            }

            VisitChildren(fn);
        }
    }
}
