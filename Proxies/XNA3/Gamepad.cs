using System;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Xna.Framework.Input;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Input.GamePad),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class GamePadProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static GamePadState GetState (params AnyType[] values) {
            throw new NotImplementedException();
        }
    }

    [JSProxy(
        typeof(Microsoft.Xna.Framework.Input.GamePadState),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class GamePadStateProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public GamePadStateProxy (params AnyType[] values) {
            throw new NotImplementedException();
        }
    }
}
