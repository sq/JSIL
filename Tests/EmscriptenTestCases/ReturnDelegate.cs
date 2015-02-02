using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate TestStruct TReturnStructArgument (TestStruct arg);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int TWriteStringIntoBuffer (byte* dest, int capacity);

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern TWriteStringIntoBuffer ReturnWriteStringIntoBuffer ();

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern TReturnStructArgument ReturnReturnStructArgument ();

    public static void Main () {
        using (var na = new NativePackedArray<byte>(512)) {
            var d = ReturnWriteStringIntoBuffer();

            int numBytes;
            fixed (byte* pBuffer = na.Array) {
                numBytes = d(pBuffer, na.Length);
            }

            var s = Encoding.ASCII.GetString(na, 0, numBytes);
            Console.WriteLine("'{0}'", s);
        }

        {
            var d = ReturnReturnStructArgument();

            var a = new TestStruct { I = 3, F = 5.5f };
            var b = d(a);

            Console.WriteLine("i={0} f={1:F4}", b.I, b.F);
        }
    }
}