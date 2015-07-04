//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    const int BufferSize = 8192;
    const int IterationCount = 64;

    public static Vector3d[] Vectors = new Vector3d[BufferSize];

    public static unsafe void Main () {
        for (int i = 0; i < BufferSize; i++)
            Vectors[i] = new Vector3d(i * 0.5, (double)i, i * 1.5);

        Console.WriteLine("Add: {0:00000.00}ms", Time(TestAdd));
        Console.WriteLine("Add Generic: {0:00000.00}ms", Time(TestAddGeneric));
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

    public static Vector3d TestAddGeneric () {
        Vector3d sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum = Vector3d.Add_Generic<double, double>(sum, Vectors[i]);
        }

        return sum;
    }

    public static Vector3d TestAdd () {
        Vector3d sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum = Vector3d.Add(sum, Vectors[i]);
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

    public static Vector3d Add (Vector3d a, Vector3d b) {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3d Add_Generic<T, U> (Vector3d a, Vector3d b) {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public override string ToString () {
        return String.Format("<{0:00000000.00}, {1:00000000.00}, {2:00000000.00}>", x, y, z);
    }
}