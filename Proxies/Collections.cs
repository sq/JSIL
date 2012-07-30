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
            "System.Collections.Hashtable/KeyCollection",
            "System.Collections.Hashtable/ValueCollection",
            "System.Collections.Generic.Dictionary`2/KeyCollection",
            "System.Collections.Generic.Dictionary`2/ValueCollection"
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

    [JSProxy(
        new[] {
            "System.Collections.ArrayList",
            "System.Collections.Generic.List`1",
            "System.Collections.Generic.Stack`1",
            "System.Collections.Generic.Queue`1"
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        inheritable: true
    )]
    public abstract class CollectionProxy2<T> : IEnumerable {
        [JSUnderlyingArray("_items", "_size")]
        System.Collections.IEnumerator IEnumerable.GetEnumerator () {
            throw new InvalidOperationException();
        }

        [JSUnderlyingArray("_items", "_size")]
        public AnyType GetEnumerator () {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        new[] {
            "System.Collections.ArrayList/ArrayListEnumerator",
            "System.Collections.Hashtable/HashtableEnumerator",
            "System.Collections.Generic.List`1/Enumerator",
            "System.Collections.Generic.Stack`1/Enumerator",
            "System.Collections.Generic.Queue`1/Enumerator",
            "System.Collections.Generic.HashSet`1/Enumerator",
            "System.Collections.Generic.Dictionary`2/Enumerator",
            "System.Collections.Generic.Dictionary`2/KeyCollection/Enumerator",
            "System.Collections.Generic.Dictionary`2/ValueCollection/Enumerator"
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        inheritable: false
    )]
    [JSPureDispose]
    public abstract class CollectionEnumeratorProxy {
    }

    [JSProxy(
        new[] {
            "System.Collections.ArrayList/ArrayListEnumerator",
            "System.Collections.Generic.List`1/Enumerator",
            "System.Collections.Generic.Stack`1/Enumerator",
            "System.Collections.Generic.Queue`1/Enumerator",
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        inheritable: false
    )]
    [JSIsArrayEnumerator("_array", "_index", "_length")]
    public abstract class ArrayEnumeratorProxy {
    }
}
