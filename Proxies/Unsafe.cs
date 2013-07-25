using System;
using System.Runtime.InteropServices;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(GCHandle),
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class GCHandleProxy {
        [JSAllowPackedArrayArguments]
        public static GCHandle Alloc (object value) {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public static GCHandle Alloc (object value, GCHandleType type) {
            throw new NotImplementedException();
        }
    }
}
