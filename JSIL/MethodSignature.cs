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

            if (!TypeUtil.TypesAreEqual(ReturnType, rhs.ReturnType, true))
                return false;

            if (GenericParameterCount != rhs.GenericParameterCount)
                return false;

            if (ParameterCount != rhs.ParameterCount)
                return false;

            for (int i = 0, c = ParameterCount; i < c; i++) {
                if (!TypeUtil.TypesAreEqual(ParameterTypes[i], rhs.ParameterTypes[i], true))
                    return false;
            }

            for (int i = 0, c = GenericParameterNames.Length; i < c; i++) {
                if (GenericParameterNames[i] != rhs.GenericParameterNames[i])
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

            int hash = 0;

            if ((ReturnType != null) && !TypeUtil.IsOpenType(ReturnType))
                hash = ReturnType.Name.GetHashCode();

            if ((ParameterCount > 0) && !TypeUtil.IsOpenType(ParameterTypes[0]))
                hash ^= (ParameterTypes[0].Name.GetHashCode() << 16);

            hash ^= (GenericParameterCount) << 24;

            hash ^= (ParameterCount) << 28;

            _Hash = hash;
            return hash;
        }
    }

    public struct NamedMethodSignature {
        public readonly MethodSignature Signature;
        public readonly string Name;

        public NamedMethodSignature (string name, MethodSignature signature) {
            Name = name;
            Signature = signature;
        }

        public override int GetHashCode() {
 	        return Name.GetHashCode() ^ Signature.GetHashCode();
        }

        public class Comparer : IEqualityComparer<NamedMethodSignature> {
            public bool Equals (NamedMethodSignature x, NamedMethodSignature y) {
                return (x.Name == y.Name) && (x.Signature.Equals(y.Signature));
            }

            public int GetHashCode (NamedMethodSignature obj) {
                return obj.GetHashCode();
            }
        }
    }

    public class MethodSignatureSet : IDisposable {
        internal class Count {
            public int Value = 0;
        }

        private int _Count = 0;
        private readonly ConcurrentCache<NamedMethodSignature, Count> Counts;
        private readonly string Name;

        internal MethodSignatureSet (MethodSignatureCollection collection, string name) {
            Counts = collection.Counts;
            Name = name;
        }

        public IEnumerable<MethodSignature> Signatures {
            get {
                foreach (var key in Counts.Keys)
                    if (key.Name == this.Name)
                        yield return key.Signature;
            }
        }

        public void Dispose () {
        }

        public void Add (MethodSignature signature) {
            var count = Counts.GetOrCreate(
                new NamedMethodSignature(Name, signature), () => new Count()
            );

            Interlocked.Increment(ref _Count);
            Interlocked.Increment(ref count.Value);
        }

        public int GetCountOf (MethodSignature signature) {
            Count result;
            if (Counts.TryGet(new NamedMethodSignature(Name, signature), out result))
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

                foreach (var key in Counts.Keys)
                    if (key.Name == this.Name)
                        result += 1;

                return result;
            }
        }
    }

    public class MethodSignatureCollection : ConcurrentCache<string, MethodSignatureSet>, IDisposable {
        internal readonly ConcurrentCache<NamedMethodSignature, MethodSignatureSet.Count> Counts;

        public MethodSignatureCollection ()
            : base(1, 8, StringComparer.Ordinal) {

            Counts = new ConcurrentCache<NamedMethodSignature, MethodSignatureSet.Count>(
                1, 8, new NamedMethodSignature.Comparer()
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
            if (TryGet(method.Name, out set))
                return set.GetCountOf(method.Signature);

            return 0;
        }

        public MethodSignatureSet GetOrCreateFor (string methodName) {
            return GetOrCreate(
                methodName, () => new MethodSignatureSet(this, methodName)
            );
        }
    }
}
