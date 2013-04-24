using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var a = new float3(0, 1, 1);
        var b = new float3(1, 0, 0);

        Console.WriteLine("{0:000.000}", Test(a, b));
    }

    public static float Test (float3 lookAt, float3 upDirection) {
        float3.OrthoNormalize(ref lookAt, ref upDirection);

        float w = upDirection.y + lookAt.z;
        float w4Recip = 1.0f / (4.0f * w);
        float x = (upDirection.z - lookAt.y) * w4Recip;
        float y = lookAt.x * w4Recip;
        float z = upDirection.x * w4Recip;

        return w + x + y + z;
    }
}

public struct float3 {
    public readonly float x, y, z;

    public float3 (float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static void OrthoNormalize (ref float3 a, ref float3 b) {
        a = a;
        b = b;
    }
}