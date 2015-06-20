using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TestComplexUnion {
        [FieldOffset(0)]
        public int I;
        [FieldOffset(0)]
        public TestStruct S;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern void WriteComplexUnionInt(int i, out TestComplexUnion result);

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void WriteComplexUnionStruct(int i, float f, out TestComplexUnion result);

    public static void Main () {
        TestComplexUnion result;
        WriteComplexUnionInt(6, out result);

        TestComplexUnion result2;
        WriteComplexUnionStruct(9, 3.7f, out result2);

        Console.WriteLine("i={0} i2={1} f2={2:F4}", result.I, result2.S.I, result2.S.F);
    }
}