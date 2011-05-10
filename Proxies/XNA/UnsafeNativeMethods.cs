using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new [] { 
            "Microsoft.Xna.Framework.Audio.UnsafeNativeMethods",
            "Microsoft.Xna.Framework.Audio.SoundEffectUnsafeNativeMethods",
            "Microsoft.Xna.Framework.GamerServices.UnsafeNativeMethods",
            "Microsoft.Xna.Framework.Media.UnsafeNativeMethods",
            "Microsoft.Xna.Framework.Input.UnsafeNativeMethods",
            "Microsoft.Xna.Framework.Storage.UnsafeNativeMethods"
        },
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    [JSIgnore]
    public abstract class UnsafeNativeMethodsProxy {
    }
}
