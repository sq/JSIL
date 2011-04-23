using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Expressions;
using ICSharpCode.Decompiler.Ast.Transforms;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class ReplacementFinder : ContextTrackingVisitor<object> {
        public ReplacementFinder (DecompilerContext context)
            : base(context) {
        }

        protected string GetReplacement (AstNodeCollection<AttributeSection> attributes) {
            foreach (var section in attributes)
            foreach (var attribute in section.Attributes) {
                switch (attribute.Type.ToString()) {
                    case "JSReplacement":
                        var replacement = attribute.Arguments.FirstOrDefault() as PrimitiveExpression;
                        if (replacement == null)
                            throw new InvalidOperationException("JSReplacement's only argument must be a string");

                        return (string)replacement.Value;
                }
            }

            return null;
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            string replacement = GetReplacement(methodDeclaration.Attributes);

            if (replacement != null) {
                var declaringType = methodDeclaration.Annotation<MethodDefinition>().DeclaringType;

                var pd = new PropertyDeclaration {
                    ReturnType = SimpleType.Create(typeof(object)),
                    Name = methodDeclaration.Name,
                };
                pd.AddAnnotation(methodDeclaration);
                pd.AddAnnotation(new PrimitiveExpression(replacement));                

                methodDeclaration.ReplaceWith(pd);
            }

            return null;
        }
    }
}
