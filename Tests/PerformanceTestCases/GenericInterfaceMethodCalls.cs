//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    const int BufferSize = 8192;
    const int IterationCount = 320;

    public static Vector3d[] Vectors = new Vector3d[BufferSize];

    public static unsafe void Main () {
        for (int i = 0; i < BufferSize; i++)
            Vectors[i] = new Vector3d(i * 0.5, (double)i, i * 1.5);

        Console.WriteLine("Add Through Generic Interface: {0:00000.00}ms", Time(TestAddThroughGenericInterface));
        Console.WriteLine("Add Through Open Generic Interface: {0:00000.00}ms", Time(TestAddThroughOpenGenericInterface<double>));
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

    public static Vector3d TestAddThroughGenericInterface () {
        IVector3<double> sum = new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum = sum.Add(Vectors[i]);
        }

        return (Vector3d)sum;
    }

    public static Vector3d TestAddThroughOpenGenericInterface<TElement> () {
        IVector3<TElement> sum = (IVector3<TElement>)new Vector3d();

        for (int i = 0; i < BufferSize; i++) {
            sum = sum.Add((IVector3<TElement>)Vectors[i]);
        }

        return (Vector3d)sum;
    }
}

public interface IVector3<T> {
    IVector3<T> Add (IVector3<T> rhs);
}

public class Vector3d : IVector3<double> {
    public readonly double x, y, z;

    public Vector3d (double x = 0, double y = 0, double z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public IVector3<double> Add (IVector3<double> _rhs) {
        var rhs = (Vector3d)_rhs;
        return new Vector3d(x + rhs.x, y + rhs.y, z + rhs.z);
    }

    public override string ToString () {
        return String.Format("<{0:00000000.00}, {1:00000000.00}, {2:00000000.00}>", x, y, z);
    }
}