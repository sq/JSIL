using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ReadStringLength(ref string s);

    public static void Main () {
        string s = "foo";
        Console.WriteLine(ReadStringLength(ref s));
    }
}