using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Object)
    )]
    public abstract class ObjectProxy {
        [JSReplacement("JSIL.GetType($this)")]
        new abstract public Type GetType ();

        [JSExternal]
        [JSNeverReplace]
        new abstract public AnyType MemberwiseClone ();

        [JSChangeName("toString")]
        [JSNeverReplace]
        [JSRuntimeDispatch]
        new abstract public string ToString ();
    }
}
