using System;
using System.Text;
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

        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D image, Vector2 position, Color color);

        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D image, Vector2 position, Rectangle? sourceRectangle, Color color);

        [JSChangeName("DrawScaleF")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        [JSChangeName("DrawRect")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D image, Rectangle destinationRectangle, Color color);

        [JSChangeName("DrawRect")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D image, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color);

        [JSChangeName("DrawRect")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void Draw (Texture2D image, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth);

        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void DrawString (SpriteFont font, string text, Vector2 position, Color color);

        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void DrawString (SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        [JSChangeName("DrawStringScaleF")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void DrawString (SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        [JSChangeName("DrawStringBuilder")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void DrawString (SpriteFont font, StringBuilder text, Vector2 position, Color color);

        [JSChangeName("DrawStringBuilder")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void DrawString (SpriteFont font, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        [JSChangeName("DrawStringBuilderScaleF")]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public abstract void DrawString (SpriteFont font, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
    }
}
