using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Array),
        memberPolicy: JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class ArrayProxy {
        [JSChangeName("length")]
        abstract public int Length { get; }

        [JSExternal]
        public void Set (AnyType x, AnyType value) {
            throw new NotImplementedException();
        }

        [JSExternal]
        public void Set (AnyType x, AnyType y, AnyType value) {
            throw new NotImplementedException();
        }

        [JSExternal]
        public void Set (AnyType x, AnyType y, AnyType z, AnyType value) {
            throw new NotImplementedException();
        }

        [JSExternal]
        public AnyType Get (AnyType x) {
            throw new NotImplementedException();
        }

        [JSExternal]
        public AnyType Get (AnyType x, AnyType y) {
            throw new NotImplementedException();
        }

        [JSExternal]
        public AnyType Get (AnyType x, AnyType y, AnyType z) {
            throw new NotImplementedException();
        }
    }
}
