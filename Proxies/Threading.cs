using System;
using JSIL.Meta;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(System.Threading.Interlocked),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class InterlockedProxy {
        [JSRuntimeDispatch]
        public static void CompareExchange () {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof(System.Threading.Monitor),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class MonitorProxy {
        [JSRuntimeDispatch]
        public static void Enter () {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        public static void Exit () {
            throw new InvalidOperationException();
        }
    }
}
