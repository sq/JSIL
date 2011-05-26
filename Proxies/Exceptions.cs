using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Exception),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        false
    )]
    public abstract class ExceptionProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public ExceptionProxy () {
        }
    }
}
