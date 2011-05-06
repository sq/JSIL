using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace JSIL.Ast {
    public abstract class JSAstVisitor {
        public readonly Stack<JSNode> Stack = new Stack<JSNode>();
        protected JSNode CurrentNode = null;
        protected JSNode PreviousSibling = null;
        protected JSNode NextSibling = null;

        /// <summary>
        /// Visits a node and its children (if any), updating the traversal stack.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        public void Visit (JSNode node) {
            CurrentNode = node;

            try {
                if (node != null) {
                    if (node.IsNull)
                        return;

                    (this as dynamic).VisitNode(node as dynamic);
                } else
                    VisitNode(null);
            } finally {
                CurrentNode = null;
            }
        }

        /// <summary>
        /// Responsible for traversing a node. Do not invoke directly.
        /// By default, this method traverses the node's children, but takes no other action.
        /// </summary>
        public virtual void VisitNode (JSNode node) {
            VisitChildren(node as dynamic);
        }

        /// <summary>
        /// Traverses all of a node's children. This is the default behavior for VisitNode.
        /// </summary>
        protected virtual void VisitChildren (JSNode node) {
            if (node == null) {
                Debug.WriteLine("Warning: Null node found in JavaScript AST");
                return;
            }

            if (Stack.Contains(node))
                throw new InvalidOperationException("AST traversal formed a cycle");

            Stack.Push(node);
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
                Stack.Pop();
                PreviousSibling = oldPreviousSibling;
                NextSibling = oldNextSibling;
            }
        }

        protected JSNode ParentNode {
            get {
                return Stack.Peek();
            }
        }
    }
}
