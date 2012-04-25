using System;
using System.Collections;
using System.Collections.Generic;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new[] {
            "System.Collections.ArrayList",
            "System.Collections.Hashtable",
            "System.Collections.Generic.List`1",
            "System.Collections.Generic.Dictionary`2",
            "System.Collections.Generic.Stack`1",
            "System.Collections.Generic.Queue`1",
            "System.Collections.Generic.HashSet`1",
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        inheritable: false
    )]
    public abstract class CollectionProxy<T> : IEnumerable {
        [JSIsPure]
        [JSResultIsNew]
        System.Collections.IEnumerator IEnumerable.GetEnumerator () {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public AnyType GetEnumerator () {
            throw new InvalidOperationException();
        }
    }
}
