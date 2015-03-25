using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ReadRefStringLength(ref string s);

    private static string s = "foo";

    public static void Main () {
        Console.WriteLine(ReadRefStringLength(ref s));
    }
}