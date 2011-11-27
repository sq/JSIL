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
            "Microsoft.Xna.Framework.Storage.UnsafeNativeMethods",
            "Microsoft.Xna.Framework.Content.NativeMethods",
            "Microsoft.Xna.Framework.SystemNativeMethods",
            "Microsoft.Xna.Framework.NativeMethods",
            "Microsoft.Xna.Framework.NativeMethods.Message",
            "Microsoft.Xna.Framework.NativeMethods.MinMaxInformation",
            "Microsoft.Xna.Framework.NativeMethods.MonitorInformation",
            "Microsoft.Xna.Framework.WindowsGameForm",
            "Microsoft.Win32.UnsafeNativeMethods"
        },
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceAll,
        JSProxyInterfacePolicy.ReplaceAll
    )]
    [JSIgnore]
    public abstract class UnsafeNativeMethodsProxy {
    }
}
