using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr ReturnNullPtr ();

    public static void Main () {
        if (ReturnNullPtr() == IntPtr.Zero) {
            Console.WriteLine("null pointers equal");
        } else {
            Console.WriteLine("null pointers are not equal");
        }
    }
}