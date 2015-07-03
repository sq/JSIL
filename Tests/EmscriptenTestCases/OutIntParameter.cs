using System;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern void WriteInt (int value, out int result);

    public static void Main () {
        int result;
        WriteInt(2147483646, out result);

        Console.WriteLine(result);
    }
}