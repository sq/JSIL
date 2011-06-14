using System;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Xna.Framework.Input;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Input.Keyboard),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class KeyboardProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static KeyboardState GetState (params AnyType[] values) {
            throw new NotImplementedException();
        }
    }
}
