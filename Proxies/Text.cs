using System;
using System.Text;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Encoding),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class EncodingProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        public EncodingProxy (params AnyType[] values) {
            throw new InvalidOperationException();
        }
    }
}
