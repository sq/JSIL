using System;
using System.Text;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    public static extern void MutateStringArgument (StringBuilder buf, int capacity);

    public static void Main () {
        var sb = new StringBuilder("hello");
        sb.Capacity = 256;

        MutateStringArgument(sb, sb.Capacity);

        Console.WriteLine("'{0}'", sb.ToString());
    }
}