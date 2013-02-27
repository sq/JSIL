using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ICSharpCode.NRefactory.PatternMatching;
using JSIL.Ast.Traversal;
using JSIL.Internal;

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

        public IEnumerator<JSNode> GetEnumerator () {
            if (IncludeSelf)
                yield return Node;

            var list = new LinkedList<JSNode>();

            using (var inner = new JSNodeChildEnumerator(Node, false))
            while (inner.MoveNext()) {
                var node = inner.Current;
                if (node != null)
                    list.AddLast(node);
            }

            while (list.Count > 0) {
                var current = list.First;

                foreach (var leaf in current.Value.Children) {
                    if (leaf != null)
                        list.AddBefore(current, leaf);
                }

                list.Remove(current);

                yield return current.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return this.GetEnumerator();
        }
    }

    public struct JSNodeChildEnumerator : IEnumerator<JSNode> {
        public readonly JSNodeTraversalData TraversalData;
        public readonly JSNode Node;
        public readonly bool IncludeSelf;

        internal int _Index, _ArrayIndex;

        internal JSNode _Current;
        internal string _CurrentName;

        public JSNodeChildEnumerator (JSNode node, bool includeSelf) {
            TraversalData = JSNodeTraversalData.Get(node);
            Node = node;
            IncludeSelf = includeSelf;
            _Index = IncludeSelf ? -2 : -1;
            _ArrayIndex = -1;
            _Current = null;
            _CurrentName = null;
        }

        public string CurrentName {
            get { return _CurrentName; }
        }

        public JSNode Current {
            get { return _Current; }
        }

        public void Dispose () {
        }

        object System.Collections.IEnumerator.Current {
            get { return _Current; }
        }

        public bool MoveNext () {
            _Current = null;
            _CurrentName = null;

            while (true) {
                if ((_ArrayIndex < 0) || (_Index < 0))
                    _Index += 1;

                if (_Index >= TraversalData.Records.Length)
                    return false;

                if (_Index < 0) {
                    if (IncludeSelf) {
                        _Current = Node;
                        _CurrentName = "Self";
                        return true;
                    } else
                        throw new InvalidOperationException("Enumerator error");
                }

                var record = TraversalData.Records[_Index];
                var elementRecord = record as JSNodeTraversalElementRecord;
                if (elementRecord != null) {
                    elementRecord.Get(Node, out _Current, out _CurrentName);

                    if (_Current != null)
                        return true;
                } else {
                    var arrayRecord = record as JSNodeTraversalArrayRecord;
                    if (arrayRecord != null) {
                        _ArrayIndex += 1;

                        if (arrayRecord.GetElement(Node, _ArrayIndex, out _Current, out _CurrentName)) {
                            if (_Current != null)
                                return true;
                        } else
                            _ArrayIndex = -1;
                    } else {
                        throw new InvalidDataException("Unrecognized record type");
                    }
                }
            }
        }

        public void Reset () {
            _Index = IncludeSelf ? -2 : -1;
            _ArrayIndex = -1;
            _Current = null;
            _CurrentName = null;
        }
    }
}
