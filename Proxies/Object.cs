using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Object),
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class ObjectProxy {
        [JSIsPure]
        [JSExternal]
        new abstract public Type GetType ();

        [JSExternal]
        [JSNeverReplace]
        new abstract public AnyType MemberwiseClone ();

        [JSChangeName("toString")]
        [JSNeverReplace]
        [JSRuntimeDispatch]
        new abstract public string ToString ();

        [JSIsPure]
        [JSNeverReplace]
        [JSRuntimeDispatch]
        new public abstract bool Equals (object obj);

        [JSIsPure]
        [JSReplacement("JSIL.ObjectEquals($objA, $objB)")]
        public static bool Equals (object objA, object objB) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSReplacement("$objA === $objB")]
        public static bool ReferenceEquals (object objA, object objB) {
            throw new InvalidOperationException();
        }
    }
}
