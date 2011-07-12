using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Console),
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class ConsoleProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        new public static void WriteLine (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        new public static void Write (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
