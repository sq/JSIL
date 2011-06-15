using System;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch),
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class SpriteBatchProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public abstract void Begin (params AnyType[] values);

        [JSReplacement("$this.InternalDraw($image, $position, null, $color, 0, null, 1)")]
        public abstract void Draw (Texture2D image, Vector2 position, Color color);

        [JSReplacement("$this.InternalDraw($image, $position, $sourceRectangle, $color, 0, null, 1)")]
        public abstract void Draw (Texture2D image, Vector2 position, Rectangle? sourceRectangle, Color color);

        [JSReplacement("$this.InternalDraw($image, $position, $sourceRectangle, $color, $rotation, $origin, $scale, $effects)")]
        public abstract void Draw (Texture2D image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        [JSReplacement("$this.InternalDraw($image, $position, $sourceRectangle, $color, $rotation, $origin, $scale, $effects)")]
        public abstract void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        [JSReplacement("$this.InternalDraw($image, $destinationRectangle, null, $color, 0, null)")]
        public abstract void Draw (Texture2D image, Rectangle destinationRectangle, Color color);

        [JSReplacement("$this.InternalDraw($image, $destinationRectangle, $sourceRectangle, $color, 0, null)")]
        public abstract void Draw (Texture2D image, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color);

        [JSReplacement("$this.InternalDraw($image, $destinationRectangle, $sourceRectangle, $color, $rotation, $origin, 1, $effects)")]
        public abstract void Draw (Texture2D image, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth);

        [JSReplacement("$this.InternalDrawString($font, $text, $position, $color, 1)")]
        public abstract void DrawString (SpriteFont font, string text, Vector2 position, Color color);

        [JSReplacement("$this.InternalDrawString($font, $text, $position, $color, $scale, $effects)")]
        public abstract void DrawString (SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        [JSReplacement("$this.InternalDrawString($font, $text, $position, $color, $scale, $effects)")]
        public abstract void DrawString (SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
    }
}
