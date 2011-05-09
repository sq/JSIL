using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new [] { typeof(Delegate), typeof (MulticastDelegate) },
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class DelegateProxy {
        [JSRuntimeDispatch]
        public static void Combine (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
