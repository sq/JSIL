using System;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern int Add (int a, int b);

    public static void Main () {
        Console.WriteLine(Add(2, 3));
    }
}