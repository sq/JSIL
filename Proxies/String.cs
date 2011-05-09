using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(String),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class StringProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Format (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        public abstract string[] Split (params AnyType[] arguments);
    }
}
