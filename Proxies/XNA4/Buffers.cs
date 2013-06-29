using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Proxy;
using JSIL.Meta;
using Microsoft.Xna.Framework;

namespace JSIL.Proxies.XNA {
    [JSProxy(
        typeof(Microsoft.Xna.Framework.Graphics.VertexBuffer),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class VertexBufferProxy {
        [JSAllowPackedArrayArguments]
        public void SetData<T> (T[] data) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void SetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct {
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
        public void GetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct {
            throw new NotImplementedException();
        }
    }

    [JSProxy(
        typeof(Microsoft.Xna.Framework.Graphics.IndexBuffer),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class IndexBufferProxy {
        [JSAllowPackedArrayArguments]
        public void SetData<T> (T[] data) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public void SetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct {
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
        public void GetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct {
            throw new NotImplementedException();
        }
    }
}
