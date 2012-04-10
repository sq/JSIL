using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mono.Cecil;

namespace JSIL.Internal {
    public class MethodSignature {
        public class EqualityComparer : IEqualityComparer<MethodSignature> {
            public bool Equals (MethodSignature x, MethodSignature y) {
                if (x == null)
                    return x == y;

                return x.Equals(y);
            }

            public int GetHashCode (MethodSignature obj) {
                return obj.GetHashCode();
            }
        }

        public readonly TypeReference ReturnType;
        public readonly TypeReference[] ParameterTypes;
        public readonly string[] GenericParameterNames;

        protected int? _Hash;

        public MethodSignature (
            TypeReference returnType, TypeReference[] parameterTypes, string[] genericParameterNames
        ) {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            GenericParameterNames = genericParameterNames;
        }

        public int ParameterCount {
            get {
                if (ParameterTypes == null)
                    return 0;

                return ParameterTypes.Length;
            }
        }

        public int GenericParameterCount {
            get {
                if (GenericParameterNames == null)
                    return 0;

                return GenericParameterNames.Length;
            }
        }

        public bool Equals (MethodSignature rhs) {
            if (this == rhs)
                return true;

            if (!ILBlockTranslator.TypesAreEqual(ReturnType, rhs.ReturnType, true))
                return false;

            if (GenericParameterCount != rhs.GenericParameterCount)
                return false;

            if (ParameterCount != rhs.ParameterCount)
                return false;

            for (int i = 0, c = ParameterCount; i < c; i++) {
                if (!ILBlockTranslator.TypesAreEqual(ParameterTypes[i], rhs.ParameterTypes[i], true))
                    return false;
            }

            return true;
        }

        public override bool Equals (object obj) {
            var ms = obj as MethodSignature;

            if (ms != null)
                return Equals(ms);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode () {
            if (_Hash.HasValue)
                return _Hash.Value;

            int hash = GenericParameterCount;

            hash ^= (ParameterCount) << 8;

            _Hash = hash;
            return hash;
        }
    }

    public class MethodSignatureCache {
        struct Key {
            public readonly MethodSignature Signature;

            public Key (MethodSignature signature) {
                Signature = signature;
            }

            public bool Equals (Key rhs) {
                return (Signature == rhs.Signature);
            }

            public override bool Equals (object rhs) {
                if (rhs is Key)
                    return Equals((Key)rhs);

                return base.Equals(rhs);
            }

            public override int GetHashCode () {
                return Signature.GetHashCode();
            }
        }

        private readonly ConcurrentCache<Key, int> IDs;
        private int NextID = 0;

        public MethodSignatureCache () {
            IDs = new ConcurrentCache<Key, int>();
        }

        public int Get (MethodSignature signature) {
            var key = new Key(signature);

            return IDs.GetOrCreate(
                key, () => Interlocked.Increment(ref NextID)
            );
        }
    }

    public class MethodSignatureSet {
        private class Count {
            public int Value = 0;
        }

        private int _Count = 0;
        private readonly ConcurrentCache<MethodSignature, Count> Counts;

        public MethodSignatureSet () {
            Counts = new ConcurrentCache<MethodSignature, Count>(
                2, 8, new MethodSignature.EqualityComparer()
            );
        }

        public void Add (MethodSignature signature) {
            var count = Counts.GetOrCreate(
                signature, () => new Count()
            );

            Interlocked.Increment(ref _Count);
            Interlocked.Increment(ref count.Value);
        }

        public int GetCountOf (MethodSignature signature) {
            Count result;
            if (Counts.TryGet(signature, out result))
                return result.Value;

            return 0;
        }

        public int DefinitionCount {
            get {
                return _Count;
            }
        }

        public int DistinctSignatureCount {
            get {
                return Counts.Count;
            }
        }
    }

    public class MethodSignatureCollection : ConcurrentCache<string, MethodSignatureSet> {
        public MethodSignatureCollection ()
            : base() {
        }

        public int GetOverloadCountOf (string methodName) {
            MethodSignatureSet set;
            if (TryGet(methodName, out set))
                return set.DistinctSignatureCount;

            return 0;
        }

        public int GetDefinitionCountOf (MethodInfo method) {
            MethodSignatureSet set;
            if (TryGet(method.Name, out set))
                return set.GetCountOf(method.Signature);

            return 0;
        }
    }
}
