using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Cecil;

namespace JSIL.Internal {
    public class MethodSignature {
        public readonly TypeReference ReturnType;
        public readonly TypeReference[] ParameterTypes;
        public readonly string[] GenericParameterNames;

        public MethodSignature (
            TypeReference returnType, TypeReference[] parameterTypes, string[] genericParameterNames
        ) {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            GenericParameterNames = genericParameterNames;
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
}
