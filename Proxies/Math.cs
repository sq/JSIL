using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Math),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class MathProxy {
        [JSRuntimeDispatch]
        new public static void Min (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        new public static void Max (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
