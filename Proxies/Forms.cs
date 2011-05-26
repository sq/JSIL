using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        "System.Windows.Forms.AccessibleObject",
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceAll,
        false
    )]
    public abstract class AccessibleObjectProxy {
    }
}
