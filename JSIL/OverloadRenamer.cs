using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public class OverloadRenamer : ContextTrackingVisitor<object> {
        public class Scope {
            public readonly Dictionary<string, List<MethodDeclaration>> Overloads = new Dictionary<string, List<MethodDeclaration>>();
        }

        public readonly Stack<Scope> Scopes = new Stack<Scope>();

        public OverloadRenamer (DecompilerContext context)
            : base(context) {

            Scopes.Push(new Scope());
        }

        public Scope CurrentScope {
            get {
                return Scopes.Peek();
            }
        }

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            var scope = new Scope();
            Scopes.Push(scope);
            var result = base.VisitTypeDeclaration(typeDeclaration, data);
            Scopes.Pop();

            foreach (var kvp in scope.Overloads) {
                if (kvp.Value.Count <= 1)
                    continue;

                var omds = new List<OverloadedMethodDeclaration>();

                int i = 0;
                foreach (var method in kvp.Value) {
                    var omd = new OverloadedMethodDeclaration(method, kvp.Value, i++);
                    method.ReplaceWith(omd);
                    omds.Add(omd);
                }

                var dispatcher = new OverloadDispatcherMethodDeclaration(omds);
                var last = omds.Last();
                dispatcher.AddAnnotation(last.Annotation<MethodDefinition>());
                last.Parent.InsertChildAfter(last, dispatcher, (Role<AttributedNode>)last.Role);
                typeDeclaration.AddAnnotation(omds);
            }

            return result;
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            List<MethodDeclaration> list;
            var fullName = methodDeclaration.GetFullName();

            if (!CurrentScope.Overloads.TryGetValue(fullName, out list)) {
                list = new List<MethodDeclaration>();

                CurrentScope.Overloads[fullName] = list;
            }

            list.Add(methodDeclaration);

            return base.VisitMethodDeclaration(methodDeclaration, data);
        }
    }

    public class OverloadDispatcherMethodDeclaration : MethodDeclaration {
        public readonly IEnumerable<OverloadedMethodDeclaration> Overloads;

        public OverloadDispatcherMethodDeclaration (IEnumerable<OverloadedMethodDeclaration> overloads) {
            var method = overloads.First();
            Name = method.Name;
            Modifiers = method.Modifiers;
            ReturnType = (AstType)method.ReturnType.Clone();
            PrivateImplementationType = (AstType)method.PrivateImplementationType.Clone();
            Overloads = overloads;

            foreach (var tp in method.TypeParameters)
                TypeParameters.Add((TypeParameterDeclaration)tp.Clone());

            foreach (var c in method.Constraints)
                Constraints.Add((Constraint)c.Clone());
        }
    }

    public class OverloadedMethodDeclaration : MethodDeclaration {
        public readonly IEnumerable<MethodDeclaration> Overloads;
        public readonly int OverloadIndex;

        public OverloadedMethodDeclaration (MethodDeclaration method, IEnumerable<MethodDeclaration> overloads, int index) {
            Name = method.Name;
            Modifiers = method.Modifiers;
            Overloads = overloads;
            OverloadIndex = index;

            foreach (var a in method.Annotations) {
                var ic = a as ICloneable;

                if (ic != null)
                    AddAnnotation(ic.Clone());
                else
                    AddAnnotation(a);
            }

            foreach (var child in method.Children)
                AddChildUnsafe(child, child.Role);
        }
    }
}
