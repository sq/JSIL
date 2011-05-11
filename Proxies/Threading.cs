using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(System.Threading.Interlocked),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class InterlockedProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        public static AnyType CompareExchange (ref AnyType destination, AnyType value, AnyType comparand) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        public static int CompareExchange (ref int location1, int value, int comparand, ref bool succeeded) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof(System.Threading.Monitor),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class MonitorProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        public static void Enter (Object o) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        public static void Enter (Object o, ref bool lockTaken) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        public static void Exit (Object o) {
            throw new InvalidOperationException();
        }
    }
}
