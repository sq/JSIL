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
                    return true;
                } else {
                    var arrayRecord = record as JSNodeTraversalArrayRecord;
                    if (arrayRecord != null) {
                        _ArrayIndex += 1;

                        if (arrayRecord.GetElement(Node, _ArrayIndex, out _Current, out _CurrentName))
                            return true;
                        else {
                            _ArrayIndex = -1;
                            _Index += 1;
                        }
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
        private struct State {
            public bool RecurseOnNextStep, DontRecurseNext;
            public JSNodeChildEnumerator Enumerator;
        }

        private Stack<State> _Stack;
        private State _State;

        public JSNodeChildRecursiveEnumerator (JSNode node, bool includeSelf) {
            _Stack = new Stack<State>();
            _State = new State {
                Enumerator = new JSNodeChildEnumerator(node, includeSelf),
                RecurseOnNextStep = false,
                DontRecurseNext = includeSelf
            };
        }

        public int Depth {
            get { return _Stack.Count; }
        }

        public string CurrentName {
            get { return _State.Enumerator._CurrentName; }
        }

        public JSNode Current {
            get { return _State.Enumerator._Current; }
        }

        public void Dispose () {
            _State.Enumerator.Dispose();

            foreach (var e in _Stack)
                e.Enumerator.Dispose();

            _Stack.Clear();
        }

        object System.Collections.IEnumerator.Current {
            get { return _State.Enumerator._Current; }
        }

        public bool MoveNext () {
            if (_State.DontRecurseNext) {
                _State.DontRecurseNext = false;
            } else if (_State.RecurseOnNextStep) {
                _State.RecurseOnNextStep = false;
                _Stack.Push(_State);
                _State = new State {
                    Enumerator = new JSNodeChildEnumerator(_State.Enumerator.Current, false)
                };
            } else {
                _State.RecurseOnNextStep = true;
            }

            while (!_State.Enumerator.MoveNext()) {
                if (_Stack.Count > 0) {
                    _State.Enumerator.Dispose();
                    _State = _Stack.Pop();
                } else {
                    return false;
                }
            }

            return true;
        }

        public void Reset () {
            throw new NotSupportedException();
        }
    }
}
