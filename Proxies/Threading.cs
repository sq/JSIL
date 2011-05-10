using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(System.Threading.Interlocked),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class InterlockedProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        public static AnyType CompareExchange (ref AnyType destination, AnyType value, AnyType comparand) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof(System.Threading.Monitor),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class MonitorProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        public static void Enter (AnyType o) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        public static void Exit (AnyType o) {
            throw new InvalidOperationException();
        }
    }
}
