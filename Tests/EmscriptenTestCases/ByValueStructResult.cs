using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern TestStruct ReturnStruct (int i, float f);

    public static void Main () {
        var a = ReturnStruct(9, 12.3f);

        Console.WriteLine("i={0} f={1:F4}", a.I, a.F);
    }
}