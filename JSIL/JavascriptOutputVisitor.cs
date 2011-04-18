using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Transforms;
using Mono.Cecil;
using JSIL.Expressions;

using ExpressionType = System.Linq.Expressions.ExpressionType;

namespace JSIL.Internal {
    public class JavascriptOutputVisitor 
        : OutputVisitor, 
          IDynamicExpressionVisitor<object, object>,
          ITargetedControlFlowVisitor<object, object>
    {
        public readonly Dictionary<string, List<OverloadedMethodDeclaration>> KnownOverloads = new Dictionary<string, List<OverloadedMethodDeclaration>>();
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
            if (type == null)
                throw new ArgumentNullException("type", "Type Reference must not be null");

            WriteIdentifier(Util.EscapeIdentifier(
                type.FullName,
                escapePeriods: false
            ));
        }

        protected string GenerateName (EventDeclaration evt) {
            return "_" + evt.Annotation<EventDefinition>().Name;
        }

        protected string GenerateName (MethodDeclaration method) {
            var omd = method as OverloadedMethodDeclaration;

            if (omd != null)
                return String.Format("{0}_{1}", omd.Name, omd.OverloadIndex);
            else
                return method.Name;
        }

        protected void WriteIdentifier (MethodDeclaration method) {
            WriteIdentifier(Util.EscapeIdentifier(GenerateName(method)));
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

        public override object VisitAttributeSection (AttributeSection attributeSection, object data) {
            return null;
        }

        public override object VisitAttribute (ICSharpCode.NRefactory.CSharp.Attribute attribute, object data) {
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

        protected bool IsStatic (PropertyDefinition property) {
            if ((property.GetMethod != null) && (property.GetMethod.IsStatic))
                return true;
            if ((property.SetMethod != null) && (property.SetMethod.IsStatic))
                return true;

            return false;
        }

        protected bool IsStatic (EventDefinition property) {
            if ((property.AddMethod != null) && (property.AddMethod.IsStatic))
                return true;
            if ((property.RemoveMethod != null) && (property.RemoveMethod.IsStatic))
                return true;

            return false;
        }

        protected bool IsStatic (AttributedNode node) {
            if (node.Modifiers.HasFlag(Modifiers.Static)) {
                return true;
            } else if (
                 node is FieldDeclaration &&
                 node.Annotation<FieldReference>().Resolve()
                     .Attributes.HasFlag(FieldAttributes.Static)
             ) {
                return true;
            }

            return false;
        }

        protected bool EmitInSecondPass (AttributedNode node) {
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
            } else if (node is EventDeclaration) {
                return false;
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
            var mr = invocationExpression.Annotation<MethodReference>();
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

                if (mr != null) {
                    List<OverloadedMethodDeclaration> overloads;
                    if (KnownOverloads.TryGetValue(mre.MemberName, out overloads)) {
                        foreach (var omd in overloads) {
                            var omr = omd.Annotation<MethodReference>();

                            if (omr == mr) {
                                mre.MemberName = GenerateName(omd);
                                break;
                            }
                        }
                    }
                }
            }

            return base.VisitInvocationExpression(invocationExpression, data);
        }

        protected IEnumerable<ConstructorDeclaration> GetConstructors (TypeDeclaration typeDeclaration) {
            return (from member in typeDeclaration.Members
                    where member is ConstructorDeclaration
                    select (ConstructorDeclaration)member);
        }

        protected void EmitInitialTypeDeclaration (TypeDeclaration typeDeclaration) {
            bool isStatic = typeDeclaration.Modifiers.HasFlag(Modifiers.Static);
            var typeReference = typeDeclaration.Annotation<TypeReference>();

            var constructors = GetConstructors(typeDeclaration).ToArray();

            var instanceConstructors = (from constructor in constructors
                                        where !constructor.Modifiers.HasFlag(Modifiers.Static)
                                        select constructor);
            var instanceConstructor = instanceConstructors.FirstOrDefault();

            if (!(typeDeclaration.Parent is TypeDeclaration) && (!typeReference.FullName.Contains("."))) {
                WriteKeyword("var");
                Space();
            }

            WriteIdentifier(typeReference);
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
                WriteIdentifier("__ctor");
                WriteToken(".", null);
                WriteIdentifier("apply");
                LPar();
                WriteKeyword("this");
                WriteToken(",", null);
                Space();
                WriteKeyword("arguments");
                RPar();
                Semicolon();
            }

            CloseBrace(BraceStyle.NextLine);
            Semicolon();
            NewLine();
        }

