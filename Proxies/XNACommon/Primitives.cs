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
        "Microsoft.Xna.Framework.Matrix",
        JSProxyMemberPolicy.ReplaceNone,
        JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceDeclared
    )]
    public abstract class MatrixProxy {
        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Transpose (MatrixProxy matrix) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Transpose (ref MatrixProxy matrix, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Invert (MatrixProxy matrix) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Invert (ref MatrixProxy matrix, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Lerp (MatrixProxy matrix1, MatrixProxy matrix2, float amount) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Lerp (ref MatrixProxy matrix1, ref MatrixProxy matrix2, float amount, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Negate (MatrixProxy matrix) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Negate (ref MatrixProxy matrix, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Add (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Add (ref MatrixProxy matrix1, ref MatrixProxy matrix2, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Subtract (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Subtract (ref MatrixProxy matrix1, ref MatrixProxy matrix2, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Multiply (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Multiply (ref MatrixProxy matrix1, ref MatrixProxy matrix2, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Multiply (MatrixProxy matrix1, float scaleFactor) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Multiply (ref MatrixProxy matrix1, float scaleFactor, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Divide (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Divide (ref MatrixProxy matrix1, ref MatrixProxy matrix2, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy Divide (MatrixProxy matrix1, float divider) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Divide (ref MatrixProxy matrix1, float divider, out MatrixProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator - (MatrixProxy matrix1) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator == (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator != (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator + (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator - (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator * (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator * (MatrixProxy matrix, float scaleFactor) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator * (float scaleFactor, MatrixProxy matrix) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator / (MatrixProxy matrix1, MatrixProxy matrix2) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static MatrixProxy operator / (MatrixProxy matrix1, float divider) {
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
        [JSResultIsNew]
        public static AnyType Dot (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Dot (ref VectorProxy a, ref VectorProxy b, out AnyType result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Multiply (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Multiply (ref VectorProxy a, ref VectorProxy b, out VectorProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Multiply (VectorProxy a, float b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Multiply (ref VectorProxy a, float b, out VectorProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Divide (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Divide (ref VectorProxy a, ref VectorProxy b, out VectorProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Divide (VectorProxy a, float b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Divide (ref VectorProxy a, float b, out VectorProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Add (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Add (ref VectorProxy a, ref VectorProxy b, out VectorProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Subtract (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Subtract (ref VectorProxy a, ref VectorProxy b, out VectorProxy result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy Normalize (VectorProxy v) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static float Distance (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void Distance (ref VectorProxy a, ref VectorProxy b, out float result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static float DistanceSquared (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static void DistanceSquared (ref VectorProxy a, ref VectorProxy b, out float result) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator == (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static bool operator != (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator / (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator / (VectorProxy a, float b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator * (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator * (VectorProxy a, float b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator * (float a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator - (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator + (VectorProxy a, VectorProxy b) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSResultIsNew]
        public static VectorProxy operator - (VectorProxy a) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public bool Equals (VectorProxy other) {
            throw new InvalidOperationException();
        }
    }
}
