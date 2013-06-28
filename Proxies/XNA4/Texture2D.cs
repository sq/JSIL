using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Proxy;
using JSIL.Meta;
using Microsoft.Xna.Framework;

namespace JSIL.Proxies.XNA {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Graphics.Texture2D),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class Texture2DProxy {
        [JSAllowPackedArrayArguments]
        public void SetData<T> (T[] data) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void SetData<T> (int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void GetData<T> (T[] data) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void GetData<T> (T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void GetData<T> (int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }
    }
}
