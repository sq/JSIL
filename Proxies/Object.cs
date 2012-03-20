using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Object),
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared,
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

        [JSExternal]
        [JSIsPure]
        [JSNeverReplace]
        new abstract public bool Equals (object obj);
    }
}
