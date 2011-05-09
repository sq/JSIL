using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Exception),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class ExceptionProxy {
        [JSRuntimeDispatch]
        public ExceptionProxy () {
        }
    }
}
