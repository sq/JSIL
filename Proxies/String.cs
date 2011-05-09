using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(String),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class StringProxy {
        [JSRuntimeDispatch]
        new public static string Format () {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        new public static string Concat () {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        new public string[] Split () {
            throw new InvalidOperationException();
        }
    }
}
