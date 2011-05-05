using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;

namespace JSIL.Transforms {
    public class IntroduceVariableReferences : JSAstVisitor {
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly JSILIdentifier JSIL;

        public IntroduceVariableReferences (JSILIdentifier jsil, Dictionary<string, JSVariable> variables) {
            JSIL = jsil;
            Variables = variables;
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

                    return referentVariable;
                } else
                    return null;
            }

            referentVariable = referent as JSVariable;
            if (referentVariable == null)
                return null;

            // If the variable does not match the one in the dictionary, it is a constructed
            //  reference to a parameter.
            if (referentVariable != Variables[referentVariable.Identifier]) {
                if (!referentVariable.IsParameter)
                    throw new InvalidOperationException();

                return referentVariable;
            }

            return null;
        }

        public void VisitNode (JSFunctionExpression fn) {
            var referencesToTransform =
                from r in fn.AllChildrenRecursive.OfType<JSPassByReferenceExpression>()
                let cr = GetConstructedReference(r)
                where cr != null
                select r;
            var declarations =
                fn.AllChildrenRecursive.OfType<JSVariableDeclarationStatement>().ToArray();

            foreach (var r in referencesToTransform) {
                var cr = GetConstructedReference(r);

                var parameter = (from p in fn.Parameters
                                 where p.Identifier == cr.Identifier
                                 select p).FirstOrDefault();

                if (parameter != null) {
                    Console.WriteLine("{0} is reference to {1}", r, parameter);
                } else {
                    var declaration = (from vds in declarations
                                       from vd in vds.Declarations
                                       where MatchesConstructedReference(vd.Left, cr)
                                       select new { vds = vds, vd = vd }).FirstOrDefault();

                    if (declaration == null)
                        throw new InvalidOperationException(String.Format("Could not locate declaration for {0}", cr));

                    Console.WriteLine("{0} declared in {1} at {2}", r, declaration.vds, declaration.vd);
                }
            }

            VisitChildren(fn);
        }
    }
}
