using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new [] { typeof(Delegate), typeof (MulticastDelegate) },
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class DelegateProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static AnyType Combine (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
