using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [DllImport("what.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern TestStruct ReturnStructArgument (TestStruct arg);

    public static void Main () {
        var a = new TestStruct { I = 1, F = 2.5f };

        try {
            var b = ReturnStructArgument(a);
            Console.WriteLine("Lookup ignored DLL name");
        } catch (Exception exc) {
            Console.WriteLine(exc.GetType().Name);

            // HACK: JS exception won't have hresult
            var msg = exc.Message;
            var parenPosition = msg.IndexOf("(");
            if (parenPosition >= 0)
                msg = msg.Substring(0, parenPosition - 1).Trim();

            Console.WriteLine(msg);
        }
    }
}