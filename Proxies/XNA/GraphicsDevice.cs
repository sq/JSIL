using System;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Graphics.GraphicsDevice),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class GraphicsDeviceProxy {
        [JSReplacement("$this.InternalClear($color)")]
        public abstract void Clear (Color color);

        [JSReplacement("$this.InternalClear($color)")]
        public abstract void Clear (ClearOptions options, Color color, float depth, int stencil);

        [JSReplacement("$this.InternalClear($color)")]
        public abstract void Clear (ClearOptions options, Vector4 color, float depth, int stencil);

        [JSIgnore]
        public abstract void Clear (ClearOptions options, Vector4 color, float depth, int stencil, Rectangle[] regions);

        [JSIgnore]
        public abstract void Clear (ClearOptions options, Color color, float depth, int stencil, Rectangle[] regions);
    }
}
