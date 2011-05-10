using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Exception),
        JSProxyMemberPolicy.ReplaceNone,
        JSProxyAttributePolicy.ReplaceDeclared,
        false
    )]
    public abstract class ExceptionProxy {
        [JSRuntimeDispatch]
        public ExceptionProxy () {
        }
    }
}
