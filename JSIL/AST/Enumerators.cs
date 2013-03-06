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
        public readonly JSNodeChildEnumerator EnumeratorTemplate;

        public JSNodeChildren (JSNode node, JSNodeTraversalData traversalData, bool includeSelf) {
            EnumeratorTemplate = new JSNodeChildEnumerator(node, traversalData, includeSelf);
        }

        System.Collections.Generic.IEnumerator<JSNode> System.Collections.Generic.IEnumerable<JSNode>.GetEnumerator () {
            return EnumeratorTemplate;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return EnumeratorTemplate;
        }
    }

    public struct JSNodeChildrenRecursive : IEnumerable<JSNode> {
        public readonly JSNode Node;
        public readonly JSNodeTraversalData TraversalData;
        public readonly bool IncludeSelf;

        public JSNodeChildrenRecursive (JSNode node, JSNodeTraversalData traversalData, bool includeSelf) {
            Node = node;
            TraversalData = traversalData;
            IncludeSelf = includeSelf;
        }

        public IEnumerator<JSNode> GetEnumerator () {
            if (IncludeSelf)
                yield return Node;

            var list = new LinkedList<JSNode>();

            using (var inner = new JSNodeChildEnumerator(Node, TraversalData, false))
            while (inner.MoveNext()) {
                var node = inner.Current;
                if (node != null)
                    list.AddLast(node);
            }

            while (list.Count > 0) {
                var current = list.First;

                using (var e = current.Value.Children.EnumeratorTemplate)
                while (e.MoveNext()) {
                    var leaf = e.Current;
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

        internal int _Index;
        internal JSNodeTraversalArrayRecord _ArrayRecord;
        internal JSNodeTraversalArrayRecordState _ArrayRecordState;

        internal JSNode _Current;
        internal string _CurrentName;

        public JSNodeChildEnumerator (JSNode node, JSNodeTraversalData traversalData, bool includeSelf) {
            TraversalData = traversalData;
            Node = node;
            IncludeSelf = includeSelf;
            _Index = IncludeSelf ? -2 : -1;
            _ArrayRecord = null;
            _ArrayRecordState = default(JSNodeTraversalArrayRecordState);
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
                if ((_ArrayRecord == null) || (_Index < 0))
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

                if (_ArrayRecord != null) {
                    if (_ArrayRecord.MoveNext(ref _ArrayRecordState)) {
                        _Current = _ArrayRecordState.CurrentNode;
                        _CurrentName = _ArrayRecordState.CurrentName;
                        return true;
                    } else {
                        _ArrayRecord = null;
                    }
                } else {
                    var record = TraversalData.Records[_Index];
                    var elementRecord = record as JSNodeTraversalElementRecord;
                    if (elementRecord != null) {
                        elementRecord.Get(Node, out _Current, out _CurrentName);

                        if (_Current != null)
                            return true;
                    } else {
                        var arrayRecord = record as JSNodeTraversalArrayRecord;
                        if (arrayRecord != null) {
                            _ArrayRecord = arrayRecord;
                            _ArrayRecordState = arrayRecord.StartEnumeration(Node);
                        } else {
                            throw new InvalidDataException("Unrecognized record type");
                        }
                    }
                }
            }
        }

        public void Reset () {
            _Index = IncludeSelf ? -2 : -1;
            _ArrayRecord = null;
            _Current = null;
            _CurrentName = null;
        }
    }
}
