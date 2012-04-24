using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using JSIL.Internal;
using Microsoft.CSharp.RuntimeBinder;
using MethodInfo = System.Reflection.MethodInfo;

namespace JSIL.Ast {
    public abstract class JSAstVisitor {
        public readonly Stack<JSNode> Stack = new Stack<JSNode>();
        public readonly Stack<string> NameStack = new Stack<string>(); 
        
        protected int NodeIndex, NextNodeIndex;
        protected int StatementIndex, NextStatementIndex;
        protected JSNode PreviousSibling = null;
        protected JSNode NextSibling = null;

        public delegate void NodeVisitor (JSAstVisitor @this, JSNode node);
        public delegate void NodeVisitor<TVisitor, TNode> (TVisitor @this, TNode node)
            where TVisitor : JSAstVisitor
            where TNode : JSNode;

        protected readonly VisitorCache Visitors;

        protected JSAstVisitor () {
            Visitors = VisitorCache.Get(this);
        }

        protected class VisitorCache {
            protected class Adapter<TVisitor, TNode> 
                where TVisitor : JSAstVisitor
                where TNode : JSNode {
                public readonly NodeVisitor<TVisitor, TNode> Method;

                public Adapter (NodeVisitor<TVisitor, TNode> method) {
                    Method = method;
                }

                public void Visit (JSAstVisitor @this, JSNode node) {
                    Method((TVisitor)@this, (TNode)node);
                }
            }

            protected static ConcurrentCache<Type, VisitorCache> VisitorCaches = new ConcurrentCache<Type, VisitorCache>(); 

            protected readonly Dictionary<Type, NodeVisitor> Methods = new Dictionary<Type, NodeVisitor>();
            protected readonly ConcurrentCache<Type, NodeVisitor> Cache = new ConcurrentCache<Type, NodeVisitor>();
            public readonly Type VisitorType;

            protected VisitorCache (Type visitorType) {
                VisitorType = visitorType;

                foreach (var m in VisitorType.GetMethods()) {
                    if (m.Name != "VisitNode")
                        continue;

                    var parameters = m.GetParameters();
                    if (parameters.Length != 1)
                        continue;

                    var nodeType = parameters[0].ParameterType;

                    Methods.Add(nodeType, MakeVisitorAdapter(m, visitorType, nodeType));
                }
            }

            public static VisitorCache Get (JSAstVisitor visitor) {
                var visitorType = visitor.GetType();

                return VisitorCaches.GetOrCreate(
                    visitorType, () => new VisitorCache(visitorType)
                );
            }

            protected static NodeVisitor MakeVisitorAdapter (MethodInfo method, Type visitorType, Type nodeType) {
                var tAdapterUnbound = typeof(Adapter<,>);
                var tAdapter = tAdapterUnbound.MakeGenericType(visitorType, nodeType);
                var tVisitorMethodUnbound = typeof(NodeVisitor<,>);
                var tVisitorMethod = tVisitorMethodUnbound.MakeGenericType(visitorType, nodeType);
                var tAdapterMethod = typeof(NodeVisitor);

                var visitorMethod = Delegate.CreateDelegate(tVisitorMethod, method, true);

                var adapter = tAdapter.GetConstructor(new[] { 
                    tVisitorMethod
                }).Invoke(new object[] { visitorMethod });

                var adapterMethod = adapter.GetType().GetMethod("Visit", BindingFlags.Public | BindingFlags.Instance);
                var result = Delegate.CreateDelegate(tAdapterMethod, adapter, adapterMethod);

                return (NodeVisitor)result;
            }

            public NodeVisitor Get (JSNode node) {
                if (node == null)
                    return null;

                var nodeType = node.GetType();
                var currentType = nodeType;

                return Cache.GetOrCreate(
                    nodeType, () => {
                        while (currentType != null) {
                            NodeVisitor result;
                            if (Methods.TryGetValue(currentType, out result))
                                return result;

                            currentType = currentType.BaseType;
                        }

                        return null;
                    }
                );
            }
        }

        /// <summary>
        /// Visits a node and its children (if any), updating the traversal stack. The current node is replaced by the new node.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected void VisitReplacement (JSNode node) {
            Stack.Pop();
            Stack.Push(node);

            var visitor = Visitors.Get(node);

            if (visitor != null)
                visitor(this, node);
            else
                VisitNode(node);
        }

        /// <summary>
        /// Visits a node and its children (if any), updating the traversal stack.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <param name="name">The name to annotate the node with, if any.</param>
        public void Visit (JSNode node, string name = null) {
            var oldNodeIndex = NodeIndex;
            var oldStatementIndex = StatementIndex;

#if PARANOID
            if (Stack.Contains(node))
                throw new InvalidOperationException("AST traversal formed a cycle");
#endif

            Stack.Push(node);
            NameStack.Push(name);

            try {
                NodeIndex = NextNodeIndex;
                NextNodeIndex += 1;

                if (node is JSStatement) {
                    StatementIndex = NextStatementIndex;
                    NextStatementIndex += 1;
                }

                var visitor = Visitors.Get(node);

                if (visitor != null)
                    visitor(this, node);
                else
                    VisitNode(node);
            } finally {
                Stack.Pop();
                NameStack.Pop();
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
                throw new ArgumentNullException("node");

            var oldPreviousSibling = PreviousSibling;
            var oldNextSibling = NextSibling;

            try {
                PreviousSibling = NextSibling = null;

                var annotated = node as IAnnotatedChildren;
                if (annotated != null) {
                    string nextSiblingName = null;

                    using (var e = annotated.AnnotatedChildren.GetEnumerator())
                    while (e.MoveNext()) {
                        var toVisit = NextSibling;
                        var toVisitName = nextSiblingName;
                        NextSibling = e.Current.Node;
                        nextSiblingName = e.Current.Name;

                        if (toVisit != null)
                            Visit(toVisit, toVisitName);

                        PreviousSibling = toVisit;
                    }
                } else {
                    using (var e = node.Children.GetEnumerator())
                    while (e.MoveNext()) {
                        var toVisit = NextSibling;
                        NextSibling = e.Current;

                        if (toVisit != null)
                            Visit(toVisit);

                        PreviousSibling = toVisit;
                    }

                }

                if (NextSibling != null) {
                    var toVisit = NextSibling;
                    NextSibling = null;

                    if (toVisit != null)
                        Visit(toVisit);
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

        protected string CurrentName {
            get {
                return NameStack.FirstOrDefault();
            }
        }

        protected JSNode ParentNode {
            get {
                using (var e = Stack.GetEnumerator()) {
                    if (e.MoveNext())
                        if (e.MoveNext())
                            return e.Current;

                    return null;
                }
            }
        }

        protected string ParentName {
            get {
                using (var e = NameStack.GetEnumerator()) {
                    if (e.MoveNext())
                        if (e.MoveNext())
                            return e.Current;

                    return null;
                }
            }
        }
    }
}
