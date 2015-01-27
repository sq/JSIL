using System;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern int ReadInt (ref int value);

    public static void Main () {
        int i = 7;

        Console.WriteLine(ReadInt(ref i));
    }
}