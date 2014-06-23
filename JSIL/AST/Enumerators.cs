using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public JSNodeChildEnumerator GetEnumerator () {
            return EnumeratorTemplate;
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        System.Collections.Generic.IEnumerator<JSNode> System.Collections.Generic.IEnumerable<JSNode>.GetEnumerator () {
            return EnumeratorTemplate;
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator();
        }
    }

    public struct JSNodeChildEnumerator : IEnumerator<JSNode> {
        public readonly JSNodeTraversalData TraversalData;
        public readonly JSNode Node;
        public readonly bool IncludeSelf;
        internal readonly int RecordCount;

        internal int _Index;
        internal JSNodeTraversalArrayRecord _ArrayRecord;
        internal JSNodeTraversalArrayRecordState _ArrayRecordState;

        internal JSNode _Current;
        internal string _CurrentName;

        public JSNodeChildEnumerator (JSNode node, JSNodeTraversalData traversalData, bool includeSelf) {
            TraversalData = traversalData;
            Node = node;
            IncludeSelf = includeSelf;
            RecordCount = traversalData.Records.Length;
            _Index = IncludeSelf ? -2 : -1;
            _ArrayRecord = null;
            _ArrayRecordState = default(JSNodeTraversalArrayRecordState);
            _Current = null;
            _CurrentName = null;
        }

        public string CurrentName {
#if TARGETTING_FX_4_5
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get { return _CurrentName; }
        }

        public JSNode Current {
#if TARGETTING_FX_4_5
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get { return _Current; }
        }

        public void Dispose () {
        }

        object System.Collections.IEnumerator.Current {
            get { return _Current; }
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool MoveNextArray () {
            if (_ArrayRecord.MoveNext(ref _ArrayRecordState)) {
                _Current = _ArrayRecordState.CurrentNode;
                _CurrentName = _ArrayRecordState.CurrentName;
                return true;
            } else {
                _ArrayRecord = null;
                return false;
            }
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool MoveNextElement (JSNodeTraversalElementRecord elementRecord) {
            elementRecord.Get(Node, out _Current, out _CurrentName);

            return _Current != null;
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void MoveNextArrayRecord (JSNodeTraversalArrayRecord arrayRecord) {
            _ArrayRecord = arrayRecord;
            _ArrayRecordState = arrayRecord.StartEnumeration(Node);
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool MoveNext () {
            _Current = null;
            _CurrentName = null;

            while (true) {
                var isEnumeratingArray = _ArrayRecord != null;
                if (!isEnumeratingArray || (_Index < 0))
                    _Index += 1;

                if (_Index >= RecordCount)
                    return false;

                if (_Index < 0) {
                    if (IncludeSelf) {
                        _Current = Node;
                        _CurrentName = "Self";
                        return true;
                    } else
                        // FIXME: Throw?
                        return false;
                }

                if (!isEnumeratingArray) {
                    var record = TraversalData.Records[_Index];
                    var elementRecord = record as JSNodeTraversalElementRecord;

                    if (elementRecord != null) {
                        if (MoveNextElement(elementRecord))
                            return true;
                    } else {
                        var arrayRecord = record as JSNodeTraversalArrayRecord;

                        if (arrayRecord != null)
                            MoveNextArrayRecord(arrayRecord);
                        else
                            // FIXME: Throw?
                            return false;
                    }
                } else {
                    if (MoveNextArray())
                        return true;
                }
            }
        }

#if TARGETTING_FX_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Reset () {
            _Index = IncludeSelf ? -2 : -1;
            _ArrayRecord = null;
            _Current = null;
            _CurrentName = null;
        }
    }
}
