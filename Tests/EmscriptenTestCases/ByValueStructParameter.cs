using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern TestStruct ReturnStructArgument (TestStruct arg);

    public static void Main () {
        var a = new TestStruct { I = 1, F = 2.5f };
        var b = ReturnStructArgument(a);

        Console.WriteLine("i={0} f={1:F4}", b.I, b.F);
    }
}