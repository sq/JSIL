using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace JSIL.Ast {
    public abstract class JSAstVisitor {
        public readonly Stack<JSNode> Stack = new Stack<JSNode>();

        /// <summary>
        /// Visits a node and its children (if any), updating the traversal stack.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        public void Visit (JSNode node) {
            Stack.Push(node);
            try {
                if (node != null)
                    (this as dynamic).VisitNode(node as dynamic);
                else
                    VisitNode(null);
            } catch (RuntimeBinderException) {
                Debug.WriteLine("Warning: Failed to dynamically dispatch node of type {0}", node.GetType());
                VisitNode(node);
            } finally {
                Stack.Pop();
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

            foreach (var child in node.Children)
                Visit(child);
        }
    }
}
