using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Explicit)]
    public struct TestUnion {
        [FieldOffset(0)]
        public int I;
        [FieldOffset(0)]
        public float F;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern void WriteUnionInt(int i, out TestUnion result);

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void WriteUnionFloat(float f, out TestUnion result);

    public static void Main () {
        TestUnion result;
        WriteUnionInt(6, out result);

        TestUnion result2;
        WriteUnionFloat(3.7f, out result2);

        Console.WriteLine("i={0} f={1:F4}", result.I, result2.F);
    }
}