//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    const int BufferSize = 8192;
    const int IterationCount = 512;

    public static Vector3d[] Vectors = new Vector3d[BufferSize];

    public static unsafe void Main () {
        for (int i = 0; i < BufferSize; i++) {
            Vectors[i] = new Vector3d(i * 0.5, (double)i, i * 1.5);
        }

        Console.WriteLine("Fields: {0:00000.00}ms", Time(TestFields));
        Console.WriteLine("Properties: {0:00000.00}ms", Time(TestProperties));
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

    public static Vector3d TestProperties () {
        Vector3d sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum = sum.AddWithProperties(Vectors[i]);
        }

        return sum;
    }

    public static Vector3d TestFields () {
        Vector3d sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum = sum.AddWithFields(Vectors[i]);
        }

        return sum;
    }
}

public struct Vector3d {
    public readonly double x, y, z;

    public double X {
        get {
            return x;
        }
    }

    public double Y {
        get {
            return y;
        }
    }

    public double Z {
        get {
            return z;
        }
    }

    public Vector3d (double x = 0, double y = 0, double z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3d AddWithFields (Vector3d rhs) {
        return new Vector3d(x + rhs.x, y + rhs.y, z + rhs.z);
    }

    public Vector3d AddWithProperties (Vector3d rhs) {
        return new Vector3d(X + rhs.X, Y + rhs.Y, Z + rhs.Z);
    }

    public override string ToString () {
        return String.Format("<{0:00000000.00}, {1:00000000.00}, {2:00000000.00}>", x, y, z);
    }
}