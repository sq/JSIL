using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ReturnStaticString();

    public static string GetStringFromIntPtr() {
        unsafe {
            return new string((sbyte*)ReturnStaticString());
        }
    }

    public static void Main () {
        Console.WriteLine(GetStringFromIntPtr());
    }
}