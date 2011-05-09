using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Console),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class ConsoleProxy {
        [JSRuntimeDispatch]
        new public static void WriteLine () {
            throw new InvalidOperationException();
        }
    }
}
