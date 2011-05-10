using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Enum),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class EnumProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        new public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
