using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.PatternMatching;
using Mono.Cecil;
using Attribute = ICSharpCode.NRefactory.CSharp.Attribute;

namespace JSIL.Internal {
    public class JavascriptOutputVisitor : OutputVisitor {
        public readonly Stack<TypeDeclaration> TypeStack = new Stack<TypeDeclaration>();

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

        protected MethodDefinition ToMethodDefinition (MethodDeclaration declaration) {
            return (MethodDefinition)declaration.Annotations.First();
        }

        protected FieldReference ToFieldReference (AstNode declaration) {
            return (FieldReference)declaration.Annotations.First();
        }

        protected void WriteIdentifier (TypeReference type) {
            base.WriteIdentifier(Util.EscapeIdentifier(
                type.FullName,
                escapePeriods: false
            ));
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

        protected bool IsStatic (AttributedNode node) {
            if (node is TypeDeclaration)
                return true;
            else if (node.Modifiers.HasFlag(Modifiers.Static))
                return true;
            else if (node is FieldDeclaration && ToFieldReference(node).Resolve().Attributes.HasFlag(FieldAttributes.Static))
                return true;

            return false;
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
                if (member is ConstructorDeclaration)
                    continue;
                else if (IsStatic(member))
                    continue;

                member.AcceptVisitor(this, data);
            }

            CloseBrace(BraceStyle.NextLine);
            Semicolon();
            NewLine();

            foreach (var member in typeDeclaration.Members) {
                if (member is ConstructorDeclaration)
                    continue;
                else if (!IsStatic(member))
                    continue;

                member.AcceptVisitor(this, data);
            }

            return EndNode(typeDeclaration);
        }

        public override object VisitPrimitiveType (PrimitiveType primitiveType, object data) {
            Type type;
            if (AstType.PrimitiveTypeToType.TryGetValue(primitiveType.Keyword, out type)) {
                StartNode(primitiveType);
                WriteIdentifier(Util.EscapeIdentifier(type.FullName, false));
                return EndNode(primitiveType);
            } else {
                return base.VisitPrimitiveType(primitiveType, data);
            }
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
            switch (parameterDeclaration.ParameterModifier) {
                case ParameterModifier.Out:
                case ParameterModifier.Ref:
                case ParameterModifier.Params:
                case ParameterModifier.This:
                    throw new NotImplementedException();
                break;
            }

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
            bool isField = variableInitializer.Parent is FieldDeclaration;

            if (!variableInitializer.Initializer.IsNull) {
                if (isField) {
                    var fieldRef = ToFieldReference(variableInitializer.Parent);
                    WriteThisReference(fieldRef.DeclaringType.Resolve(), fieldRef);
                    WriteToken(".", null);
                }

                WriteIdentifier(variableInitializer.Name);
                Space();
                WriteToken("=", VariableInitializer.Roles.Assign);
                Space();
                variableInitializer.Initializer.AcceptVisitor(this, null);
                result = true;
            } else if (!isField) {
                WriteIdentifier(variableInitializer.Name);
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

        protected void WriteThisReference (TypeDefinition declaringType, FieldReference field) {
            if (field.Resolve().Attributes.HasFlag(FieldAttributes.Static))
                WriteIdentifier(declaringType);
            else
                WriteKeyword("this");
        }

        protected void WriteThisReference (TypeDefinition declaringType, AttributedNode node) {
            if (node.Modifiers.HasFlag(Modifiers.Static))
                WriteIdentifier(declaringType);
            else
                WriteKeyword("this");
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            StartNode(methodDeclaration);

            WriteThisReference(ToMethodDefinition(methodDeclaration).DeclaringType, methodDeclaration);

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
