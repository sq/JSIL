using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern TestStruct RetrurnStructArgument (TestStruct arg);

    public static void Main () {
        var a = new TestStruct { I = 1, F = 2.5f };

        try {
            var b = RetrurnStructArgument(a);
        } catch (Exception exc) {
            Console.WriteLine(exc.GetType().Name);
            Console.WriteLine(exc.Message);
        }
    }
}