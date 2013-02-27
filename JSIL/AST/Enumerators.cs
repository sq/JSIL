using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Ast.Enumerators {
    public struct JSNodeChildren : IEnumerable<JSNode> {
        public readonly JSNode Node;
        public readonly bool IncludeSelf;

        public JSNodeChildren (JSNode node, bool includeSelf) {
            Node = node;
            IncludeSelf = includeSelf;
        }

        public JSNodeChildEnumerator GetEnumerator () {
            return new JSNodeChildEnumerator(Node, IncludeSelf);
        }

        System.Collections.Generic.IEnumerator<JSNode> System.Collections.Generic.IEnumerable<JSNode>.GetEnumerator () {
            return new JSNodeChildEnumerator(Node, IncludeSelf);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return new JSNodeChildEnumerator(Node, IncludeSelf);
        }
    }

    public struct JSNodeChildrenRecursive : IEnumerable<JSNode> {
        public readonly JSNode Node;
        public readonly bool IncludeSelf;

        public JSNodeChildrenRecursive (JSNode node, bool includeSelf) {
            Node = node;
            IncludeSelf = includeSelf;
        }

        public JSNodeChildEnumerator GetEnumerator () {
            return new JSNodeChildRecursiveEnumerator(Node, IncludeSelf);
        }

        System.Collections.Generic.IEnumerator<JSNode> System.Collections.Generic.IEnumerable<JSNode>.GetEnumerator () {
            return new JSNodeChildRecursiveEnumerator(Node, IncludeSelf);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return new JSNodeChildRecursiveEnumerator(Node, IncludeSelf);
        }
    }

    public struct JSNodeChildEnumerator : IEnumerator<JSNode> {
        public readonly JSNode Node;
        public readonly bool IncludeSelf;

        private JSNode _Current;

        public JSNodeChildEnumerator (JSNode node, bool includeSelf) {
            Node = node;
            IncludeSelf = includeSelf;
            _Current = null;
        }

        public JSNode Current {
            get { return _Current; }
        }

        public void Dispose () {
            throw new NotImplementedException();
        }

        object System.Collections.IEnumerator.Current {
            get { return _Current; }
        }

        public bool MoveNext () {
            throw new NotImplementedException();
        }

        public void Reset () {
            throw new NotImplementedException();
        }
    }

    public struct JSNodeChildRecursiveEnumerator : IEnumerator<JSNode> {
        public readonly JSNode Node;
        public readonly bool IncludeSelf;

        private JSNode _Current;

        public JSNodeChildRecursiveEnumerator (JSNode node, bool includeSelf) {
            Node = node;
            IncludeSelf = includeSelf;
            _Current = null;
        }

        public JSNode Current {
            get { return _Current; }
        }

        public void Dispose () {
            throw new NotImplementedException();
        }

        object System.Collections.IEnumerator.Current {
            get { return _Current; }
        }

        public bool MoveNext () {
            throw new NotImplementedException();
        }

        public void Reset () {
            throw new NotImplementedException();
        }
    }
}
