//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    const int BufferSize = 10240;
    const int IterationCount = 320;

    public static Vertex[] VerticesN;
    [JSPackedArray]
    public static Vertex[] VerticesP;

    public static unsafe void Main () {
        VerticesN = new Vertex[BufferSize];
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

        Console.WriteLine("Packed Array Alternate: {0:00000.00}ms", Time(TestPackedArrayAlternate));
        Console.WriteLine("Packed Array: {0:00000.00}ms", Time(TestPackedArray));
        Console.WriteLine("Array: {0:00000.00}ms", Time(TestArray));
    }

    public static int Time (Action func) {
        var started = Environment.TickCount;

        for (int i = 0; i < IterationCount; i++) {
            func();
        }

        var ended = Environment.TickCount;
        return ended - started;
    }

    public static void TestArray () {
        for (int i = 0; i < BufferSize; i++) {
            var x = VerticesN[i].Normals;
            VerticesN[i].Normals = VerticesN[i].Position;
            VerticesN[i].Position = x;
            VerticesN[i].UVs = VerticesN[i].UVs - new float2(1, 1);
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

    public static void TestPackedArrayAlternate () {
        for (int i = 0; i < BufferSize; i++) {
            var item = VerticesP[i];
            var x = item.Normals;

            item.Normals = item.Position;
            item.Position = x;
            item.UVs = item.UVs - new float2(1, 1);

            VerticesP[i] = item;
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