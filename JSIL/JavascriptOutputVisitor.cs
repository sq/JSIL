using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Transforms;
using Mono.Cecil;
using Attribute = ICSharpCode.NRefactory.CSharp.Attribute;
using JSIL.Expressions;
using DynamicExpression = JSIL.Expressions.DynamicExpression;
using Expression = ICSharpCode.NRefactory.CSharp.Expression;
using InvocationExpression = ICSharpCode.NRefactory.CSharp.InvocationExpression;

namespace JSIL.Internal {
    public class JavascriptOutputVisitor 
        : OutputVisitor, 
          IDynamicExpressionVisitor<object, object>,
          ITargetedControlFlowVisitor<object, object>
    {
        public readonly Stack<string> BaseTypeStack = new Stack<string>();

        public JavascriptOutputVisitor (IOutputFormatter formatter)
            : base (formatter, new CSharpFormattingPolicy {
                ConstructorBraceStyle = BraceStyle.EndOfLine,
                MethodBraceStyle = BraceStyle.EndOfLine,
            }) {
        }

        protected override void WriteIdentifier (string identifier, Role<Identifier> identifierRole = null) {
            WriteSpecialsUpToRole(
                identifierRole ?? AstNode.Roles.Identifier
            );

            if (lastWritten == LastWritten.KeywordOrIdentifier)
                formatter.Space(); // this space is strictly required, so we directly call the formatter

            formatter.WriteIdentifier(identifier);
            lastWritten = LastWritten.KeywordOrIdentifier;
        }

        protected void WriteIdentifier (TypeReference type) {
            WriteIdentifier(Util.EscapeIdentifier(
                type.FullName,
                escapePeriods: false
            ));
        }

        protected void WriteIdentifier (MethodDeclaration method) {
            WriteIdentifier(Util.EscapeIdentifier(method.Name));
        }

        protected void WriteIdentifier (MemberReferenceExpression member) {
            WriteIdentifier(Util.EscapeIdentifier(member.MemberName));
        }

        protected void WriteIdentifier (OperatorDeclaration op) {
            WriteIdentifier(PickOperatorName(op));
        }

        protected void WriteIdentifier (NamespaceDeclaration ns) {
            WriteIdentifier(Util.EscapeIdentifier(
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

        public override object VisitUnaryOperatorExpression (UnaryOperatorExpression unaryOperatorExpression, object data) {
            var method = unaryOperatorExpression.Annotation<MethodDefinition>();

            if (method != null) {
                StartNode(unaryOperatorExpression);

                WriteIdentifier(method.DeclaringType);
                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(method.Name));

                LPar();
                unaryOperatorExpression.Expression.AcceptVisitor(this, data);
                RPar();

                return EndNode(unaryOperatorExpression);
            } else {
                return base.VisitUnaryOperatorExpression(unaryOperatorExpression, data);
            }
        }

        public override object VisitBinaryOperatorExpression (BinaryOperatorExpression binaryOperatorExpression, object data) {
            var method = binaryOperatorExpression.Annotation<MethodDefinition>();

            if (method != null) {
                StartNode(binaryOperatorExpression);

                WriteIdentifier(method.DeclaringType);
                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(method.Name));

                LPar();
                binaryOperatorExpression.Left.AcceptVisitor(this, data);
                WriteToken(",", null);
                Space();
                binaryOperatorExpression.Right.AcceptVisitor(this, data);
                RPar();

                return EndNode(binaryOperatorExpression);
            } else {
                return base.VisitBinaryOperatorExpression(binaryOperatorExpression, data);
            }
        }

        protected bool IsStatic (AttributedNode node) {
            if (node is TypeDeclaration)
                return true;
            else if (node.Modifiers.HasFlag(Modifiers.Static))
                return true;
            else if (
                node is FieldDeclaration && 
                node.Annotation<FieldReference>().Resolve()
                    .Attributes.HasFlag(FieldAttributes.Static)
            ) {
                return true;
            } else if (node is PropertyDeclaration) {
                return true;
            }

            return false;
        }

        protected bool IsIgnored (AstNodeCollection<AttributeSection> attributes) {
            foreach (var section in attributes)
            foreach (var attribute in section.Attributes) {
                switch (attribute.Type.ToString()) {
                    case "JSIgnore":
                    return true;
                    break;
                }
            }

            return false;
        }

        public override object VisitInvocationExpression (InvocationExpression invocationExpression, object data) {
            var mre = invocationExpression.Target as MemberReferenceExpression;
            TypeReferenceExpression tre;

            if (mre != null) {
                if ((tre = mre.Target as TypeReferenceExpression) != null) {
                    var type = tre.Type.Annotation<TypeReference>();

                    if ((type != null) && (type.FullName == "JSIL.Verbatim")) {
                        switch (mre.MemberName) {
                            case "Eval": {
                                StartNode(invocationExpression);

                                var firstArgument = invocationExpression.Arguments.FirstOrDefault() as PrimitiveExpression;
                                if (firstArgument == null)
                                    throw new InvalidOperationException("Verbatim.Eval's only argument must be a string");

                                var rawText = firstArgument.Value as string;
                                if (rawText == null)
                                    throw new InvalidOperationException("Verbatim.Eval's only argument must be a string");

                                WriteToken(rawText.Trim(), null);

                                return EndNode(invocationExpression);
                            }
                            default:
                            throw new NotImplementedException();
                        }
                    }
                } else if (mre.Target is BaseReferenceExpression) {
                    StartNode(invocationExpression);
                    invocationExpression.Target.AcceptVisitor(this, data);
                    WriteToken(".", null);
                    WriteIdentifier("call");
                    LPar();
                    WriteKeyword("this");
                    if (invocationExpression.Arguments.Count > 0) {
                        WriteToken(",", null);
                        Space();
                        WriteCommaSeparatedList(invocationExpression.Arguments);
                    }
                    RPar();
                    return EndNode(invocationExpression);
                }
            }

            return base.VisitInvocationExpression(invocationExpression, data);
        }

        protected IEnumerable<ConstructorDeclaration> GetConstructors (TypeDeclaration typeDeclaration) {
            return (from member in typeDeclaration.Members
                    where member is ConstructorDeclaration
                    select (ConstructorDeclaration)member);
        }

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            if (IsIgnored(typeDeclaration.Attributes))
                return null;

            int numStaticMembers = 0;
            bool isStatic = typeDeclaration.Modifiers.HasFlag(Modifiers.Static);

            var constructors = GetConstructors(typeDeclaration).ToArray();

            var instanceConstructors = (from constructor in constructors
                                       where !constructor.Modifiers.HasFlag(Modifiers.Static)
                                       select constructor);
            var instanceConstructor = instanceConstructors.FirstOrDefault();

            var staticConstructor = (from constructor in constructors
                                      where constructor.Modifiers.HasFlag(Modifiers.Static)
                                      select constructor).FirstOrDefault();

            if (instanceConstructors.Count() > 1)
                throw new NotImplementedException("Overloaded constructors are not supported");

            StartNode(typeDeclaration);
            WriteIdentifier(typeDeclaration.Annotation<TypeReference>());
            Space();
            WriteToken("=", null);
            Space();

            if (isStatic) {
            } else {
                WriteKeyword("function");

                Space();
                LPar();
                if (instanceConstructor != null) {
                    StartNode(instanceConstructor);
                    WriteCommaSeparatedList(instanceConstructor.Parameters);
                    EndNode(instanceConstructor);
                }
                RPar();
            }

            OpenBrace(BraceStyle.EndOfLine);

            if (!isStatic) {
                WriteKeyword("this");
                WriteToken(".", null);
                WriteKeyword("__ctor");
                LPar();

                if (instanceConstructor != null) {
                    StartNode(instanceConstructor);
                    WriteCommaSeparatedList(instanceConstructor.Parameters);
                    EndNode(instanceConstructor);
                }

                RPar();
                Semicolon();
            }

            CloseBrace(BraceStyle.NextLine);
            Semicolon();
            NewLine();

            if (true) {
                LPar();

                Space();
                WriteKeyword("function");
                // I'd emit a function name here for debuggability, but for some
                //  reason that breaks tests :(
                Space();

                LPar();
                RPar();

                OpenBrace(BraceStyle.EndOfLine);

                if (isStatic) {
                } else if (typeDeclaration.BaseTypes.Count > 1) {
                    throw new NotImplementedException("Inheritance from multiple bases not implemented");
                } else {
                    string baseClass = "System.Object";
                    if (typeDeclaration.BaseTypes.Count == 1) {
                        var baseReference = typeDeclaration.BaseTypes.FirstOrDefault().Annotation<TypeReference>();
                        baseClass = baseReference.FullName;
                        BaseTypeStack.Push(baseClass);
                    }

                    WriteIdentifier(typeDeclaration.Annotation<TypeReference>());
                    WriteToken(".", null);
                    WriteKeyword("prototype");
                    Space();
                    WriteToken("=", null);
                    Space();
                    WriteIdentifier("JSIL.CloneObject");
                    LPar();
                    WriteIdentifier(baseClass);
                    WriteToken(".", null);
                    WriteKeyword("prototype");
                    RPar();
                    Semicolon();
                }

                if (!isStatic) {
                    WriteIdentifier(typeDeclaration.Annotation<TypeReference>());
                    WriteToken(".", null);
                    WriteIdentifier("prototype");
                    WriteToken(".", null);
                    WriteIdentifier("__TypeName__");
                    Space();
                    WriteToken("=", null);
                    Space();
                    WritePrimitiveValue(typeDeclaration.Annotation<TypeReference>().ToString());
                    Semicolon();

                    WriteIdentifier(typeDeclaration.Annotation<TypeReference>());
                    WriteToken(".", null);
                    WriteIdentifier("prototype");
                    WriteToken(".", null);
                    WriteIdentifier("__ctor");
                    Space();
                    WriteToken("=", null);
                    Space();
                    WriteKeyword("function", null);
                    LPar();

                    if (instanceConstructor != null) {
                        StartNode(instanceConstructor);
                        WriteCommaSeparatedList(instanceConstructor.Parameters);
                        EndNode(instanceConstructor);
                    }

                    RPar();
                    OpenBrace(BraceStyle.EndOfLine);

                    if (typeDeclaration.BaseTypes.Count == 1) {
                        var baseReference = typeDeclaration.BaseTypes.FirstOrDefault().Annotation<TypeReference>();
                        WriteIdentifier(baseReference);
                        WriteToken(".", null);
                        WriteIdentifier("prototype");
                        WriteToken(".", null);
                        WriteIdentifier("__ctor");
                        WriteToken(".", null);
                        WriteIdentifier("call");
                        LPar();
                        WriteKeyword("this");
                        RPar();
                        Semicolon();
                    }

                    foreach (var member in typeDeclaration.Members) {
                        if (member is ConstructorDeclaration)
                            continue;
                        else if (member is MethodDeclaration)
                            continue;
                        else if (member is PropertyDeclaration) {
                            EmitPropertyDefault(
                                member, (member as PropertyDeclaration).Annotation<PropertyDefinition>()
                            );
                            continue;
                        } else if (IsStatic(member))
                            continue;

                        member.AcceptVisitor(this, data);
                    }

                    if (instanceConstructor != null) {
                        StartNode(instanceConstructor);
                        instanceConstructor.Body.AcceptVisitor(this, "nobraces");
                        EndNode(instanceConstructor);
                    }

                    CloseBrace(BraceStyle.NextLine);
                    Semicolon();
                }

                foreach (var member in typeDeclaration.Members) {
                    if (member is MethodDeclaration)
                        ;
                    else if (member is ConstructorDeclaration)
                        continue;
                    else if (member is PropertyDeclaration)
                        ;
                    else if (!IsStatic(member))
                        continue;

                    member.AcceptVisitor(this, data);

                    if (isStatic && (member is PropertyDeclaration)) {
                        var propertyDefinition = (member as PropertyDeclaration).Annotation<PropertyDefinition>();
                        if (propertyDefinition != null)
                            EmitPropertyDefault(
                                member, propertyDefinition
                            );
                    }
                }

                if (staticConstructor != null) {
                    StartNode(staticConstructor);
                    staticConstructor.Body.AcceptVisitor(this, "nobraces");
                    EndNode(staticConstructor);
                }

                CloseBrace(BraceStyle.NextLine);

                LPar();
                RPar();

                Space();
                RPar();

                Semicolon();
                NewLine();
            }

            if (typeDeclaration.BaseTypes.Count == 1)
                BaseTypeStack.Pop();

            return EndNode(typeDeclaration);
        }

        protected override void WritePrimitiveValue (object val) {
            if (val == null) {
                WriteKeyword("null");
                return;
            }

            if (val is bool) {
                if ((bool)val) {
                    WriteKeyword("true");
                } else {
                    WriteKeyword("false");
                }
                return;
            }

            if (val is string) {
                formatter.WriteToken("\"" + ConvertString(val.ToString()) + "\"");
                lastWritten = LastWritten.Other;
            } else if (val is char) {
                formatter.WriteToken("\"" + ConvertChar((char)val) + "\"");
                lastWritten = LastWritten.Other;
            } else {
                if ((val is double) || (val is float) || (val is decimal)) {
                    double f = Convert.ToDouble(val);
                    if (double.IsPositiveInfinity(f)) {
                        WriteIdentifier("Number.POSITIVE_INFINITY");
                        return;
                    } else if (double.IsNegativeInfinity(f)) {
                        WriteIdentifier("Number.NEGATIVE_INFINITY");
                        return;
                    } else if (double.IsNaN(f)) {
                        WriteIdentifier("Number.NaN");
                        return;
                    } else {
                        // Use numeric round-trip format to preserve accuracy
                        formatter.WriteToken(f.ToString("R"));
                        lastWritten = LastWritten.Other;
                        return;
                    }
                } else if (val is IFormattable) {
                    StringBuilder b = new StringBuilder();
                    b.Append(((IFormattable)val).ToString(null, NumberFormatInfo.InvariantInfo));
                    formatter.WriteToken(b.ToString());
                } else {
                    formatter.WriteToken(val.ToString());
                }

                lastWritten = LastWritten.Other;
            }
        }

        protected bool TypeDerivesFrom (TypeReference haystack, string needleFullName) {
            while (haystack != null) {
                if (haystack.FullName == needleFullName)
                    return true;

                haystack = haystack.Resolve().BaseType;
            }

            return false;
        }

        public override object VisitObjectCreateExpression (ObjectCreateExpression objectCreateExpression, object data) {
            StartNode(objectCreateExpression);

            var objectType = objectCreateExpression.Type;
            var typeReference = objectType.Annotation<TypeReference>();

            if (TypeDerivesFrom(typeReference, "System.Delegate")) {
                WriteIdentifier("JSIL.Delegate.New");
                LPar();

                var target = objectCreateExpression.Arguments
                    .FirstOrDefault() as MemberReferenceExpression;
                if (target == null)
                    throw new NotImplementedException("This type of delegate construction is not implemented: " + objectCreateExpression.ToString());

                WritePrimitiveValue(typeReference.FullName);
                WriteToken(",", null);
                Space();

                StartNode(target);
                target.Target.AcceptVisitor(this, data);
                EndNode(target);

                WriteToken(",", null);
                Space();

                target.AcceptVisitor(this, data);

                RPar();
                return EndNode(objectCreateExpression);
            }

            WriteKeyword("new");
            objectCreateExpression.Type.AcceptVisitor(this, data);
            LPar();

            WriteCommaSeparatedList(objectCreateExpression.Arguments);

            RPar();

            objectCreateExpression.Initializer.AcceptVisitor(this, data);

            return EndNode(objectCreateExpression);
        }

        public override object VisitArrayCreateExpression (ArrayCreateExpression arrayCreateExpression, object data) {
            StartNode(arrayCreateExpression);

            if (!arrayCreateExpression.Initializer.IsNull) {
                WriteToken("[", null);

                StartNode(arrayCreateExpression.Initializer);
                WriteCommaSeparatedList(arrayCreateExpression.Initializer.Elements);
                EndNode(arrayCreateExpression.Initializer);

                WriteToken("]", null);
            } else {
                WriteIdentifier("JSIL.Array.New");
                LPar();

                arrayCreateExpression.Type.AcceptVisitor(this, null);
                WriteToken(",", null);
                Space();

                if (arrayCreateExpression.Arguments.Count > 1)
                    throw new NotImplementedException("Multidimensional arrays are not supported");
                else if (arrayCreateExpression.Arguments.Count > 0)
                    WriteCommaSeparatedList(arrayCreateExpression.Arguments);

                RPar();
            }

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

        public override object VisitIdentifierExpression (IdentifierExpression identifierExpression, object data) {
            StartNode(identifierExpression);
            var mi = identifierExpression as ModifiedIdentifierExpression;

            if (mi != null) {
                WriteIdentifier(Util.EscapeIdentifier(mi.Identifier));
                WriteToken(".", null);
                WriteIdentifier("value");
            } else
                WriteIdentifier(Util.EscapeIdentifier(identifierExpression.Identifier));

            return EndNode(identifierExpression);
        }

        public override object VisitMemberReferenceExpression (MemberReferenceExpression memberReferenceExpression, object data) {
            StartNode(memberReferenceExpression);
            memberReferenceExpression.Target.AcceptVisitor(this, data);
            WriteToken(".", MemberReferenceExpression.Roles.Dot);
            WriteIdentifier(memberReferenceExpression);
            return EndNode(memberReferenceExpression);
        }

        public override object VisitDirectionExpression (DirectionExpression directionExpression, object data) {
            StartNode(directionExpression);

            directionExpression.Expression.AcceptVisitor(this, data);

            return EndNode(directionExpression);
        }

        public override object VisitParameterDeclaration (ParameterDeclaration parameterDeclaration, object data) {
            StartNode(parameterDeclaration);
            switch (parameterDeclaration.ParameterModifier) {
                case ParameterModifier.Params:
                case ParameterModifier.This:
                    throw new NotImplementedException(
                        "Parameter modifier not supported: " + parameterDeclaration.ParameterModifier.ToString()
                    );
                case ParameterModifier.None:
                    break;
                default:
                    WriteToken("/* ", null);
                    WriteIdentifier(parameterDeclaration.ParameterModifier.ToString());
                    WriteToken(" */", null);
                    Space();
                    break;
            }

            if (!string.IsNullOrEmpty(parameterDeclaration.Name))
                WriteIdentifier(parameterDeclaration.Name);

            if (!parameterDeclaration.DefaultExpression.IsNull) {
                throw new NotImplementedException(
                    "Default argument values not supported"
                );
            }

            return EndNode(parameterDeclaration);
        }

        protected bool VisitVariableInitializer (VariableInitializer variableInitializer) {
            bool result = false;
            bool isField = variableInitializer.Parent is FieldDeclaration;
            bool isNull = variableInitializer.Initializer.IsNull;
            Expression fakeInitializer = null;

            if (isNull && isField) {
                var fieldRef = variableInitializer.Parent.Annotation<FieldReference>();
                if (fieldRef.FieldType.IsPrimitive) {
                    isNull = false;
                    fakeInitializer = AstMethodBodyBuilder.MakeDefaultValue(fieldRef.FieldType);
                }
            }

            var mvi = variableInitializer as ModifiedVariableInitializer;

            if (!isNull) {
                if (isField) {
                    var fieldRef = variableInitializer.Parent.Annotation<FieldReference>();
                    WriteThisReference(fieldRef.DeclaringType.Resolve(), fieldRef);
                    WriteToken(".", null);
                }

                WriteIdentifier(Util.EscapeIdentifier(variableInitializer.Name));
                Space();
                WriteToken("=", VariableInitializer.Roles.Assign);
                Space();

                if (mvi != null) {
                    WriteKeyword("new");
                    Space();
                    WriteIdentifier("JSIL.Variable");
                    LPar();
                }

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

                if (mvi != null)
                    RPar();
                
                result = true;
            } else if (!isField) {
                WriteIdentifier(Util.EscapeIdentifier(variableInitializer.Name));
            }

            return result;
        }

        public override object VisitVariableInitializer (VariableInitializer variableInitializer, object data) {
            StartNode(variableInitializer);

            VisitVariableInitializer(variableInitializer);

            return EndNode(variableInitializer);
        }

        public override object VisitBaseReferenceExpression (BaseReferenceExpression baseReferenceExpression, object data) {
            StartNode(baseReferenceExpression);
            var baseTypeName = BaseTypeStack.Peek();
            WriteIdentifier(Util.EscapeIdentifier(baseTypeName, false));
            WriteToken(".", null);
            WriteIdentifier("prototype");
            return EndNode(baseReferenceExpression);
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
            if (IsIgnored(fieldDeclaration.Attributes))
                return null;

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
            if (IsIgnored(propertyDeclaration.Attributes))
                return null;

            StartNode(propertyDeclaration);

            var rawValue = propertyDeclaration.Annotation<PrimitiveExpression>();
            if (rawValue != null) {
                // If the property is annotative with a primitive expression, it was replaced with JSReplacement
                var originalMethod = propertyDeclaration.Annotation<MethodDeclaration>();
                WriteIdentifier(originalMethod.Annotation<MethodDefinition>().DeclaringType);

                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(propertyDeclaration.Name));

                Space();
                WriteToken("=", null);
                Space();

                WriteIdentifier(rawValue.Value as string, null);
                Semicolon();

                return EndNode(propertyDeclaration);
            }

            var propertyDefinition = propertyDeclaration.Annotation<PropertyDefinition>();
            var declaringType = propertyDefinition.DeclaringType;
            bool isStatic = propertyDefinition.GetMethod.IsStatic || propertyDefinition.SetMethod.IsStatic;
            bool isAutoProperty = propertyDefinition.GetMethod.CustomAttributes.Concat(
                    propertyDefinition.SetMethod.CustomAttributes
                ).Where((ca) => ca.AttributeType.Name == "CompilerGeneratedAttribute")
                .Count() > 0;

            // Generate the accessor methods
            foreach (AstNode node in propertyDeclaration.Children) {
                if (node.Role == IndexerDeclaration.GetterRole || node.Role == IndexerDeclaration.SetterRole) {
                    node.AcceptVisitor(this, data);
                    Semicolon();
                }
            }

            // Now generate the property definition
            WriteIdentifier("Object");
            WriteToken(".", null);
            WriteIdentifier("defineProperty");
            LPar();

            if (isStatic) {
                WriteIdentifier(declaringType);
            } else {
                WriteIdentifier(declaringType);
                WriteToken(".", null);
                WriteIdentifier("prototype");
            }

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
                        NewLine();
                    }
                    first = false;

                    var accessor = node as Accessor;
                    string prefix;
                    if (node.Role == IndexerDeclaration.GetterRole)
                        prefix = "get";
                    else if (node.Role == IndexerDeclaration.SetterRole)
                        prefix = "set";
                    else
                        throw new InvalidOperationException();

                    WriteIdentifier(prefix);
                    WriteToken(":", null);
                    Space();

                    if (isStatic) {
                        WriteIdentifier(declaringType);
                    } else {
                        WriteIdentifier(declaringType);
                        WriteToken(".", null);
                        WriteIdentifier("prototype");
                    }
                    WriteToken(".", null);
                    WriteIdentifier(String.Format(
                        "{0}_{1}", prefix, propertyDeclaration.Name
                    ));
                }
            }

            CloseBrace(BraceStyle.NextLine);

            RPar();
            Semicolon();

            return EndNode(propertyDeclaration);
        }

        protected object EmitPropertyDefault (AttributedNode node, PropertyDefinition propertyDefinition) {
            if (IsIgnored(node.Attributes))
                return null;

            StartNode(node);

            var declaringType = propertyDefinition.DeclaringType;
            bool isStatic = propertyDefinition.GetMethod.IsStatic || propertyDefinition.SetMethod.IsStatic;
            bool isAutoProperty = propertyDefinition.GetMethod.CustomAttributes.Concat(
                    propertyDefinition.SetMethod.CustomAttributes
                ).Where((ca) => ca.AttributeType.Name == "CompilerGeneratedAttribute")
                .Count() > 0;

            // If the property is of a primitive type, we must assign it a default value so it's not undefined
            if (propertyDefinition.PropertyType.IsPrimitive && isAutoProperty) {
                if (isStatic)
                    WriteIdentifier(declaringType);
                else
                    WriteKeyword("this");

                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(propertyDefinition.Name));

                Space();
                WriteToken("=", null);
                Space();

                WritePrimitiveValue(AstMethodBodyBuilder.MakeDefaultValue(propertyDefinition.PropertyType));
                Semicolon();
            }

            return EndNode(node);
        }

        public override object VisitAccessor (Accessor accessor, object data) {
            StartNode(accessor);
            string prefix;

            if (accessor.Role == PropertyDeclaration.GetterRole) {
                prefix = "get";
            } else if (accessor.Role == PropertyDeclaration.SetterRole) {
                prefix = "set";
            } else {
                throw new NotImplementedException();
            }

            var propertyDefinition = accessor.Parent.Annotation<PropertyDefinition>();
            var methodName = String.Format("{0}_{1}", prefix, propertyDefinition.Name);
            var declaringType = propertyDefinition.DeclaringType;
            var storageName = Util.EscapeIdentifier(
                String.Format("{0}.value", propertyDefinition.Name)
            );
            bool isStatic = propertyDefinition.GetMethod.IsStatic || propertyDefinition.SetMethod.IsStatic;

            if (!isStatic) {
                WriteIdentifier(declaringType);
                WriteToken(".", null);
                WriteIdentifier("prototype");
            } else {
                WriteThisReference(
                    declaringType, isStatic
                );
            }

            WriteToken(".", null);
            WriteIdentifier(methodName);

            Space();
            WriteToken("=", null);
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
            WriteIdentifier(simpleType.Annotation<TypeReference>());
            return EndNode(simpleType);
        }

        public override object VisitVariableDeclarationStatement (VariableDeclarationStatement variableDeclarationStatement, object data) {
            StartNode(variableDeclarationStatement);

            WriteKeyword("let");

            Space();
            WriteCommaSeparatedList(variableDeclarationStatement.Variables);
            Semicolon();
            return EndNode(variableDeclarationStatement);
        }

        public override object VisitConstructorDeclaration (ConstructorDeclaration constructorDeclaration, object data) {
            throw new InvalidOperationException("Constructors should have been processed by the type definition explicitly");
        }

        protected void WriteThisReference (TypeDefinition declaringType, FieldReference field) {
            WriteThisReference(
                declaringType,
                field.Resolve().Attributes.HasFlag(FieldAttributes.Static)
            );
        }

        protected void WriteThisReference (TypeDefinition declaringType, bool isStatic) {
            if (isStatic)
                WriteIdentifier(declaringType);
            else
                WriteKeyword("this");
        }

        public override object VisitAnonymousMethodExpression (AnonymousMethodExpression anonymousMethodExpression, object data) {
            StartNode(anonymousMethodExpression);
            WriteKeyword("function");
            Space();
            LPar();

            if (anonymousMethodExpression.HasParameterList)
                WriteCommaSeparatedList(anonymousMethodExpression.Parameters);

            RPar();

            anonymousMethodExpression.Body.AcceptVisitor(this, data);

            return EndNode(anonymousMethodExpression);
        }

        protected string PickOperatorName (OperatorDeclaration op) {
            return String.Format("op_{0}", op.OperatorType);
        }

        public override object VisitOperatorDeclaration (OperatorDeclaration operatorDeclaration, object data) {
            if (IsIgnored(operatorDeclaration.Attributes))
                return null;

            StartNode(operatorDeclaration);

            var declaringType = operatorDeclaration.Annotation<MethodDefinition>().DeclaringType;

            if (!IsStatic(operatorDeclaration)) {
                WriteIdentifier(declaringType);
                WriteToken(".", null);
                WriteIdentifier("prototype");
            } else {
                WriteThisReference(
                    declaringType, true
                );
            }

            WriteToken(".", null);
            WriteIdentifier(operatorDeclaration);
            Space();
            WriteToken("=", null);
            Space();
            WriteKeyword("function");
            Space();

            WriteCommaSeparatedListInParenthesis(operatorDeclaration.Parameters, true);

            if (!operatorDeclaration.Body.IsNull) {
                VisitBlockStatement(operatorDeclaration.Body, "method");
            } else {
                OpenBrace(BraceStyle.EndOfLine);
                CloseBrace(BraceStyle.NextLine);
            }

            Semicolon();

            return EndNode(operatorDeclaration);
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            if (IsIgnored(methodDeclaration.Attributes))
                return null;

            StartNode(methodDeclaration);

            var declaringType = methodDeclaration.Annotation<MethodDefinition>().DeclaringType;

            if (!IsStatic(methodDeclaration)) {
                WriteIdentifier(declaringType);
                WriteToken(".", null);
                WriteIdentifier("prototype");
            } else {
                WriteThisReference(
                    declaringType, true
                );
            }

            WriteToken(".", null);
            WriteIdentifier(methodDeclaration);
            Space();
            WriteToken("=", null);
            Space();
            WriteKeyword("function");
            Space();

            WriteCommaSeparatedListInParenthesis(methodDeclaration.Parameters, true);

            if (!methodDeclaration.Body.IsNull) {
                VisitBlockStatement(methodDeclaration.Body, "method");
            } else {
                OpenBrace(BraceStyle.EndOfLine);
                CloseBrace(BraceStyle.NextLine);
            }

            Semicolon();

            return EndNode(methodDeclaration);
        }

        public static string GetLINQOperatorByName (string name) {
            var enumType = typeof(ExpressionType);
            var value = (ExpressionType)Enum.Parse(enumType, name, true);

            switch (value) {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.RightShift:
                    return ">>";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Assign:
                    return "=";
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                    return "+=";
                case ExpressionType.AndAssign:
                    return "&=";
                case ExpressionType.DivideAssign:
                    return "/=";
                case ExpressionType.ExclusiveOrAssign:
                    return "^=";
                case ExpressionType.LeftShiftAssign:
                    return "<<=";
                case ExpressionType.ModuloAssign:
                    return "%=";
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.MultiplyAssign:
                    return "*=";
                case ExpressionType.OrAssign:
                    return "|=";
                case ExpressionType.PowerAssign:
                    return "**=";
                case ExpressionType.RightShiftAssign:
                    return ">>=";
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                    return "-=";
            }

            throw new NotImplementedException("Operator " + name + " not implemented.");
        }

        public override object VisitCastExpression (CastExpression castExpression, object data) {
            StartNode(castExpression);

            WriteIdentifier("JSIL.Cast");
            LPar();
            castExpression.Expression.AcceptVisitor(this, null);
            WriteToken(",", null);
            Space();
            castExpression.Type.AcceptVisitor(this, null);
            RPar();

            return EndNode(castExpression);
        }

        public object VisitDynamicExpression (DynamicExpression dynamicExpression, object data) {
            StartNode(dynamicExpression);

            switch (dynamicExpression.CallSiteType) {
                case CallSiteType.GetMember:
                    dynamicExpression.Target.AcceptVisitor(this, null);
                    WriteToken(".", null);
                    WriteIdentifier(Util.EscapeIdentifier(dynamicExpression.MemberName));
                    break;
                case CallSiteType.SetMember:
                    dynamicExpression.Target.AcceptVisitor(this, null);
                    WriteToken(".", null);
                    WriteIdentifier(Util.EscapeIdentifier(dynamicExpression.MemberName));
                    Space();
                    WriteToken("=", null);
                    Space();
                    WriteCommaSeparatedList(dynamicExpression.Arguments);
                    break;
                case CallSiteType.InvokeMember:
                    dynamicExpression.Target.AcceptVisitor(this, null);
                    WriteToken(".", null);
                    WriteIdentifier(Util.EscapeIdentifier(dynamicExpression.MemberName));
                    LPar();
                    WriteCommaSeparatedList(dynamicExpression.Arguments);
                    RPar();
                    break;
                case CallSiteType.Convert:
                    WriteIdentifier("JSIL.Dynamic.Cast");                    
                    LPar();
                    dynamicExpression.Target.AcceptVisitor(this, null);
                    WriteToken(",", null);
                    Space();
                    dynamicExpression.TargetType.AcceptVisitor(this, null);
                    RPar();
                    break;
                case CallSiteType.BinaryOperator:
                    dynamicExpression.Target.AcceptVisitor(this, null);
                    Space();
                    WriteToken(GetLINQOperatorByName(dynamicExpression.MemberName), null);
                    Space();
                    dynamicExpression.Arguments.First().AcceptVisitor(this, null);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return EndNode(dynamicExpression);
        }

        public object VisitTargetedBreakStatement (TargetedBreakStatement targetedBreakStatement, object data) {
            StartNode(targetedBreakStatement);
            WriteKeyword("break");
            Space();
            WriteIdentifier(targetedBreakStatement.LabelName);
            Semicolon();
            return EndNode(targetedBreakStatement);
        }

        public object VisitTargetedContinueStatement (TargetedContinueStatement targetedContinueStatement, object data) {
            StartNode(targetedContinueStatement);
            WriteKeyword("continue");
            Space();
            WriteIdentifier(targetedContinueStatement.LabelName);
            Semicolon();
            return EndNode(targetedContinueStatement);
        }

        public override object VisitCatchClause (CatchClause catchClause, object data) {
            StartNode(catchClause);
            Space();
            WriteKeyword("catch");
            Space();

            if (!string.IsNullOrEmpty(catchClause.VariableName)) {
                LPar();
                WriteIdentifier(Util.EscapeIdentifier(catchClause.VariableName));
                RPar();
            }

            catchClause.Body.AcceptVisitor(this, data);
            return EndNode(catchClause);
        }

        public override object VisitForeachStatement (ForeachStatement foreachStatement, object data) {
            StartNode(foreachStatement);

            var fieldReference = foreachStatement.InExpression.Annotation<FieldReference>();
            var propertyReference = foreachStatement.InExpression.Annotation<PropertyReference>();

            TypeReference sequenceType = null;
            if (fieldReference != null)
                sequenceType = fieldReference.FieldType;
            else if (propertyReference != null)
                sequenceType = propertyReference.PropertyType;

            if (sequenceType.IsArray) {
                Debugger.Break();
            }

            WriteKeyword("foreach");
            Space();
            LPar();
            WriteIdentifier(foreachStatement.VariableName);
            WriteKeyword("in", ForeachStatement.Roles.InKeyword);
            Space();
            foreachStatement.InExpression.AcceptVisitor(this, data);
            RPar();
            foreachStatement.EmbeddedStatement.AcceptVisitor(this, data);
            Semicolon();
            return EndNode(foreachStatement);
        }
    }
}
