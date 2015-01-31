using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    delegate int TWriteStringIntoBuffer (byte* dest, int capacity);

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr ReturnFunctionPointer ();

    public static void Main () {
        using (var na = new NativePackedArray<byte>(512)) {
            var pFunction = ReturnFunctionPointer();
            var d = Marshal.GetDelegateForFunctionPointer<TWriteStringIntoBuffer>(pFunction);

            int numBytes;
            fixed (byte* pBuffer = na.Array) {
                numBytes = d(pBuffer, na.Length);
            }

            var s = Encoding.ASCII.GetString(na, 0, numBytes);
            Console.WriteLine("'{0}'", s);
        }
    }
}