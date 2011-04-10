using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.PatternMatching;
using Mono.Cecil;
using Attribute = ICSharpCode.NRefactory.CSharp.Attribute;

namespace JSIL.Internal {
    public class JavascriptOutputVisitor : OutputVisitor {
        public JavascriptOutputVisitor (IOutputFormatter formatter)
            : base (formatter, new CSharpFormattingPolicy {
                ConstructorBraceStyle = BraceStyle.EndOfLine,
                MethodBraceStyle = BraceStyle.EndOfLine,
            }) {
        }

        protected TypeReference ToTypeReference (SimpleType type) {
            return (TypeReference)type.Annotations.First();
        }

        protected TypeReference ToTypeReference (TypeDeclaration declaration) {
            return (TypeReference)declaration.Annotations.First();
        }

        protected void WriteIdentifier (TypeReference type) {
            base.WriteIdentifier(Util.EscapeIdentifier(
                type.FullName,
                escapePeriods: false
            ));
        }

        protected void WriteIdentifier (ConstructorDeclaration constructor) {
            var declaringType = constructor.Parent as TypeDeclaration;

            var declaringTypeName = Util.EscapeIdentifier(
                ToTypeReference(declaringType).FullName,
                escapePeriods: false
            );
            var methodName = declaringTypeName + 
                (constructor.Modifiers.HasFlag(Modifiers.Static) ? ".cctor" : ".ctor");

            base.WriteIdentifier(Util.EscapeIdentifier(methodName));
        }

        protected void WriteIdentifier (MethodDeclaration method) {
            base.WriteIdentifier(Util.EscapeIdentifier(method.Name));
        }

        protected void WriteIdentifier (MemberReferenceExpression member) {
            base.WriteIdentifier(Util.EscapeIdentifier(member.MemberName));
        }

        protected void WriteIdentifier (NamespaceDeclaration ns) {
            base.WriteIdentifier(Util.EscapeIdentifier(
                ns.FullName,
                escapePeriods: false
            ));
        }

        public override object VisitUsingDeclaration (UsingDeclaration usingDeclaration, object data) {
            return null;
        }

        public override object VisitNamespaceDeclaration (NamespaceDeclaration namespaceDeclaration, object data) {
            StartNode(namespaceDeclaration);

            WriteIdentifier(namespaceDeclaration);
            Space();
            WriteToken("= {}", null);
            Semicolon();
            NewLine();

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

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            StartNode(typeDeclaration);
            WriteIdentifier(ToTypeReference(typeDeclaration));
            Space();
            WriteToken("=", null);
            Space();
            WriteKeyword("function");

            var constructor = (ConstructorDeclaration)typeDeclaration.Members.FirstOrDefault(
                (member) => member is ConstructorDeclaration
            );

            Space();
            LPar();
            if (constructor != null) {
                StartNode(constructor);
                WriteCommaSeparatedList(constructor.Parameters);
                EndNode(constructor);
            }
            RPar();

            OpenBrace(BraceStyle.EndOfLine);

            if (constructor != null) {
                StartNode(constructor);
                constructor.Body.AcceptVisitor(this, "nobraces");
                EndNode(constructor);
            }

            foreach (var member in typeDeclaration.Members) {
                if (member == constructor)
                    continue;

                member.AcceptVisitor(this, data);
            }

            CloseBrace(BraceStyle.NextLine);
            Semicolon();
            NewLine();

            return EndNode(typeDeclaration);
        }

        public override object VisitMemberReferenceExpression (MemberReferenceExpression memberReferenceExpression, object data) {
            StartNode(memberReferenceExpression);
            memberReferenceExpression.Target.AcceptVisitor(this, data);
            WriteToken(".", MemberReferenceExpression.Roles.Dot);
            WriteIdentifier(memberReferenceExpression);
            return EndNode(memberReferenceExpression);
        }

