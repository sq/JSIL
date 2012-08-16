using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static int NextID = 0;

        internal int ID;

        protected int? _Hash;

        public MethodSignature (
            TypeReference returnType, TypeReference[] parameterTypes, string[] genericParameterNames
        ) {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            GenericParameterNames = genericParameterNames;
            ID = Interlocked.Increment(ref NextID);
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

            if (GenericParameterCount != rhs.GenericParameterCount)
                return false;

            if (ParameterCount != rhs.ParameterCount)
                return false;

            for (int i = 0, c = GenericParameterNames.Length; i < c; i++) {
                if (GenericParameterNames[i] != rhs.GenericParameterNames[i])
                    return false;
            }

            if (!TypeUtil.TypesAreEqual(ReturnType, rhs.ReturnType, true))
                return false;

            for (int i = 0, c = ParameterCount; i < c; i++) {
                if (!TypeUtil.TypesAreEqual(ParameterTypes[i], rhs.ParameterTypes[i], true))
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

            int hash = ParameterCount;

            if ((ReturnType != null) && !TypeUtil.IsOpenType(ReturnType))
                hash ^= (ReturnType.Name.GetHashCode() << 8);

            if ((ParameterCount > 0) && !TypeUtil.IsOpenType(ParameterTypes[0]))
                hash ^= (ParameterTypes[0].Name.GetHashCode() << 20);

            hash ^= (GenericParameterCount) << 24;

            _Hash = hash;
            return hash;
        }

        public override string ToString () {
            if (GenericParameterCount > 0) {
                return String.Format(
                    "<{0}>({1})",
                    String.Join(",", GenericParameterNames),
                    String.Join(",", (from p in ParameterTypes select p.ToString()))
                );
            } else {
                return String.Format(
                    "({0})",
                    String.Join(",", (from p in ParameterTypes select p.ToString()))
                );
            }
        }
    }

    public class NamedMethodSignature {
        public readonly MethodSignature Signature;
        public readonly string Name;

        private int? _Hash;

        public NamedMethodSignature (string name, MethodSignature signature) {
            Name = name;
            Signature = signature;
            _Hash = null;
        }

        public override int GetHashCode() {
            if (!_Hash.HasValue)
                _Hash = (Name.GetHashCode() ^ Signature.GetHashCode());

            return _Hash.Value;
        }

        public class Comparer : IEqualityComparer<NamedMethodSignature> {
            public bool Equals (NamedMethodSignature x, NamedMethodSignature y) {
                var result = (x.Name == y.Name) && (x.Signature.Equals(y.Signature));
                return result;
            }

            public int GetHashCode (NamedMethodSignature obj) {
                return obj.GetHashCode();
            }
        }

        public override string ToString () {
            return String.Format("{0}{1}", Name, Signature);
        }
    }

    public class MethodSignatureSet : IDisposable {
        public class Count {
            public volatile int Value;
        }

        private volatile int _Count = 0;
        private readonly Dictionary<NamedMethodSignature, Count> Counts;
        private readonly string Name;

        internal MethodSignatureSet (MethodSignatureCollection collection, string name) {
            Counts = collection.Counts;
            Name = name;
        }

        public MethodSignature[] Signatures {
            get {
                lock (Counts)
                    return (from k in Counts.Keys where k.Name == this.Name select k.Signature).ToArray();
            }
        }

        public void Dispose () {
        }

        public void Add (NamedMethodSignature signature) {
            // if (signature.Name != Name)
            //     throw new InvalidOperationException();

            Count c;

            lock (Counts) {
                if (!Counts.TryGetValue(signature, out c))
                    Counts.Add(signature, c = new Count());
            }

            Interlocked.Increment(ref c.Value);
            Interlocked.Increment(ref _Count);
        }

        public int GetCountOf (NamedMethodSignature signature) {
            Count result;
            lock (Counts)
                if (Counts.TryGetValue(signature, out result))
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
                int result = 0;

                lock (Counts)
                    foreach (var key in Counts.Keys)
                        if (key.Name == this.Name)
                            result += 1;

                return result;
            }
        }
    }

    public class MethodSignatureCollection : IDisposable {
        const int InitialCapacity = 16;

        internal readonly Dictionary<string, MethodSignatureSet> Sets;
        internal readonly Dictionary<NamedMethodSignature, MethodSignatureSet.Count> Counts;

        public MethodSignatureCollection () {
            Sets = new Dictionary<string, MethodSignatureSet>(InitialCapacity, StringComparer.Ordinal);

            Counts = new Dictionary<NamedMethodSignature, MethodSignatureSet.Count>(
                InitialCapacity, new NamedMethodSignature.Comparer()
            );
        }

        public int GetOverloadCountOf (string methodName) {
            MethodSignatureSet set;
            if (TryGet(methodName, out set))
                return set.DistinctSignatureCount;

            return 0;
        }

        public int GetDefinitionCountOf (MethodInfo method) {
            MethodSignatureSet set;
            var namedSignature = method.NamedSignature;

            if (TryGet(namedSignature.Name, out set))
                return set.GetCountOf(namedSignature);

            return 0;
        }

        public MethodSignatureSet GetOrCreateFor (string methodName) {
            lock (Sets) {
                MethodSignatureSet result;

                if (!Sets.TryGetValue(methodName, out result)) {
                    methodName = methodName;
                    Sets[methodName] = result = new MethodSignatureSet(this, methodName);
                }

                return result;
            }
        }

        public bool TryGet (string methodName, out MethodSignatureSet result) {
            lock (Sets)
                return Sets.TryGetValue(methodName, out result);
        }

        public void Dispose () {
        }
    }
}