        class InitialTypeDeclarer : DepthFirstAstVisitor<object, object> {
            public readonly JavascriptOutputVisitor Parent;

            public InitialTypeDeclarer (JavascriptOutputVisitor parent) {
                Parent = parent;
            }

            public override object VisitNamespaceDeclaration (NamespaceDeclaration namespaceDeclaration, object data) {
                Parent.StartNode(namespaceDeclaration);

                var name = Util.EscapeIdentifier(namespaceDeclaration.FullName);

                if (!name.Contains(".")) {
                    Parent.formatter.WriteKeyword("var");
                    Parent.formatter.Space();
                }

                Parent.formatter.WriteIdentifier(name);
                Parent.formatter.Space();
                Parent.formatter.WriteToken("=");
                Parent.formatter.Space();
                Parent.formatter.WriteToken("{};");
                Parent.formatter.NewLine();

                base.VisitNamespaceDeclaration(namespaceDeclaration, data);

                return Parent.EndNode(namespaceDeclaration);
            }

            public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
                if (Parent.IsIgnored(typeDeclaration.Attributes))
                    return null;

                Parent.StartNode(typeDeclaration);

                Parent.EmitInitialTypeDeclaration(typeDeclaration);

                base.VisitTypeDeclaration(typeDeclaration, data);

                return Parent.EndNode(typeDeclaration);
            }

            public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
                var odmd = methodDeclaration as OverloadDispatcherMethodDeclaration;
                if (odmd != null) {
                    List<OverloadedMethodDeclaration> overloads;
                    if (!Parent.KnownOverloads.TryGetValue(methodDeclaration.Name, out overloads)) {
                        overloads = new List<OverloadedMethodDeclaration>();
                        Parent.KnownOverloads[methodDeclaration.Name] = overloads;
                    }

                    overloads.AddRange(odmd.Overloads);
                }

