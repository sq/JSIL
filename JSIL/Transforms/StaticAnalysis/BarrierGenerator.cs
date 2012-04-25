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
        public readonly BarrierFlags Flags;
        public readonly BarrierSlot[] Slots;

        private Barrier (int node, BarrierFlags flags, BarrierSlot[] slots) {
            NodeIndex = node;
            Flags = flags;
            Slots = slots;
        }

        public override string ToString () {
            return String.Format(
                "{0} = [{1}, {2}]", NodeIndex, Flags,
                String.Join(", ", from s in Slots select s.ToString())
            );
        }

        public static Barrier New (int node, BarrierFlags flags, SlotDictionary slots) {
            return new Barrier(node, flags, slots.ToArray());
        }

        /// <summary>
        /// Creates an empty barrier usable for search operations.
        /// </summary>
        public static Barrier Key (int node) {
            return new Barrier(node, BarrierFlags.None, null);
        }

        public static int Order (Barrier lhs, Barrier rhs) {
            return lhs.NodeIndex.CompareTo(rhs.NodeIndex);
        }

        public class Sorter : IComparer<Barrier> {
            public int Compare (Barrier x, Barrier y) {
                return Barrier.Order(x, y);
            }
        }
    }

    [Flags]
    public enum BarrierFlags : byte {
        None = 0x0,
        Invoke = 0x1,
        Return = 0x2
    }

    [Flags]
    public enum SlotFlags : byte {
        None = 0x0,
        Read = 0x1,
        Write = 0x2,
        PassByReference = 0x4,
        GlobalState = 0x8,

        ReadWrite = Read | Write,

        ReadGlobalState = Read | GlobalState,
        WriteGlobalState = Write | GlobalState,
        ReadWriteGlobalState = ReadGlobalState | WriteGlobalState,
    }

    public struct BarrierSlot {
        public readonly string Name;
        public readonly SlotFlags Flags;

        public BarrierSlot (string name, SlotFlags flags) {
            Name = name;
            Flags = flags;
        }

        public override string ToString () {
            return String.Format("{0}({1})", Name, Flags);
        }
    }

    public class BarrierCollection {
        protected readonly IComparer<Barrier> Comparer = new Barrier.Sorter();
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
                Barriers.Sort(Barrier.Order);
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
        protected readonly Dictionary<string, SlotFlags> Dictionary = new Dictionary<string, SlotFlags>();

        public void Clear () {
            Dictionary.Clear();
        }

        public void Add (string key, SlotFlags flags) {
            this[key] = flags;
        }

        public void Add (ILVariable key, SlotFlags flags) {
            this[key] = flags;
        }

        public void Add (JSVariable key, SlotFlags flags) {
            this[key] = flags;
        }

        public SlotFlags this[string key] {
            get {
                SlotFlags result;
                if (Dictionary.TryGetValue(key, out result))
                    return result;

                return SlotFlags.None;
            }
            set {
                Dictionary[key] = value;
            }
        }

        public SlotFlags this[ILVariable v] {
            get {
                return this[v.Name];
            }
            set {
                this[v.Name] = value;
            }
        }

        public SlotFlags this[JSVariable v] {
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

        protected void CreateBarrier (SlotDictionary slots, BarrierFlags flags = BarrierFlags.None) {
            Result.Add(Barrier.New(
                NodeIndex, flags, slots
            ));
        }

        public void VisitNode (JSVariable v) {
            if (CurrentName == "Parameter") {
                // In argument list
                return;
            }

            CreateBarrier(new SlotDictionary {
                {v, SlotFlags.Read}
            });

            VisitChildren(v);
        }

        public void VisitNode (JSField f) {
            if (f.HasGlobalStateDependency) {
                CreateBarrier(new SlotDictionary {
                    {f.Identifier, SlotFlags.ReadGlobalState}
                });
            }

            VisitChildren(f);
        }
    }
}
