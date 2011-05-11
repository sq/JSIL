using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace JSIL.Ast {
    public abstract class JSAstVisitor {
        public readonly Stack<JSNode> Stack = new Stack<JSNode>();
        protected int NodeIndex, NextNodeIndex;
        protected int StatementIndex, NextStatementIndex;
        protected JSNode PreviousSibling = null;
        protected JSNode NextSibling = null;

        protected readonly VisitorCache Visitors;

        protected JSAstVisitor () {
            Visitors = new VisitorCache(this);
        }

        protected class VisitorCache {
            protected class Adapter<T> where T : JSNode {
                public readonly Action<T> Method;

                public Adapter (Action<T> method) {
                    Method = method;
                }

                public void Visit (JSNode node) {
                    Method((T)node);
                }
            }

            public readonly Dictionary<Type, Action<JSNode>> Methods = new Dictionary<Type, Action<JSNode>>();
            public readonly Dictionary<Type, Action<JSNode>> Cache = new Dictionary<Type, Action<JSNode>>();
            public readonly Type VisitorType;

            public VisitorCache (JSAstVisitor target) {
                VisitorType = target.GetType();

                foreach (var m in VisitorType.GetMethods()) {
                    if (m.Name != "VisitNode")
                        continue;

                    var parameters = m.GetParameters();
                    if (parameters.Length != 1)
                        continue;

                    var nodeType = parameters[0].ParameterType;

                    Methods.Add(nodeType, MakeVisitorAdapter(m, nodeType, target));
                }
            }

            protected static Action<JSNode> MakeVisitorAdapter (MethodInfo method, Type nodeType, JSAstVisitor target) {
                var tAdapterUnbound = typeof(Adapter<>);
                var tAdapter = tAdapterUnbound.MakeGenericType(nodeType);
                var tVisitorMethodUnbound = typeof(Action<>);
                var tVisitorMethod = tVisitorMethodUnbound.MakeGenericType(nodeType);
                var tAdapterMethod = typeof(Action<JSNode>);

                var visitorMethod = Delegate.CreateDelegate(tVisitorMethod, target, method);

                var adapter = tAdapter.GetConstructor(new[] { 
                        tVisitorMethod
                    }).Invoke(new object[] { visitorMethod });

                var adapterMethod = adapter.GetType().GetMethod("Visit", BindingFlags.Public | BindingFlags.Instance);
                var result = Delegate.CreateDelegate(tAdapterMethod, adapter, adapterMethod);

                return (Action<JSNode>)result;
            }

            public Action<JSNode> Get (JSNode node) {
                if (node == null)
                    return null;

                var nodeType = node.GetType();
                var currentType = nodeType;
                Action<JSNode> result;

                if (Cache.TryGetValue(nodeType, out result))
                    return result;

                while (currentType != null) {
                    if (Methods.TryGetValue(currentType, out result)) {
                        Cache[nodeType] = result;
                        return result;
                    }

                    currentType = currentType.BaseType;
                }

                Cache[nodeType] = null;
                return null;
            }
        }

        /// <summary>
        /// Visits a node and its children (if any), updating the traversal stack.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        public void Visit (JSNode node) {
            var oldNodeIndex = NodeIndex;
            var oldStatementIndex = StatementIndex;

#if PARANOID
            if (Stack.Contains(node))
                throw new InvalidOperationException("AST traversal formed a cycle");
#endif

            Stack.Push(node);

            try {
                NodeIndex = NextNodeIndex;
                NextNodeIndex += 1;

                if (node is JSStatement) {
                    StatementIndex = NextStatementIndex;
                    NextStatementIndex += 1;
                }

                var visitor = Visitors.Get(node);

                if (visitor != null)
                    visitor(node);
                else
                    VisitNode(node);
            } finally {
                Stack.Pop();
                NodeIndex = oldNodeIndex;
                StatementIndex = oldStatementIndex;
            }
        }

        /// <summary>
        /// Responsible for traversing a node. Do not invoke directly.
        /// By default, this method traverses the node's children, but takes no other action.
        /// </summary>
        public virtual void VisitNode (JSNode node) {
            if (node == null) {
                Debug.WriteLine("Warning: Null node found in JavaScript AST");
                return;
            }

            VisitChildren(node);
        }

        /// <summary>
        /// Traverses all of a node's children. This is the default behavior for VisitNode.
        /// </summary>
        protected virtual void VisitChildren (JSNode node) {
            if (node == null)
                throw new ArgumentNullException();

            var oldPreviousSibling = PreviousSibling;
            var oldNextSibling = NextSibling;

            try {
                PreviousSibling = NextSibling = null;

                using (var e = node.Children.GetEnumerator()) {
                    while (e.MoveNext()) {
                        var toVisit = NextSibling;
                        NextSibling = e.Current;

                        if (toVisit != null)
                            Visit(toVisit);

                        PreviousSibling = toVisit;
                    }

                    if (NextSibling != null) {
                        var toVisit = NextSibling;
                        NextSibling = null;

                        if (toVisit != null)
                            Visit(toVisit);
                    }
                }
            } finally {
                PreviousSibling = oldPreviousSibling;
                NextSibling = oldNextSibling;
            }
        }

        protected JSNode CurrentNode {
            get {
                return Stack.FirstOrDefault();
            }
        }

        protected JSNode ParentNode {
            get {
                return Stack.Skip(1).FirstOrDefault();
            }
        }
    }
}
