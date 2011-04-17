using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class ModifiedVariableInitializer : VariableInitializer {
        public readonly ParameterModifier Modifier;

        public ModifiedVariableInitializer (VariableInitializer inner, ParameterModifier modifier) {
            Modifier = modifier;
            Name = inner.Name;
            Initializer = inner.Initializer.Clone();
        }
    }

    public class ModifiedIdentifierExpression : IdentifierExpression {
        public readonly ParameterModifier Modifier;

        public ModifiedIdentifierExpression (IdentifierExpression inner, ParameterModifier modifier) {
            Modifier = modifier;
            Identifier = inner.Identifier;
            TypeArguments.AddRange(inner.TypeArguments);
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

                        var replacement = new MemberReferenceExpression {
                            Target = iref.Clone(),
                            MemberName = "value"
                        };
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
                    break;
                    case FieldDirection.Ref:
                        return ParameterModifier.Ref;
                    break;
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

        public override object VisitDirectionExpression (DirectionExpression directionExpression, object data) {
            var result = base.VisitDirectionExpression(directionExpression, data);

            var idE = directionExpression.Expression as IdentifierExpression;
            if ((idE == null) && (directionExpression.FieldDirection != FieldDirection.None))
                throw new NotImplementedException("Members of object instances cannot be passed as ref or out");

            return result;
        }

        public override object VisitParameterDeclaration (ParameterDeclaration parameterDeclaration, object data) {
            var id = parameterDeclaration.Name;

            CurrentScope.Initializers[id] = parameterDeclaration;
            CurrentScope.Modifiers[id] = parameterDeclaration.ParameterModifier;

            return null;
        }
    }
}
