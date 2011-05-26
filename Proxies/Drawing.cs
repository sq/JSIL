using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        "System.Drawing.Color",
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        false
    )]
    public abstract class ColorProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static AnyType FromArgb (params AnyType[] values) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        "System.Drawing.Image",
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        true
    )]
    public abstract class ImageProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public abstract void Save (params AnyType[] values);
    }
}
