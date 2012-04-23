using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms.StaticAnalysis {
    public class Barrier {
        public readonly int NodeIndex;
        public readonly BarrierSlot[] Slots;

        private Barrier (int node, BarrierSlot[] slots) {
            NodeIndex = node;
            Slots = slots;
        }

        public override string ToString () {
            return String.Format(
                "{0} = [{1}]", NodeIndex,
                String.Join(", ", from s in Slots select s.ToString())
            );
        }

        public static Barrier New (int node, SlotDictionary slots) {
            return new Barrier(node, slots.ToArray());
        }

        /// <summary>
        /// Creates an empty barrier usable for search operations.
        /// </summary>
        public static Barrier Key (int node) {
            return new Barrier(node, null);
        }

        public static int Compare (Barrier lhs, Barrier rhs) {
            return lhs.NodeIndex.CompareTo(rhs.NodeIndex);
        }

        public class Comparer : IComparer<Barrier> {
            public int Compare (Barrier x, Barrier y) {
                return Barrier.Compare(x, y);
            }
        }
    }

    [Flags]
    public enum BarrierFlags : byte {
        None = 0x0,
        Read = 0x1,
        Write = 0x2,
        Invoke = 0x4,
        Return = 0x8,
        GlobalState = 0x10,
        PassByReference = 0x20,

        ReadWrite = Read | Write,

        ReadGlobalState = Read | GlobalState,
        WriteGlobalState = Write | GlobalState,
        ReadWriteGlobalState = ReadGlobalState | WriteGlobalState,
    }

    public struct BarrierSlot {
        public readonly string Name;
        public readonly BarrierFlags Flags;

        public BarrierSlot (string name, BarrierFlags flags) {
            Name = name;
            Flags = flags;
        }

        public override string ToString () {
            return String.Format("{0}({1})", Name, Flags);
        }
    }

    public class BarrierCollection {
        protected readonly IComparer<Barrier> Comparer = new Barrier.Comparer();
        protected readonly List<Barrier> Barriers = new List<Barrier>();
        protected bool _SortNeeded = false;

        public void Clear () {
            Barriers.Clear();
        }

        public void Add (Barrier barrier) {
            Barriers.Add(barrier);
            _SortNeeded = true;
        }

        public bool Remove (Barrier barrier) {
            return Barriers.Remove(barrier);
        }

        public bool RemoveAt (int nodeIndex) {
            SortIfNeeded();
            int index = Barriers.BinarySearch(Barrier.Key(nodeIndex), Comparer);
            if (index >= 0) {
                Barriers.RemoveAt(index);
                return true;
            }

            return false;
        }

        protected void SortIfNeeded () {
            if (_SortNeeded) {
                _SortNeeded = false;
                Barriers.Sort(Barrier.Compare);
            }
        }

        public bool TryGet (int nodeIndex, out Barrier result) {
            SortIfNeeded();
            result = default(Barrier);
            int index = Barriers.BinarySearch(Barrier.Key(nodeIndex), Comparer);

            return false;
        }

        public Barrier[] ToArray () {
            SortIfNeeded();
            return Barriers.ToArray();
        }
    }

    public class SlotDictionary : IEnumerable {
        protected readonly Dictionary<string, BarrierFlags> Dictionary = new Dictionary<string, BarrierFlags>();

        public void Clear () {
            Dictionary.Clear();
        }

        public void Add (string key, BarrierFlags flags) {
            this[key] = flags;
        }

        public void Add (ILVariable key, BarrierFlags flags) {
            this[key] = flags;
        }

        public void Add (JSVariable key, BarrierFlags flags) {
            this[key] = flags;
        }

        public BarrierFlags this[string key] {
            get {
                BarrierFlags result;
                if (Dictionary.TryGetValue(key, out result))
                    return result;

                return BarrierFlags.None;
            }
            set {
                Dictionary[key] = value;
            }
        }

        public BarrierFlags this[ILVariable v] {
            get {
                return this[v.Name];
            }
            set {
                this[v.Name] = value;
            }
        }

        public BarrierFlags this[JSVariable v] {
            get {
                return this[v.Name];
            }
            set {
                this[v.Name] = value;
            }
        }

        public int Count {
            get {
                return Dictionary.Count;
            }
        }

        public BarrierSlot[] ToArray () {
            return (from kvp in Dictionary select new BarrierSlot(kvp.Key, kvp.Value)).ToArray();
        }

        public IEnumerator GetEnumerator () {
            return Dictionary.GetEnumerator();
        }
    }

    public class BarrierGenerator : JSAstVisitor {
        public readonly BarrierCollection Result = new BarrierCollection();
        public readonly TypeSystem TypeSystem;

        public BarrierGenerator (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        protected void CreateBarrier (SlotDictionary slots) {
            Result.Add(Barrier.New(
                NodeIndex, slots
            ));
        }

        public void VisitNode (JSVariable v) {
            if (ParentNode is JSFunctionExpression) {
                // In argument list
                return;
            }

            CreateBarrier(new SlotDictionary {
                {v, BarrierFlags.Read}
            });

            VisitChildren(v);
        }

        public void VisitNode (JSField f) {
            if (f.HasGlobalStateDependency) {
                CreateBarrier(new SlotDictionary {
                    {f.Identifier, BarrierFlags.ReadGlobalState}
                });
            }

            VisitChildren(f);
        }
    }
}
