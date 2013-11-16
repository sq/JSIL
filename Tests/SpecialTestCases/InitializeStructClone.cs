//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    const int BufferSize = 1024;

    public static Vertex[] VerticesN;
    [JSPackedArray]
    public static Vertex[] VerticesP;

    public static unsafe void Main () {
        VerticesP = new Vertex[BufferSize];

        for (int i = 0; i < BufferSize; i++) {
            var vertex = new Vertex {
                Position = new float3(i, i * 1.5f, i * 2f),
                Normals = new float3(i * 2f, i * 2.5f, i * 3f),
                UVs = new float2(i * 8f, i * 16f)
            };

            VerticesN[i] = vertex;
            VerticesP[i] = vertex;
        }
    }

    public static void TestPackedArray () {
        for (int i = 0; i < BufferSize; i++) {
            var x = VerticesP[i].Normals;
            VerticesP[i].Normals = VerticesP[i].Position;
            VerticesP[i].Position = x;
            VerticesP[i].UVs = VerticesP[i].UVs - new float2(1, 1);
        }
    }

}

public struct float2 {
    public float X, Y;

    public float2 (float x, float y) {
        X = x;
        Y = y;
    }

    public static float2 operator - (float2 lhs, float2 rhs) {
        return new float2(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }
}

public struct float3 {
    public float X, Y, Z;

    public float3 (float x, float y, float z) {
        X = x;
        Y = y;
        Z = z;
    }
}

public struct Vertex {
    public float3 Position;
    public float3 Normals;
    public float2 UVs;
}