using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Game),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class GameProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        public abstract void Dispose (params AnyType[] values);
    }
}
