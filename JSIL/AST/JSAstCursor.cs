using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JSIL.Ast {
    public class JSAstCursor : IDisposable {
        private class Indices {
            private int NextNodeIndex, NextStatementIndex;

            public int GetNodeIndex () {
                return NextNodeIndex++;
            }

            public int GetStatementIndex () {
                return NextStatementIndex++;
            }
        }

        private struct State {
            public readonly JSNode Node;
            public readonly string Name;
            public readonly int Depth;
            public readonly int NodeIndex;
            public readonly int? StatementIndex;

            public State (JSNode node, string name, int depth, int nodeIndex, int? statementIndex) {
                Node = node;
                Name = name;
                Depth = depth;
                NodeIndex = nodeIndex;
                StatementIndex = statementIndex;
            }
        }

        public readonly JSNode Root;
        public readonly HashSet<string> NamesToSkip;

        private IEnumerator<State> Enumerator = null;
        private State Current = default(State);

        public JSAstCursor (JSNode root, params string[] namesToSkip) {
            Root = root;
            NamesToSkip = new HashSet<string>(namesToSkip);

            Reset();
        }

        private void DisposeEnumerator () {
            if (Enumerator != null)
                Enumerator.Dispose();

            Enumerator = null;
        }

        public void Reset () {
            DisposeEnumerator();
            Enumerator = VisitNode(Root).GetEnumerator();
        }

        public void Dispose () {
            DisposeEnumerator();
        }

        public bool MoveNext () {
            var result = Enumerator.MoveNext();
            Current = Enumerator.Current;
            return result;
        }

        private void SkipNode (JSNode node, string name, Indices indices, int depth) {
            indices.GetNodeIndex();

            if (node is JSStatement)
                indices.GetStatementIndex();

            foreach (var e in VisitChildren(node, indices, depth)) {
                foreach (var c in e)
                    ;
            }
        }

        private IEnumerable<State> VisitNode (JSNode node, string name = null, Indices indices = null, int depth = 0) {
            int nodeIndex;
            int? statementIndex = null;

            if (indices == null)
                indices = new Indices();

            nodeIndex = indices.GetNodeIndex();

            if (node is JSStatement)
                statementIndex = indices.GetStatementIndex();

            yield return new State(
                node, name, depth, nodeIndex, statementIndex
            );

            foreach (var e in VisitChildren(node, indices, depth)) {
                foreach (var c in e)
                    yield return c;
            }
        }

        private IEnumerable<IEnumerable<State>> VisitChildren (JSNode node, Indices indices, int depth) {
            if (node == null)
                throw new ArgumentNullException("node");

            JSNode nextSibling = null;
            string nextSiblingName = null;

            int nextDepth = depth + 1;

            using (var e = node.Children.GetEnumerator())
            while (e.MoveNext()) {
                var toVisit = nextSibling;
                var toVisitName = nextSiblingName;
                nextSibling = e.Current;
                nextSiblingName = e.CurrentName;

                if (toVisit != null) {
                    if (toVisitName == null || !NamesToSkip.Contains(toVisitName)) {
                        yield return VisitNode(toVisit, toVisitName, indices, nextDepth);
                    } else {
                        SkipNode(toVisit, toVisitName, indices, nextDepth);
                    }
                }
            }

            if (nextSibling != null) {
                if (nextSiblingName == null || !NamesToSkip.Contains(nextSiblingName)) {
                    yield return VisitNode(nextSibling, nextSiblingName, indices, nextDepth);
                } else {
                    SkipNode(nextSibling, nextSiblingName, indices, nextDepth);
                }
            }
        }

        public int Depth {
            get {
                return Current.Depth;
            }
        }

        public int NodeIndex {
            get {
                return Current.NodeIndex;
            }
        }

        public int? StatementIndex {
            get {
                return Current.StatementIndex;
            }
        }

        public JSNode CurrentNode {
            get {
                return Current.Node;
            }
        }

        public string CurrentName {
            get {
                return Current.Name;
            }
        }
    }
}
