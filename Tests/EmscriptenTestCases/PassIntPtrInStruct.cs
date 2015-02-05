using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public IntPtr I;
        public float F;
    }

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern TestStruct ReturnStructArgument (TestStruct arg);

    public static void Main () {
        var ip = new IntPtr(32);
        var a = new TestStruct { I = ip, F = 2.5f };
        var b = ReturnStructArgument(a);

        Console.WriteLine("ip.ToInt32() = {0}", b.I.ToInt32());
    }
}