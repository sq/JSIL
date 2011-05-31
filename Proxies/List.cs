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
    public abstract class ListProxy<T> {
        [JSRuntimeDispatch]
        [JSExternal]
        public ListProxy (params AnyType[] values) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.GetEnumerator()")]
        [JSExternal]
        public abstract AnyType GetEnumerator ();
    }

    [JSProxy(
        new[] {
            "System.Collections.IEnumerable",
            "System.Collections.Generic.IEnumerable`1"
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared,
        interfacePolicy: JSProxyInterfacePolicy.ReplaceNone,
        inheritable: true
    )]
    public abstract class IEnumerableProxy<T> : IEnumerable<T> {
        [JSReplacement("$this.GetEnumerator()")]
        IEnumerator IEnumerable.GetEnumerator () {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.GetEnumerator()")]
        IEnumerator<T> IEnumerable<T>.GetEnumerator () {
            throw new InvalidOperationException();
        }
    }
}
