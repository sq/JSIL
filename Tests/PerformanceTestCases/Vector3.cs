using System;

public static class Program {
    const int BufferSize = 8192;
    const int IterationCount = 512;

    public static Vector3d[] Vectors = new Vector3d[BufferSize];
    public static byte[] PackedVectors;

    public static unsafe void Main () {
        PackedVectors = new byte[sizeof(Vector3d) * BufferSize];

        fixed (byte* pPackedBytes = PackedVectors) {
            var pPackedStructs = (Vector3d*)pPackedBytes;

            for (int i = 0; i < BufferSize; i++) {
                Vectors[i] = new Vector3d(i * 0.5, (double)i, i * 1.5);
                pPackedStructs[i] = Vectors[i];
            }
        }

        Console.WriteLine("Arrays: {0:00000.00}ms", Time(TestArrays));
        Console.WriteLine("PackedPointers: {0:00000.00}ms", Time(TestPackedPointers));
        Console.WriteLine("Pointers: {0:00000.00}ms", Time(TestPointers));
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

    public static unsafe Vector3d TestPackedPointers () {
        fixed (byte* pPackedBytes = PackedVectors) {
            Vector3d sum = new Vector3d();

            var pCurrent = (Vector3d*)pPackedBytes;
            for (int i = 0; i < BufferSize; i++) {
                sum += *pCurrent;
                pCurrent++;
            }

            return sum;
        }
    }

    public static unsafe Vector3d TestPointers () {
        fixed (Vector3d* pVectors = Vectors) {
            Vector3d sum = new Vector3d();

            var pCurrent = pVectors;
            for (int i = 0; i < BufferSize; i++) {
                sum += *pCurrent;
                pCurrent++;
            }

            return sum;
        }
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