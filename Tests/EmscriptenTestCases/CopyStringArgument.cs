using System;
using System.Text;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    public static extern int CopyStringArgument (
        StringBuilder dest, int capacity,
        string source
    );

    public static void Main () {
        var sb = new StringBuilder("butt");
        sb.Capacity = 256;

        int length = CopyStringArgument(sb, sb.Capacity, "Hello, world!");

        Console.WriteLine("{1} '{0}'", sb.ToString(), length);
    }
}