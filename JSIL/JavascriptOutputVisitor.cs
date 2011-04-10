using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.PatternMatching;

namespace JSIL.Internal {
    public class JavascriptOutputVisitor : OutputVisitor {
        public JavascriptOutputVisitor (IOutputFormatter formatter)
            : base (formatter, new CSharpFormattingPolicy()) {
        }

        public override object VisitUsingDeclaration (UsingDeclaration usingDeclaration, object data) {
            return null;
        }

        public override object VisitNamespaceDeclaration (NamespaceDeclaration namespaceDeclaration, object data) {
            StartNode(namespaceDeclaration);

            foreach (var member in namespaceDeclaration.Members)
                member.AcceptVisitor(this, data);

            return EndNode(namespaceDeclaration);
        }

        public override object VisitAttributeSection (AttributeSection attributeSection, object data) {
            return null;
        }

        public override object VisitAttribute (Attribute attribute, object data) {
            return null;
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            StartNode(methodDeclaration);
            // WriteAttributes(methodDeclaration.Attributes);
            // WriteModifiers(methodDeclaration.ModifierTokens);
            methodDeclaration.ReturnType.AcceptVisitor(this, data);
            Space();
            // WritePrivateImplementationType(methodDeclaration.PrivateImplementationType);
            WriteIdentifier(methodDeclaration.Name);
            // WriteTypeParameters(methodDeclaration.TypeParameters);
            Space();
            WriteCommaSeparatedListInParenthesis(methodDeclaration.Parameters, true);
            foreach (Constraint constraint in methodDeclaration.Constraints) {
                constraint.AcceptVisitor(this, data);
            }
            WriteMethodBody(methodDeclaration.Body);
            return EndNode(methodDeclaration);
        }
    }
}
