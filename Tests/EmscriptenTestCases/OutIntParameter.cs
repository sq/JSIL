using System;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll")]
    public static extern void WriteInt (int value, out int result);

    public static void Main () {
        int result;
        WriteInt(5, out result);

        Console.WriteLine(result);
    }
}