using System;
using JSIL.Meta;

public static class Program {
    const int BufferSize = 8192;
    const int IterationCount = 320;

    public static Vector3d[] Vectors = new Vector3d[BufferSize];
    [JSPackedArray]
    public static Vector3d[] PackedVectors = new Vector3d[BufferSize];
    public static byte[] ManuallyPackedVectors;

    public static unsafe void Main () {
        ManuallyPackedVectors = new byte[sizeof(Vector3d) * BufferSize];

        fixed (byte* pPackedBytes = ManuallyPackedVectors) {
            var pPackedStructs = (Vector3d*)pPackedBytes;

            for (int i = 0; i < BufferSize; i++) {
                Vectors[i] = new Vector3d(i * 0.5, (double)i, i * 1.5);
                PackedVectors[i] = Vectors[i];
                pPackedStructs[i] = Vectors[i];
            }
        }

        Console.WriteLine("Arrays: {0:00000.00}ms", Time(TestArrays));
        Console.WriteLine("ManuallyPackedStructs: {0:00000.00}ms", Time(TestManuallyPackedStructs));
        Console.WriteLine("PackedStructs: {0:00000.00}ms", Time(TestPackedStructs));
        Console.WriteLine("PackedStructPointers: {0:00000.00}ms", Time(TestPackedStructPointers));
    }

    public static int Time (Func<Vector3d> func) {
        var started = Environment.TickCount;

        Vector3d sum = default(Vector3d);

        for (int i = 0; i < IterationCount; i++) {
            sum = func();
        }

        Console.WriteLine(sum);

        var ended = Environment.TickCount;
        return ended - started;
    }

    public static Vector3d TestArrays () {
        Vector3d sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum += Vectors[i];
        }

        return sum;
    }

    public static unsafe Vector3d TestManuallyPackedStructs () {
        fixed (byte* pPackedBytes = ManuallyPackedVectors) {
            Vector3d sum = new Vector3d();

            var pCurrent = (Vector3d*)pPackedBytes;
            for (int i = 0; i < BufferSize; i++) {
                sum += *pCurrent;
                pCurrent++;
            }

            return sum;
        }
    }

    public static unsafe Vector3d TestPackedStructs () {
        Vector3d sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum += PackedVectors[i];
        }

        return sum;
    }

    public static unsafe Vector3d TestPackedStructPointers () {
        Vector3d sum = new Vector3d();

        fixed (Vector3d* pStructs = PackedVectors) {
            var pCurrent = pStructs;

            for (int i = 0; i < BufferSize; i++) {
                sum += *pCurrent;
                pCurrent++;
            }
        }

        return sum;
    }
}

public struct Vector3d {
    public readonly double x, y, z;

    public Vector3d (double x = 0, double y = 0, double z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public double Dot (Vector3d b) {
        return (x * b.x + y * b.y + z * b.z);
    }

    public Vector3d Normalize () {
        double d = 1.0 / this.Magnitude();

        return new Vector3d(
            x * d, y * d, z * d
        );
    }

    public double Magnitude () {
        return Math.Sqrt(x * x + y * y + z * z);
    }

    public static Vector3d operator + (Vector3d a, Vector3d b) {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3d operator - (Vector3d a, Vector3d b) {
        return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3d operator - (Vector3d a) {
        return new Vector3d(-a.x, -a.y, -a.z);
    }

    public override string ToString () {
        return String.Format("<{0:00000000.00}, {1:00000000.00}, {2:00000000.00}>", x, y, z);
    }
}