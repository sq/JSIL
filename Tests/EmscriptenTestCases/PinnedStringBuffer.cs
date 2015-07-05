using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern int WriteStringIntoBuffer (
        byte * dest, int capacity
    );

    public static void Main () {
        using (var na = new NativePackedArray<byte>("common.dll", 512)) {
            int numBytes;
            fixed (byte* pBuffer = na.Array)
                numBytes = WriteStringIntoBuffer(pBuffer, na.Length);

            var s = Encoding.ASCII.GetString(na, 0, numBytes);
            Console.WriteLine("'{0}'", s);
        }
    }
}