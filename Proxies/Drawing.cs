using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        "System.Drawing.Color",
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        inheritable: false
    )]
    public abstract class ColorProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static AnyType FromArgb (params AnyType[] values) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        "System.Drawing.Image"
    )]
    public abstract class ImageProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public abstract void Save (params AnyType[] values);
    }

    [JSProxy(
        new[] { "System.Drawing.Size", "System.Drawing.Rectangle", "System.Drawing.Point" }
    )]
    public abstract class UnitsProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public UnitsProxy (params AnyType[] values) {
        }
    }
}
