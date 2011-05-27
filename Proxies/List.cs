using System;
using System.Collections;
using System.Collections.Generic;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new [] {
            "System.Collections.ArrayList",
            "System.Collections.Generic.List`1"
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared,
        interfacePolicy: JSProxyInterfacePolicy.ReplaceNone,
        inheritable: true
    )]
    public abstract class ListProxy<T> : IEnumerable<T> {
        [JSRuntimeDispatch]
        [JSExternal]
        public ListProxy (params AnyType[] values) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        public abstract AnyType GetEnumerator ();

        [JSRuntimeDispatch]
        [JSExternal]
        IEnumerator IEnumerable.GetEnumerator () {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        IEnumerator<T> IEnumerable<T>.GetEnumerator () {
            throw new InvalidOperationException();
        }
    }
}
