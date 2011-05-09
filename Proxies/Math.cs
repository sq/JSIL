using System;
using JSIL.Meta;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Math),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class MathProxy {
        [JSRuntimeDispatch]
        new public static void Min () {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        new public static void Max () {
            throw new InvalidOperationException();
        }
    }
}
