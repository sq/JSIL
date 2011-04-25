using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace JSIL {
    public class VarargsParameterDeclaration : ParameterDeclaration {
        public VarargsParameterDeclaration (ParameterDeclaration inner) {
            ParameterModifier = inner.ParameterModifier;
            Name = inner.Name;

            foreach (var a in inner.Annotations) {
                var ic = a as ICloneable;

                if (ic != null)
                    AddAnnotation(ic.Clone());
                else
                    AddAnnotation(a);
            }

            foreach (var child in inner.Children)
                AddChildUnsafe(child.Clone(), child.Role);
        }
    }

    public class ModifiedVariableInitializer : VariableInitializer {
        public readonly ParameterModifier Modifier;

        public ModifiedVariableInitializer (VariableInitializer inner, ParameterModifier modifier) {
            Modifier = modifier;
            Name = inner.Name;

            if ((inner.Initializer == null) || inner.Initializer.IsNull)
                Initializer = new LiteralIdentifierExpression("undefined");
            else
                Initializer = inner.Initializer.Clone();
        }
    }

    public class LiteralIdentifierExpression : IdentifierExpression {
        public LiteralIdentifierExpression (string identifier)
            : base (identifier) {
        }
    }

    public class ModifiedIdentifierExpression : IdentifierExpression {
        public readonly ParameterModifier Modifier;

        public ModifiedIdentifierExpression (IdentifierExpression inner, ParameterModifier modifier) {
            Modifier = modifier;
            Identifier = inner.Identifier;

            foreach (var a in inner.Annotations) {
                var ic = a as ICloneable;

                if (ic != null)
                    AddAnnotation(ic.Clone());
                else
                    AddAnnotation(a);
            }
        }
    }

    public class ParameterModifierTransformer : ContextTrackingVisitor<object> {
        public class Scope {
            public readonly Scope Parent;
            public readonly Dictionary<string, AstNode> Initializers = new Dictionary<string, AstNode>();
            public readonly Dictionary<string, ParameterModifier> Modifiers = new Dictionary<string, ParameterModifier>();
            public readonly Dictionary<string, List<IdentifierExpression>> IdentifierReferences = new Dictionary<string, List<IdentifierExpression>>();

            public Scope (Scope parent = null) {
                Parent = parent;
            }

            public Scope NewChild () {
                var result = new Scope(this);

                foreach (var kvp in Initializers)
                    result.Initializers.Add(kvp.Key, kvp.Value);

                foreach (var kvp in Modifiers)
                    result.Modifiers.Add(kvp.Key, kvp.Value);

                return result;
            }

            public ParameterModifier GetModifier (string key) {
                ParameterModifier modifier;
                if (!Modifiers.TryGetValue(key, out modifier))
                    modifier = ParameterModifier.None;

                return modifier;
            }

            public void InitializerReplaced (string key, AstNode newInitializer) {
                Initializers[key] = newInitializer;

                if (Parent != null)
                    Parent.InitializerReplaced(key, newInitializer);
            }

            public void ModifierChanged (string key, ParameterModifier newModifier) {
                Modifiers[key] = newModifier;

                if (Parent != null)
                    Parent.ModifierChanged(key, newModifier);
            }
        }

        public readonly Stack<Scope> Scopes = new Stack<Scope>();

        public ParameterModifierTransformer (DecompilerContext context)
            : base(context) {

            Scopes.Push(new Scope());
        }

        protected Scope CurrentScope {
            get {
                return Scopes.Peek();
            }
        }

        protected override object VisitChildren (AstNode node, object data) {
            bool isDeclaration = (node is MethodDeclaration) || (node is PropertyDeclaration) || (node is TypeDeclaration);

            if (isDeclaration) {
                var currentScope = Scopes.Peek();
                var newScope = currentScope.NewChild();
                Scopes.Push(newScope);
            }

            var result = base.VisitChildren(node, data);

            if (isDeclaration) {
                var scope = CurrentScope;
                Scopes.Pop();

                foreach (var kvp in scope.IdentifierReferences) {
                    var list = kvp.Value;

                    foreach (var iref in list) {
                        var currentModifier = scope.GetModifier(iref.Identifier);
                        var refModifier = GetModifier(iref);

                        if (currentModifier != ParameterModifier.None)
                            continue;

                        if (refModifier != ParameterModifier.None) {
                            var initializer = scope.Initializers[iref.Identifier];
                            var pd = initializer as ParameterDeclaration;
                            var vi = initializer as VariableInitializer;

                            if (pd != null) {
                                var md = pd.Parent as MethodDeclaration;
                                if (md == null)
                                    throw new NotImplementedException("ref/out parameters not implemented for this body type");

                                var newName = "_" + pd.Name;
                                var newStatement = new VariableDeclarationStatement {
                                    Type = new PrimitiveType("object"),
                                    Variables = {
                                        new ModifiedVariableInitializer(new VariableInitializer(
                                            pd.Name, new IdentifierExpression(newName)
                                        ), refModifier)
                                    }
                                };

                                pd.Name = newName;

                                md.Body.InsertChildBefore(
                                    md.Body.FirstChild, newStatement, (Role<Statement>)md.Body.FirstChild.Role
                                );

                                scope.InitializerReplaced(iref.Identifier, newStatement.Variables.First());
                                scope.ModifierChanged(iref.Identifier, refModifier);
                            } else if (vi != null) {
                                var newInitializer = new ModifiedVariableInitializer(
                                    (VariableInitializer)vi.Clone(), refModifier
                                );

                                vi.ReplaceWith(newInitializer);

                                scope.InitializerReplaced(iref.Identifier, newInitializer);
                                scope.ModifierChanged(iref.Identifier, refModifier);
                            } else {
                                throw new InvalidOperationException();
                            }
                        }
                    }

                    foreach (var iref in list) {
                        var currentModifier = scope.GetModifier(iref.Identifier);
                        var refModifier = GetModifier(iref);

                        if (
                            (currentModifier == ParameterModifier.None) ==
                            (refModifier == ParameterModifier.None)
                        )
                            continue;

                        var replacement = new ModifiedIdentifierExpression(
                            iref, refModifier
                        );
                        iref.ReplaceWith(replacement);
                    }
                }
            }

            return result;
        }

        public override object VisitVariableInitializer (VariableInitializer variableInitializer, object data) {
            CurrentScope.Initializers[variableInitializer.Name] = variableInitializer;
            CurrentScope.Modifiers[variableInitializer.Name] = ParameterModifier.None;

            return base.VisitVariableInitializer(variableInitializer, data);
        }

        public override object VisitIdentifierExpression (IdentifierExpression identifierExpression, object data) {
            var id = identifierExpression.Identifier;            

            List<IdentifierExpression> seen;
            if (!CurrentScope.IdentifierReferences.TryGetValue(id, out seen)) {
                seen = new List<IdentifierExpression>();
                CurrentScope.IdentifierReferences.Add(id, seen);
            }

            seen.Add(identifierExpression);

            return base.VisitIdentifierExpression(identifierExpression, data);
        }

        protected ParameterModifier GetModifier (AstNode node) {
            var de = node.Parent as DirectionExpression;
            var pd = (node as ParameterDeclaration) ?? (node.Parent as ParameterDeclaration);

            if (de != null) {
                switch (de.FieldDirection) {
                    case FieldDirection.Out:
                        return ParameterModifier.Out;
                    case FieldDirection.Ref:
                        return ParameterModifier.Ref;
                }

            } else if (pd != null) {
                switch (pd.ParameterModifier) {
                    case ParameterModifier.Params:
                    case ParameterModifier.This:
                        throw new NotImplementedException("Only out and ref parameter modifiers are implemented");
                    default:
                        return pd.ParameterModifier;
                }
            }

            return ParameterModifier.None;
        }

        public override object VisitParameterDeclaration (ParameterDeclaration parameterDeclaration, object data) {
            var id = parameterDeclaration.Name;

            if (parameterDeclaration.ParameterModifier == ParameterModifier.Params) {
                var index = parameterDeclaration.Annotation<ParameterReference>().Index;
                ConstructorDeclaration cd;
                MethodDeclaration md;

                var invocation = new InvocationExpression {
                    Target = new LiteralIdentifierExpression("Array.prototype.slice.call"),
                    Arguments = {
                        new LiteralIdentifierExpression("arguments"),
                        new PrimitiveExpression(index)
                    }
                };
                invocation.AddAnnotation(parameterDeclaration.Annotation<ParameterReference>().ParameterType);

                var initialization = new VariableDeclarationStatement(
                    (AstType)parameterDeclaration.Type.Clone(), 
                    parameterDeclaration.Name, invocation
                );

                if ((cd = parameterDeclaration.Parent as ConstructorDeclaration) != null) {
                    var body = cd.Body;
                    body.InsertChildBefore(body.FirstChild, initialization, BlockStatement.StatementRole);
                } else if ((md = parameterDeclaration.Parent as MethodDeclaration) != null) {
                    var body = md.Body;
                    body.InsertChildBefore(body.FirstChild, initialization, BlockStatement.StatementRole);
                } else {
                    throw new NotImplementedException("Params arguments not supported for this member type");
                }

                parameterDeclaration.Remove();
            } else {
                CurrentScope.Initializers[id] = parameterDeclaration;
                CurrentScope.Modifiers[id] = parameterDeclaration.ParameterModifier;
            }

            return null;
        }
    }
}
