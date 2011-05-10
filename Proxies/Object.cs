using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Object),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class ObjectProxy {
        [JSReplacement("JSIL.GetType($this)")]
        new abstract public Type GetType ();

        [JSExternal]
        [JSNeverReplace]
        new abstract public AnyType MemberwiseClone ();

        [JSChangeName("toString")]
        new abstract public string ToString ();
    }
}
