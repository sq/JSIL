using System;

struct Vector4 {
    public float X, Y, Z, W;

    public Vector4 (float x, float y, float z, float w) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static void AddViaConstructor (ref Vector4 lhs, ref Vector4 rhs, out Vector4 result) {
        result = new Vector4(
            lhs.X + rhs.X, 
            lhs.Y + rhs.Y,
            lhs.Z + rhs.Z,
            lhs.W + rhs.W
        );
    }

    public static void AddViaFieldMutation (ref Vector4 lhs, ref Vector4 rhs, out Vector4 result) {
        result.X = lhs.X + rhs.X;
        result.Y = lhs.Y + rhs.Y;
        result.Z = lhs.Z + rhs.Z;
        result.W = lhs.W + rhs.W;
    }

    public override string ToString () {
        return String.Format("{{{0}, {1}, {2}, {3}}}", X, Y, Z, W);
    }
}

public static class Program {
    public static void Main () {
        Vector4 one = new Vector4(1, 1, 1, 1);

        Vector4 two_a, two_b;

        Vector4.AddViaConstructor(ref one, ref one, out two_a);
        Vector4.AddViaFieldMutation(ref one, ref one, out two_b);

        Console.WriteLine("{0} {1} {2}", one, two_a, two_b);
    }
}