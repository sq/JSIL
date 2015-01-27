using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern void WriteStruct (int i, float f, out TestStruct result);

    public static void Main () {
        TestStruct result;
        WriteStruct(6, 3.7f, out result);

        Console.WriteLine("i={0} f={1:F4}", result.I, result.F);
    }
}