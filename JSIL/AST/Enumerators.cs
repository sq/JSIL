using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public JSNodeChildRecursiveEnumerator GetEnumerator () {
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
            while (true) {
                if ((_ArrayIndex < 0) || (_Index < 0))
                    _Index += 1;

                if (_Index >= TraversalData.Records.Length)
                    return false;

                if (_Index < 0) {
                    if (IncludeSelf) {
                        _Current = Node;
                        _CurrentName = null;
                        return true;
                    } else
                        throw new InvalidOperationException("Enumerator error");
                }

                var record = TraversalData.Records[_Index];
                var elementRecord = record as JSNodeTraversalElementRecord;
                if (elementRecord != null) {
                    elementRecord.Get(Node, out _Current, out _CurrentName);
                    return true;
                } else {
                    var arrayRecord = record as JSNodeTraversalArrayRecord;
                    if (arrayRecord != null) {
                        _ArrayIndex += 1;

                        if (arrayRecord.GetElement(Node, _ArrayIndex, out _Current, out _CurrentName))
                            return true;
                        else
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

    public struct JSNodeChildRecursiveEnumerator : IEnumerator<JSNode> {
        private Stack<JSNodeChildEnumerator> _Stack;
        private JSNodeChildEnumerator _Enumerator;
        private bool _RecurseOnNextStep, _DontRecurseNext;

        public JSNodeChildRecursiveEnumerator (JSNode node, bool includeSelf) {
            _Stack = new Stack<JSNodeChildEnumerator>();
            _Enumerator = new JSNodeChildEnumerator(node, includeSelf);
            _RecurseOnNextStep = false;
            _DontRecurseNext = includeSelf;
        }

        public string CurrentName {
            get { return _Enumerator._CurrentName; }
        }

        public JSNode Current {
            get { return _Enumerator._Current; }
        }

        public void Dispose () {
            _Enumerator.Dispose();

            foreach (var e in _Stack)
                e.Dispose();

            _Stack.Clear();
        }

        object System.Collections.IEnumerator.Current {
            get { return _Enumerator._Current; }
        }

        public bool MoveNext () {
            if (_DontRecurseNext) {
                _DontRecurseNext = false;
            } else if (_RecurseOnNextStep) {
                _RecurseOnNextStep = false;
                _Stack.Push(_Enumerator);
                _Enumerator = new JSNodeChildEnumerator(_Enumerator.Current, false);
            } else {
                _RecurseOnNextStep = true;
            }

            while (!_Enumerator.MoveNext()) {
                if (_Stack.Count > 0) {
                    _Enumerator.Dispose();
                    _Enumerator = _Stack.Pop();
                } else {
                    return false;
                }
            }

            return true;
        }

        public void Reset () {
            while (_Stack.Count > 1) {
                var e = _Stack.Pop();
                e.Dispose();
            }

            if (_Stack.Count > 0)
                _Enumerator = _Stack.Pop();

            _Enumerator.Reset();
            _RecurseOnNextStep = false;
        }
    }
}
