using System;
using System.Collections.Generic;
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
        readonly Stack<Dictionary<string, ParameterModifier>> Modifiers = new Stack<Dictionary<string, ParameterModifier>>();
        readonly Stack<Dictionary<string, VariableInitializer>> Initializers = new Stack<Dictionary<string, VariableInitializer>>();
        readonly Stack<Dictionary<string, List<IdentifierExpression>>> SeenIdentifiers = new Stack<Dictionary<string, List<IdentifierExpression>>>();

        public ParameterModifierTransformer (DecompilerContext context)
            : base(context) {

            Modifiers.Push(new Dictionary<string, ParameterModifier>());
            Initializers.Push(new Dictionary<string, VariableInitializer>());
            SeenIdentifiers.Push(new Dictionary<string, List<IdentifierExpression>>());
        }

        protected override object VisitChildren (AstNode node, object data) {
            {
                var currentDict = Modifiers.Peek();
                var newDict = new Dictionary<string, ParameterModifier>(currentDict);
                Modifiers.Push(currentDict);
            }

            {
                var currentDict = Initializers.Peek();
                var newDict = new Dictionary<string, VariableInitializer>(currentDict);
                Initializers.Push(currentDict);
            }

            {
                var currentDict = SeenIdentifiers.Peek();
                var newDict = new Dictionary<string, List<IdentifierExpression>>();
                foreach (var kvp in currentDict)
                    newDict.Add(kvp.Key, new List<IdentifierExpression>(kvp.Value));

                SeenIdentifiers.Push(currentDict);
            }

            var result = base.VisitChildren(node, data);

            Modifiers.Pop();
            Initializers.Pop();
            SeenIdentifiers.Pop();

            return result;
        }

        public override object VisitVariableInitializer (VariableInitializer variableInitializer, object data) {
            Initializers.Peek()[variableInitializer.Name] = variableInitializer;

            return base.VisitVariableInitializer(variableInitializer, data);
        }

        public override object VisitIdentifierExpression (IdentifierExpression identifierExpression, object data) {
            var id = identifierExpression.Identifier;

            ParameterModifier modifier;
            if (Modifiers.Peek().TryGetValue(id, out modifier)) {
                identifierExpression.ReplaceWith(new MemberReferenceExpression {
                    Target = identifierExpression.Clone(),
                    MemberName = "value"
                });
            }else {
                List<IdentifierExpression> seen;
                if (!SeenIdentifiers.Peek().TryGetValue(id, out seen)) {
                    seen = new List<IdentifierExpression>();
                    SeenIdentifiers.Peek().Add(id, seen);
                }

                seen.Add(identifierExpression);
            }

            return base.VisitIdentifierExpression(identifierExpression, data);
        }

        public override object VisitDirectionExpression (DirectionExpression directionExpression, object data) {
            var idE = directionExpression.Expression as IdentifierExpression;
            if ((idE == null) && (directionExpression.FieldDirection != FieldDirection.None))
                throw new NotImplementedException("Members of object instances cannot be passed as ref or out");

            var id = idE.Identifier;

            switch (directionExpression.FieldDirection) {
                case FieldDirection.Out:
                    Modifiers.Peek()[id] = ParameterModifier.Out;
                break;
                case FieldDirection.Ref:
                    Modifiers.Peek()[id] = ParameterModifier.Ref;
                break;
                default:
                    return base.VisitDirectionExpression(directionExpression, data);
            }

            List<IdentifierExpression> seen;
            if (SeenIdentifiers.Peek().TryGetValue(id, out seen)) {
                while (seen.Count > 0) {
                    var i = seen.Count - 1;
                    seen[i].ReplaceWith(new MemberReferenceExpression {
                        Target = seen[i].Clone(),
                        MemberName = "value"
                    }); 
                    seen.RemoveAt(i);
                }
            }

            VariableInitializer initializer;
            if (Initializers.Peek().TryGetValue(id, out initializer)) {
                initializer.ReplaceWith(
                    new ModifiedVariableInitializer(initializer, Modifiers.Peek()[id])
                );
            }

            return null;
        }

        public override object VisitParameterDeclaration (ParameterDeclaration parameterDeclaration, object data) {
            var modifier = parameterDeclaration.ParameterModifier;

            if (modifier == ParameterModifier.Ref || modifier == ParameterModifier.Out)
                Modifiers.Peek()[parameterDeclaration.Name] = modifier;

            return base.VisitParameterDeclaration(parameterDeclaration, data);
        }
    }
}
