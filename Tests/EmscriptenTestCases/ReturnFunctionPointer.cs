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
    delegate TestStruct TReturnStructArgument (TestStruct arg);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int TWriteStringIntoBuffer (byte* dest, int capacity);

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr ReturnWriteStringIntoBuffer ();

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ReturnReturnStructArgument ();

    public static void Main () {
        using (var na = new NativePackedArray<byte>(512)) {
            var pFunction = ReturnWriteStringIntoBuffer();
            var d = Marshal.GetDelegateForFunctionPointer<TWriteStringIntoBuffer>(pFunction);

            int numBytes;
            fixed (byte* pBuffer = na.Array) {
                numBytes = d(pBuffer, na.Length);
            }

            var s = Encoding.ASCII.GetString(na, 0, numBytes);
            Console.WriteLine("'{0}'", s);
        }

        {
            var pFunction = ReturnReturnStructArgument();
            var d = Marshal.GetDelegateForFunctionPointer<TReturnStructArgument>(pFunction);

            var a = new TestStruct { I = 3, F = 5.5f };
            var b = d(a);

            Console.WriteLine("i={0} f={1:F4}", b.I, b.F);
        }
    }
}