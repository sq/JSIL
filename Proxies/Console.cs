using System;
using JSIL.Meta;

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
