using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        "Microsoft.Xna.Framework.Rectangle",
        JSProxyMemberPolicy.ReplaceNone,
        JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceDeclared
    )]
    public abstract class RectangleProxy {
        [JSIsPure]
        public bool Contains (int x, int y) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public bool Contains (AnyType value) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public void Contains (ref AnyType value, out bool result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public bool Intersects (AnyType value) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public void Intersects (ref AnyType value, out bool result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public bool Equals (RectangleProxy other) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator == (RectangleProxy a, RectangleProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator != (RectangleProxy a, RectangleProxy b) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        "Microsoft.Xna.Framework.Point",
        JSProxyMemberPolicy.ReplaceNone,
        JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceDeclared
    )]
    public abstract class PointProxy {
        [JSIsPure]
        public static bool operator == (PointProxy a, PointProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator != (PointProxy a, PointProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public bool Equals (PointProxy other) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        new[] {
            "Microsoft.Xna.Framework.Vector2",
            "Microsoft.Xna.Framework.Vector3",
            "Microsoft.Xna.Framework.Vector4",
        },
        JSProxyMemberPolicy.ReplaceNone,
        JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceDeclared
    )]
    public abstract class VectorProxy {
        [JSIsPure]
        public static bool operator == (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator != (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator / (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator / (VectorProxy a, float b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator * (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator * (VectorProxy a, float b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator * (float a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator - (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static VectorProxy operator + (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public bool Equals (VectorProxy other) {
            throw new InvalidOperationException();
        }
    }
}
