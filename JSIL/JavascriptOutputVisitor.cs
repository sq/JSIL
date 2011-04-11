using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ICSharpCode.Decompiler.Ast;
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

        protected TypeDefinition ToTypeDefinition (AstNode node) {
            return (TypeDefinition)node.Annotations.First();
        }

        protected MethodDefinition ToMethodDefinition (MethodDeclaration declaration) {
            return (MethodDefinition)declaration.Annotations.First();
        }

        protected PropertyDefinition ToPropertyDefinition (AstNode declaration) {
            return (PropertyDefinition)declaration.Annotations.First();
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
            else if (node is PropertyDeclaration) {
                var def = ToPropertyDefinition(node).Resolve();
                return def.GetMethod.IsStatic || def.SetMethod.IsStatic;
            }

            return false;
        }

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            StartNode(typeDeclaration);
            WriteIdentifier(ToTypeReference(typeDeclaration));
            Space();
            WriteToken("=", null);
            Space();
            WriteKeyword("function");

            int numStaticMembers = 0;
            var constructors = (from member in typeDeclaration.Members
                               where member is ConstructorDeclaration
                               select (ConstructorDeclaration)member).ToArray();
            var instanceConstructor = (from constructor in constructors
                                      where !constructor.Modifiers.HasFlag(Modifiers.Static)
                                      select constructor).FirstOrDefault();
            var staticConstructor = (from constructor in constructors
                                       where constructor.Modifiers.HasFlag(Modifiers.Static)
                                       select constructor).FirstOrDefault();

            Space();
            LPar();
            if (instanceConstructor != null) {
                StartNode(instanceConstructor);
                WriteCommaSeparatedList(instanceConstructor.Parameters);
                EndNode(instanceConstructor);
            }
            RPar();

            OpenBrace(BraceStyle.EndOfLine);

            foreach (var member in typeDeclaration.Members) {
                if (member is ConstructorDeclaration)
                    continue;
                else if (IsStatic(member)) {
                    numStaticMembers += 1;
                    continue;
                }

                member.AcceptVisitor(this, data);
            }

            if (instanceConstructor != null) {
                StartNode(instanceConstructor);
                instanceConstructor.Body.AcceptVisitor(this, "nobraces");
                EndNode(instanceConstructor);
            }

            CloseBrace(BraceStyle.NextLine);
            Semicolon();
            NewLine();

            if ((staticConstructor != null) || (numStaticMembers > 0)) {
                LPar();

                Space();
                WriteKeyword("function");
                // I'd emit a function name here for debuggability, but for some
                //  reason that breaks tests :(
                Space();

                LPar();
                RPar();

                OpenBrace(BraceStyle.EndOfLine);

                foreach (var member in typeDeclaration.Members) {
                    if (member is ConstructorDeclaration)
                        continue;
                    else if (!IsStatic(member))
                        continue;

                    member.AcceptVisitor(this, data);
                }

                if (staticConstructor != null) {
                    StartNode(staticConstructor);
                    staticConstructor.Body.AcceptVisitor(this, "nobraces");
                    EndNode(staticConstructor);
                }

                CloseBrace(BraceStyle.NextLine);

                RPar();
                Space();

                LPar();
                RPar();

                Semicolon();
                NewLine();
            }

            return EndNode(typeDeclaration);
        }

        public override object VisitArrayCreateExpression (ArrayCreateExpression arrayCreateExpression, object data) {
            StartNode(arrayCreateExpression);
            WriteKeyword("new");
            Space();
            WriteKeyword("Array");
            LPar();

            if (arrayCreateExpression.Arguments.Count > 1)
                throw new NotImplementedException("Multidimensional arrays are not supported");
            else if (arrayCreateExpression.Arguments.Count > 0)
                WriteCommaSeparatedList(arrayCreateExpression.Arguments);

            RPar();
            return EndNode(arrayCreateExpression);
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

        public override object VisitComposedType (ComposedType composedType, object data) {
            return base.VisitComposedType(composedType, data);
        }

        public override object VisitMemberType (MemberType memberType, object data) {
            StartNode(memberType);
            memberType.Target.AcceptVisitor(this, data);
            WriteToken(".", MemberType.Roles.Dot);
            WriteIdentifier(Util.EscapeIdentifier(memberType.MemberName));
            return EndNode(memberType);
        }

        public override object VisitTypeOfExpression (TypeOfExpression typeOfExpression, object data) {
            StartNode(typeOfExpression);

            typeOfExpression.Type.AcceptVisitor(this, data);

            return EndNode(typeOfExpression);
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
                    throw new NotImplementedException("Parameter modifiers not supported");
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
            bool isNull = variableInitializer.Initializer.IsNull;
            Expression fakeInitializer = null;

            if (isNull && isField) {
                var fieldRef = ToFieldReference(variableInitializer.Parent);
                if (fieldRef.FieldType.IsPrimitive) {
                    isNull = false;
                    fakeInitializer = AstMethodBodyBuilder.MakeDefaultValue(fieldRef.FieldType);
                }
            }

            if (!isNull) {
                if (isField) {
                    var fieldRef = ToFieldReference(variableInitializer.Parent);
                    WriteThisReference(fieldRef.DeclaringType.Resolve(), fieldRef);
                    WriteToken(".", null);
                }

                WriteIdentifier(variableInitializer.Name);
                Space();
                WriteToken("=", VariableInitializer.Roles.Assign);
                Space();
                if (fakeInitializer != null) {
                    if (fakeInitializer is NullReferenceExpression)
                        WriteKeyword("null");
                    else if (fakeInitializer is PrimitiveExpression)
                        WritePrimitiveValue(((PrimitiveExpression)fakeInitializer).Value);
                    else
                        throw new NotImplementedException("Unable to emit default value for field");
                } else {
                    variableInitializer.Initializer.AcceptVisitor(this, null);
                }
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

        public override object VisitLabelStatement (LabelStatement labelStatement, object data) {
            throw new NotImplementedException("Goto and labels are not implemented");
        }

        public override object VisitGotoStatement (GotoStatement gotoStatement, object data) {
            throw new NotImplementedException("Goto and labels are not implemented");
        }

        public override object VisitGotoCaseStatement (GotoCaseStatement gotoCaseStatement, object data) {
            throw new NotImplementedException("Goto and labels are not implemented");
        }

        public override object VisitGotoDefaultStatement (GotoDefaultStatement gotoDefaultStatement, object data) {
            throw new NotImplementedException("Goto and labels are not implemented");
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

        public override object VisitPropertyDeclaration (PropertyDeclaration propertyDeclaration, object data) {
            StartNode(propertyDeclaration);

            var propertyDefinition = ToPropertyDefinition(propertyDeclaration);
            var declaringType = propertyDefinition.DeclaringType;
            bool isStatic = propertyDefinition.GetMethod.IsStatic || propertyDefinition.SetMethod.IsStatic;
            bool isAutoProperty = propertyDefinition.GetMethod.CustomAttributes.Concat(
                    propertyDefinition.SetMethod.CustomAttributes
                ).Where((ca) => ca.AttributeType.Name == "CompilerGeneratedAttribute")
                .Count() > 0;

            WriteIdentifier("Object");
            WriteToken(".", null);
            WriteIdentifier("defineProperty");
            LPar();

            if (isStatic)
                WriteIdentifier(declaringType);
            else
                WriteKeyword("this");
            WriteToken(",", null);
            Space();

            WritePrimitiveValue(Util.EscapeIdentifier(propertyDeclaration.Name));
            WriteToken(",", null);

            OpenBrace(BraceStyle.EndOfLine);

            bool first = true;
            foreach (AstNode node in propertyDeclaration.Children) {
                if (node.Role == IndexerDeclaration.GetterRole || node.Role == IndexerDeclaration.SetterRole) {
                    if (!first) {
                        WriteToken(",", null);
                        Space();
                    }
                    first = false;

                    node.AcceptVisitor(this, data);
                }
            }

            CloseBrace(BraceStyle.NextLine);

            RPar();
            Semicolon();

            // If the property is of a primitive type, we must assign it a default value so it's not undefined
            if (propertyDefinition.PropertyType.IsPrimitive && isAutoProperty) {
                if (isStatic)
                    WriteIdentifier(declaringType);
                else
                    WriteKeyword("this");

                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(propertyDeclaration.Name));

                Space();
                WriteToken("=", null);
                Space();

                WritePrimitiveValue(AstMethodBodyBuilder.MakeDefaultValue(propertyDefinition.PropertyType));
                Semicolon();
            }

            return EndNode(propertyDeclaration);
        }

        public override object VisitAccessor (Accessor accessor, object data) {
            StartNode(accessor);
            string suffix;

            if (accessor.Role == PropertyDeclaration.GetterRole) {
                suffix = "get";
            } else if (accessor.Role == PropertyDeclaration.SetterRole) {
                suffix = "set";
            } else {
                throw new NotImplementedException();
            }

            var propertyDefinition = ToPropertyDefinition(accessor.Parent);
            var declaringType = propertyDefinition.DeclaringType;
            var storageName = Util.EscapeIdentifier(
                String.Format("{0}.value", propertyDefinition.Name)
            );

            bool isStatic = propertyDefinition.GetMethod.IsStatic || propertyDefinition.SetMethod.IsStatic;

            WriteIdentifier(suffix);

            Space();
            WriteToken(":", null);
            Space();

            WriteKeyword("function");
            Space();
            LPar();
            if (accessor.Role != PropertyDeclaration.GetterRole)
                WriteKeyword("value");
            RPar();

            if (accessor.Body.IsNull) {
                OpenBrace(BraceStyle.EndOfLine);
                if (accessor.Role == PropertyDeclaration.GetterRole) {
                    WriteKeyword("return");

                    if (isStatic)
                        WriteIdentifier(declaringType);
                    else
                        WriteKeyword("this");

                    WriteToken(".", null);
                    WriteIdentifier(storageName);

                    Semicolon();
                } else if (accessor.Role == PropertyDeclaration.SetterRole) {
                    if (isStatic)
                        WriteIdentifier(declaringType);
                    else
                        WriteKeyword("this");

                    WriteToken(".", null);
                    WriteIdentifier(storageName);

                    Space();
                    WriteToken("=", null);
                    Space();
                    WriteIdentifier("value");

                    Semicolon();
                }

                CloseBrace(BraceStyle.NextLine);
            } else {
                WriteMethodBody(accessor.Body);
            }

            return EndNode(accessor);
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
            throw new InvalidOperationException("Constructors should have been processed by the type definition explicitly");
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
