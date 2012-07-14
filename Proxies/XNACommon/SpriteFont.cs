using System;
using System.Text;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Graphics.SpriteFont)
    )]
    public abstract class SpriteFontProxy {
        [JSChangeName("MeasureStringBuilder")]
        public abstract Vector2 MeasureString (StringBuilder text);
    }
}
