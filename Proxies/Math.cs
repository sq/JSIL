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
        public static AnyType Min (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        public static AnyType Max (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
