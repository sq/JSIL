using System;

public static class Program {
    public static int GetI () {
        return 1;
    }

    public static void Main (string[] args) {
        float3 vColor;

        switch (GetI()) {
            case 0:
                vColor = new float3(0.0f, 1.0f, 0.0f);
                break;

            case 1:
                vColor = new float3(1.0f, 0.0f, 0.0f);
                break;

            case 2:
                vColor = new float3(0.8f, 0.8f, 0.8f);
                break;

            default:
                vColor = new float3(0.0f, 0.0f, 0.0f);
                break;
        }

        Console.WriteLine(vColor);
    }
}

public struct float3 {
    public float X, Y, Z;

    public float3 (float x, float y, float z) {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString () {
        return String.Format("({0}, {1}, {2})", X, Y, Z);
    }
}