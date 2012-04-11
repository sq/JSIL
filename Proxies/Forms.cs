using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        "System.Windows.Forms.AccessibleObject",
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceAll,
        JSProxyInterfacePolicy.ReplaceAll,
        inheritable: true
    )]
    [JSIgnore]
    public abstract class AccessibleObjectProxy {
    }
}
