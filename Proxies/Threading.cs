using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(System.Threading.Interlocked),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class InterlockedProxy {
        [JSRuntimeDispatch]
        public static void CompareExchange (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof(System.Threading.Monitor),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class MonitorProxy {
        [JSRuntimeDispatch]
        public static void Enter (AnyType o) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        public static void Exit (AnyType o) {
            throw new InvalidOperationException();
        }
    }
}