        public override object VisitParameterDeclaration (ParameterDeclaration parameterDeclaration, object data) {
            StartNode(parameterDeclaration);
            // WriteAttributes(parameterDeclaration.Attributes);
            switch (parameterDeclaration.ParameterModifier) {
                case ParameterModifier.Out:
                case ParameterModifier.Ref:
                case ParameterModifier.Params:
                case ParameterModifier.This:
                    throw new NotImplementedException();
                break;
            }
            // parameterDeclaration.Type.AcceptVisitor(this, data);
            // Space();

            if (!string.IsNullOrEmpty(parameterDeclaration.Name))
                WriteIdentifier(parameterDeclaration.Name);

            if (!parameterDeclaration.DefaultExpression.IsNull) {
                Space();
                WriteToken("=", ParameterDeclaration.Roles.Assign);
                Space();
                parameterDeclaration.DefaultExpression.AcceptVisitor(this, data);
            }

            return EndNode(parameterDeclaration);
        }

        protected bool VisitVariableInitializer (VariableInitializer variableInitializer) {
            bool result = false;

            if (!variableInitializer.Initializer.IsNull) {
                if (variableInitializer.Parent is FieldDeclaration) {
                    WriteKeyword("this");
                    WriteToken(".", null);
                }

                WriteIdentifier(variableInitializer.Name);
                Space();
                WriteToken("=", VariableInitializer.Roles.Assign);
                Space();
                variableInitializer.Initializer.AcceptVisitor(this, null);
                result = true;
            }

            return result;
        }

        public override object VisitVariableInitializer (VariableInitializer variableInitializer, object data) {
            StartNode(variableInitializer);

            VisitVariableInitializer(variableInitializer);

            return EndNode(variableInitializer);
        }

        public override object VisitBlockStatement (BlockStatement blockStatement, object data) {
            StartNode(blockStatement);

            if (data as string != "nobraces")
                OpenBrace(BraceStyle.EndOfLine);

            foreach (var node in blockStatement.Statements)
                node.AcceptVisitor(this, data);

            if (data as string != "nobraces") {
                CloseBrace(BraceStyle.NextLine);

                if (data as string != "method")
                    NewLine();
            }

            return EndNode(blockStatement);
        }

        public override object VisitFieldDeclaration (FieldDeclaration fieldDeclaration, object data) {
            if (fieldDeclaration.Modifiers.HasFlag(Modifiers.Static))
                throw new NotImplementedException();

            StartNode(fieldDeclaration);

            int i = 0;
            bool emitted = false;
            foreach (var variable in fieldDeclaration.Variables) {
                i += 1;

                StartNode(variable);
                var result = VisitVariableInitializer(variable);
                EndNode(variable);

                if (result) {
                    if (i != fieldDeclaration.Variables.Count)
                        WriteToken(",", null);

                    emitted |= result;
                }
            }

            if (emitted)
                Semicolon();

            return EndNode(fieldDeclaration);
        }

        public override object VisitSimpleType (SimpleType simpleType, object data) {
            StartNode(simpleType);
            WriteIdentifier(ToTypeReference(simpleType));
            return EndNode(simpleType);
        }

        public override object VisitVariableDeclarationStatement (VariableDeclarationStatement variableDeclarationStatement, object data) {
            StartNode(variableDeclarationStatement);
            WriteKeyword("var");
            Space();
            WriteCommaSeparatedList(variableDeclarationStatement.Variables);
            Semicolon();
            return EndNode(variableDeclarationStatement);
        }

        public override object VisitConstructorDeclaration (ConstructorDeclaration constructorDeclaration, object data) {
            throw new NotImplementedException();
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            StartNode(methodDeclaration);

            WriteKeyword("this");
            WriteToken(".", null);
            WriteIdentifier(methodDeclaration);
            Space();
            WriteToken("=", null);
            Space();
            WriteKeyword("function");
            Space();

            WriteCommaSeparatedListInParenthesis(methodDeclaration.Parameters, true);

            VisitBlockStatement(methodDeclaration.Body, "method");

            Semicolon();

            return EndNode(methodDeclaration);
        }
    }
}