                return base.VisitMethodDeclaration(methodDeclaration, data);
            }
        }

        public override object VisitCompilationUnit (CompilationUnit compilationUnit, object data) {
            var declarer = new InitialTypeDeclarer(this);
            compilationUnit.AcceptVisitor(declarer, null);

            return base.VisitCompilationUnit(compilationUnit, data);
        }

        public override object VisitNamespaceDeclaration (NamespaceDeclaration namespaceDeclaration, object data) {
            StartNode(namespaceDeclaration);

            foreach (var member in namespaceDeclaration.Members)
                member.AcceptVisitor(this, data);

            return EndNode(namespaceDeclaration);
        }

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            if (IsIgnored(typeDeclaration.Attributes))
                return null;

            bool isStatic = typeDeclaration.Modifiers.HasFlag(Modifiers.Static);

            var constructors = GetConstructors(typeDeclaration).ToArray();

            var instanceConstructors = (from constructor in constructors
                                       where !constructor.Modifiers.HasFlag(Modifiers.Static)
                                       select constructor).ToArray();
            var instanceConstructor = instanceConstructors.FirstOrDefault();

            var staticConstructor = (from constructor in constructors
                                      where constructor.Modifiers.HasFlag(Modifiers.Static)
                                      select constructor).FirstOrDefault();

            StartNode(typeDeclaration);

            var typeReference = typeDeclaration.Annotation<TypeReference>();

            if (true) {
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

                    WriteIdentifier(typeReference);
                    WriteToken(".", null);
                    WriteKeyword("prototype");
                    Space();
                    WriteToken("=", null);
                    Space();
                    WriteIdentifier("JSIL.MakeProto");
                    LPar();
                    WriteIdentifier(baseClass);
                    WriteToken(",", null);
                    Space();
                    WritePrimitiveValue(typeReference.ToString());
                    RPar();
                    Semicolon();
                }

                if (!isStatic) {
                    WriteIdentifier(typeReference);
                    WriteToken(".", null);
                    WriteIdentifier("prototype");
                    WriteToken(".", null);
                    WriteIdentifier("__ctor");
                    Space();
                    WriteToken("=", null);
                    Space();
                    WriteKeyword("function", null);
                    Space();
                    LPar();

                    if (instanceConstructors.Length == 1) {
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
                        } else if (EmitInSecondPass(member))
                            continue;

                        member.AcceptVisitor(this, data);
                    }

                    var temporaryRole = new Role<AstType>("temporary");
                    if (instanceConstructors.Length > 1) {
                        WriteKeyword("var", null);
                        Space();
                        WriteIdentifier("__ctors");
                        Space();
                        WriteToken("=", null);
                        Space();
                        WriteToken("[", null);
                        formatter.Indent();
                        NewLine();

                        for (int i = 0; i < instanceConstructors.Length; i++) {
                            var ic = instanceConstructors[i];
                            if (i != 0) {
                                WriteToken(",", null);
                                NewLine();
                            }

                            WriteToken("[", null);
                            WriteKeyword("function");
                            Space();

                            StartNode(ic);

                            LPar();
                            WriteCommaSeparatedList(ic.Parameters);
                            RPar();

                            OpenBrace(BraceStyle.EndOfLine);

                            ic.Body.AcceptVisitor(this, "nobraces");

                            CloseBrace(BraceStyle.EndOfLine);
                            WriteToken(",", null);
                            Space();

                            WriteToken("[", null);

                            bool isFirst = true;
                            foreach (var parameter in ic.Parameters) {
                                if (!isFirst) {
                                    WriteToken(",", null);
                                    Space();
                                }

                                var type = (AstType)parameter.Type.Clone();
                                ic.AddChild(type, temporaryRole);
                                type.AcceptVisitor(this, null);
                                isFirst = false;
                            }

                            WriteToken("]", null);

                            EndNode(ic);

                            WriteToken("]", null);
                        }

                        formatter.Unindent();
                        NewLine();
                        WriteToken("]", null);
                        Semicolon();

                        WriteKeyword("return");
                        Space();
                        WriteIdentifier("JSIL.DispatchOverload.call");
                        LPar();

                        WriteKeyword("this");
                        WriteToken(",", null);
                        Space();

                        WriteIdentifier("Array.prototype.slice.call");
                        LPar();
                        WriteKeyword("arguments");
                        RPar();

                        WriteToken(",", null);
                        Space();

                        WriteIdentifier("__ctors");
                        RPar();
                        Semicolon();
                    } else if (instanceConstructors.Length == 1) {
                        StartNode(instanceConstructor);
                        instanceConstructor.Body.AcceptVisitor(this, "nobraces");
                        EndNode(instanceConstructor);
                    }

                    CloseBrace(BraceStyle.NextLine);
                    Semicolon();
                }

                foreach (var member in typeDeclaration.Members) {
                    if (member is MethodDeclaration || member is PropertyDeclaration)
                        ;
                    else if (member is ConstructorDeclaration)
                        continue;
                    else if (member is EventDeclaration) {
                        EmitEventMethods(
                            member, (member as EventDeclaration).Annotation<EventDefinition>()
                        );
                        continue;
                    } else if (!EmitInSecondPass(member))
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

                NewLine();
            }

            if (typeDeclaration.BaseTypes.Count == 1)
                BaseTypeStack.Pop();

            WriteIdentifier("Object.seal");
            LPar();
            WriteIdentifier(typeReference);
            RPar();
            Semicolon();

            if (!isStatic) {
                WriteIdentifier("Object.seal");
                LPar();
                WriteIdentifier(typeReference);
                WriteToken(".", null);
                WriteIdentifier("prototype");
                RPar();
                Semicolon();
            }

            NewLine();
             
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

        protected void EmitNewExpression (AstType objectType, IEnumerable<Expression> arguments = null, Expression initializer = null) {
            var typeReference = objectType.Annotation<TypeReference>();

            if (TypeDerivesFrom(typeReference, "System.Delegate") && (arguments != null)) {
                WriteIdentifier("System.Delegate.New");
                LPar();

                var target = arguments
                    .FirstOrDefault() as MemberReferenceExpression;
                if (target == null)
                    throw new NotImplementedException("This type of delegate construction is not implemented: " + typeReference.ToString());

                WritePrimitiveValue(typeReference.FullName);
                WriteToken(",", null);
                Space();

                StartNode(target);
                target.Target.AcceptVisitor(this, null);
                EndNode(target);

                WriteToken(",", null);
                Space();

                target.AcceptVisitor(this, null);

                RPar();
                return;
            }

            WriteKeyword("new");
            objectType.AcceptVisitor(this, null);
            LPar();

            if (arguments != null)
                WriteCommaSeparatedList(arguments);

            RPar();

            if (initializer != null)
                initializer.AcceptVisitor(this, null);
        }

        public override object VisitObjectCreateExpression (ObjectCreateExpression objectCreateExpression, object data) {
            StartNode(objectCreateExpression);

            EmitNewExpression(objectCreateExpression.Type, objectCreateExpression.Arguments, objectCreateExpression.Initializer);

            return EndNode(objectCreateExpression);
        }

        public override object VisitArrayCreateExpression (ArrayCreateExpression arrayCreateExpression, object data) {
            StartNode(arrayCreateExpression);

            WriteIdentifier(
                (arrayCreateExpression.Arguments.Count > 1) ? "JSIL.JaggedArray.New" : "JSIL.Array.New"
            );
            LPar();

            arrayCreateExpression.Type.AcceptVisitor(this, null);
            WriteToken(",", null);
            Space();

            if (!arrayCreateExpression.Initializer.IsNull) {
                WriteToken("[", null);

                StartNode(arrayCreateExpression.Initializer);
                WriteCommaSeparatedList(arrayCreateExpression.Initializer.Elements);
                EndNode(arrayCreateExpression.Initializer);

                WriteToken("]", null);
            } else {
                WriteCommaSeparatedList(arrayCreateExpression.Arguments);
            }

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

        public override object VisitMemberType (MemberType memberType, object data) {
            StartNode(memberType);
            WriteIdentifier(memberType.Annotation<TypeReference>());
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

        public override object VisitDefaultValueExpression (ICSharpCode.NRefactory.CSharp.DefaultValueExpression defaultValueExpression, object data) {
            StartNode(defaultValueExpression);

            EmitNewExpression(defaultValueExpression.Type);

            return EndNode(defaultValueExpression);
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
            TypeReference variableType = null;

            if (isField) {
                var fieldRef = variableInitializer.Parent.Annotation<FieldReference>();
                variableType = fieldRef.FieldType;
                if (isNull) {
                    isNull = false;
                    fakeInitializer = AstMethodBodyBuilder.MakeDefaultValue(fieldRef.FieldType);
                }
            } else if (variableInitializer.Parent is VariableDeclarationStatement) {
                var vds = (VariableDeclarationStatement)variableInitializer.Parent;
                variableType = vds.Type.Annotation<TypeReference>();
            } else {
                Debugger.Break();
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
                    variableInitializer.AddChild(fakeInitializer, new Role<Expression>("temporary"));
                    fakeInitializer.AcceptVisitor(this, null);
                } else {
                    if (TypeIsDelegate(variableType) && 
                        (variableInitializer.Initializer is AnonymousMethodExpression)
                    ) {
                        WriteIdentifier("System.Delegate.New");
                        LPar();
                        WritePrimitiveValue(variableType.FullName);
                        WriteToken(",", null);
                        Space();
                        variableInitializer.Initializer.AcceptVisitor(this, null);
                        RPar();
                    } else {
                        variableInitializer.Initializer.AcceptVisitor(this, null);
                    }
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

            // Important not to pass on the value of 'data', since if we got 'nobraces' or 'method'
            //  that shouldn't recurse
            foreach (var node in blockStatement.Statements)
                node.AcceptVisitor(this, null);

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

        public override object VisitEventDeclaration (EventDeclaration eventDeclaration, object data) {
            if (IsIgnored(eventDeclaration.Attributes))
                return null;

            StartNode(eventDeclaration);

            var eventDefinition = eventDeclaration.Annotation<EventDefinition>();
            var eventField = eventDeclaration.Annotation<FieldDefinition>();
            var declaringType = eventDefinition.DeclaringType;
            bool isStatic = IsStatic(eventDefinition);

            WriteThisReference(declaringType, isStatic);

            WriteToken(".", null);
            WriteIdentifier(Util.EscapeIdentifier(eventDefinition.Name));
            Space();
            WriteToken("=", null);
            Space();

            WriteKeyword("null");
            Semicolon();

            return EndNode(eventDeclaration);
        }

        public override object VisitPropertyDeclaration (PropertyDeclaration propertyDeclaration, object data) {
            if (IsIgnored(propertyDeclaration.Attributes))
                return null;

            StartNode(propertyDeclaration);

            var rawValue = propertyDeclaration.Annotation<PrimitiveExpression>();
            if (rawValue != null) {
                // If the property is annotated with a primitive expression, it was replaced with JSReplacement
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
            bool isStatic = IsStatic(propertyDefinition);

            IEnumerable<CustomAttribute> cas = new CustomAttribute[0];
            if (propertyDefinition.GetMethod != null)
                cas = cas.Concat(propertyDefinition.GetMethod.CustomAttributes);
            if (propertyDefinition.SetMethod != null)
                cas = cas.Concat(propertyDefinition.SetMethod.CustomAttributes);
            bool isAutoProperty = cas.Where((ca) => ca.AttributeType.Name == "CompilerGeneratedAttribute").Count() > 0;

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
            bool isStatic = IsStatic(propertyDefinition);

            IEnumerable<CustomAttribute> cas = new CustomAttribute[0];
            if (propertyDefinition.GetMethod != null)
                cas = cas.Concat(propertyDefinition.GetMethod.CustomAttributes);
            if (propertyDefinition.SetMethod != null)
                cas = cas.Concat(propertyDefinition.SetMethod.CustomAttributes);
            bool isAutoProperty = cas.Where((ca) => ca.AttributeType.Name == "CompilerGeneratedAttribute").Count() > 0;

            bool isValueType = (propertyDefinition.PropertyType.IsPrimitive) || 
                (propertyDefinition.PropertyType.IsValueType);

            // If the property is of a primitive type, we must assign it a default value so it's not undefined
            if (isValueType && isAutoProperty) {
                if (isStatic)
                    WriteIdentifier(declaringType);
                else
                    WriteKeyword("this");

                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(propertyDefinition.Name));

                Space();
                WriteToken("=", null);
                Space();

                var defaultValue = AstMethodBodyBuilder.MakeDefaultValue(propertyDefinition.PropertyType);
                node.AddChild(defaultValue, new Role<Expression>("temporary"));
                defaultValue.AcceptVisitor(this, null);

                Semicolon();
            }

            return EndNode(node);
        }

        protected bool EmitEventMethods (AttributedNode node, EventDefinition eventDefinition) {
            if (IsIgnored(node.Attributes))
                return false;

            StartNode(node);

            var temporaryRole = new Role<BlockStatement>("temporary");
            var declaringType = eventDefinition.DeclaringType;
            bool isStatic = IsStatic(eventDefinition);

            Action<string, Action> emitBody = (prefix, f) => {
                WriteIdentifier(declaringType);
                if (!IsStatic(eventDefinition)) {
                    WriteToken(".", null);
                    WriteIdentifier("prototype");
                }

                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(String.Format("{0}_{1}", prefix, eventDefinition.Name)));

                Space();
                WriteToken("=", null);
                Space();

                WriteKeyword("function");
                Space();
                LPar();
                WriteIdentifier("value");
                RPar();
                OpenBrace(BraceStyle.EndOfLine);

                f();

                CloseBrace(BraceStyle.NextLine);
                Semicolon();
            };

            Action emitStorageRef = () => {
                WriteKeyword("this");
                WriteToken(".", null);
                WriteIdentifier(Util.EscapeIdentifier(eventDefinition.Name));
            };

            emitBody("add", () => {
                emitStorageRef();

                Space();
                WriteToken("=", null);
                Space();

                WriteIdentifier("System.Delegate.Combine");
                LPar();

                emitStorageRef();
                WriteToken(",", null);
                Space();

                WriteIdentifier("value");
                RPar();
                Semicolon();
            });

            emitBody("remove", () => {
                emitStorageRef();

                Space();
                WriteToken("=", null);
                Space();

                WriteIdentifier("System.Delegate.Remove");
                LPar();

                emitStorageRef();
                WriteToken(",", null);
                Space();

                WriteIdentifier("value");
                RPar();
                Semicolon();
            });

            EndNode(node);

            return false;
        }

        protected void EmitOverloadDispatcher (TypeReference declaringType, OverloadDispatcherMethodDeclaration methodDeclaration) {
            var overloads = methodDeclaration.Overloads.OrderBy((md) => md.Parameters.Count);
            WriteIdentifier("JSIL.OverloadedMethod");
            LPar();

            WriteIdentifier(declaringType);
            if (!IsStatic(methodDeclaration)) {
                WriteToken(".", null);
                WriteIdentifier("prototype");
            }

            WriteToken(",", null);
            Space();
            WritePrimitiveValue(methodDeclaration.Name);
            WriteToken(",", null);
            Space();
            WriteToken("[", null);

            formatter.Indent();

            var temporaryRole = new Role<AstType>("temporary");
            bool isFirst = true, isFirst2;
            foreach (var overload in overloads) {
                if (!isFirst) {
                    WriteToken(",", null);
                    Space();
                }

                NewLine();

                WriteToken("[", null);
                WritePrimitiveValue(GenerateName(overload));
                WriteToken(",", null);
                Space();

                WriteToken("[", null);
                isFirst2 = true;

                foreach (var parameter in overload.Parameters) {
                    if (!isFirst2) {
                        WriteToken(",", null);
                        Space();
                    }

                    var type = (AstType)parameter.Type.Clone();
                    methodDeclaration.AddChild(type, temporaryRole);
                    type.AcceptVisitor(this, null);
                    isFirst2 = false;
                }

                WriteToken("]", null);
                WriteToken("]", null);
                isFirst = false;
            }

            formatter.Unindent();

            NewLine();
            WriteToken("]", null);
            RPar();
            Semicolon();
        }

        public override object VisitAccessor (Accessor accessor, object data) {
            StartNode(accessor);
            string prefix;
            bool isStatic;

            var propertyDefinition = accessor.Parent.Annotation<PropertyDefinition>();
            var eventDefinition = accessor.Parent.Annotation<EventDefinition>();
            var memberName = ((MemberReference)propertyDefinition ?? (MemberReference)eventDefinition).Name;

            if (accessor.Role == PropertyDeclaration.GetterRole) {
                prefix = "get";
                isStatic = IsStatic(propertyDefinition);
            } else if (accessor.Role == PropertyDeclaration.SetterRole) {
                prefix = "set";
                isStatic = IsStatic(propertyDefinition);
            } else if (accessor.Role == CustomEventDeclaration.AddAccessorRole) {
                prefix = "add";
                isStatic = IsStatic(eventDefinition);
            } else if (accessor.Role == CustomEventDeclaration.RemoveAccessorRole) {
                prefix = "remove";
                isStatic = IsStatic(eventDefinition);
            } else {
                throw new NotImplementedException();
            }

            var methodName = String.Format(
                "{0}_{1}", prefix, memberName
            );
            TypeDefinition declaringType;

            if (propertyDefinition != null)
                declaringType = propertyDefinition.DeclaringType;
            else if (eventDefinition != null)
                declaringType = eventDefinition.DeclaringType;
            else
                throw new NotImplementedException();

            var storageName = Util.EscapeIdentifier(
                String.Format("{0}.value", memberName)
            );

            if (!isStatic) {
                WriteIdentifier(declaringType);
                WriteToken(".", null);
                WriteIdentifier("prototype");
            } else {
                WriteThisReference(declaringType, isStatic);
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
                } else {
                    throw new NotImplementedException();
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

        public override object VisitAnonymousMethodExpression (ICSharpCode.NRefactory.CSharp.AnonymousMethodExpression anonymousMethodExpression, object data) {
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

            var methodDefinition = methodDeclaration.Annotation<MethodDefinition>();
            var declaringType = methodDefinition.DeclaringType;

            var odmd = methodDeclaration as OverloadDispatcherMethodDeclaration;
            if (odmd != null) {
                EmitOverloadDispatcher(declaringType, odmd);

                return EndNode(methodDeclaration);
            }

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

        protected bool TypeIsDelegate (TypeReference typeReference) {
            if (typeReference == null)
                return false;

            var td = typeReference.Resolve();
            if ((td.BaseType.FullName == "System.Delegate") || (td.BaseType.FullName == "System.MulticastDelegate"))
                return true;

            return false;
        }

        public override object VisitCastExpression (CastExpression castExpression, object data) {
            StartNode(castExpression);

            var tr = castExpression.Type.Annotation<TypeReference>();
            if (TypeIsDelegate(tr)) {
                castExpression.Expression.AcceptVisitor(this, null);

                return EndNode(castExpression);
            }

            WriteIdentifier("JSIL.Cast");
            LPar();
            castExpression.Expression.AcceptVisitor(this, null);
            WriteToken(",", null);
            Space();
            castExpression.Type.AcceptVisitor(this, null);
            RPar();

            return EndNode(castExpression);
        }

        public object VisitDynamicExpression (JSIL.Expressions.DynamicExpression dynamicExpression, object data) {
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

            WriteKeyword("");
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
