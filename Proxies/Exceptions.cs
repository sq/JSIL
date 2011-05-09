using System;
using JSIL.Meta;

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
