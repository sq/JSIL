using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Enum),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class EnumProxy {
        [JSRuntimeDispatch]
        new public static object Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
