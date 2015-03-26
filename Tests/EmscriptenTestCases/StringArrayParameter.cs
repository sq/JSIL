using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CopySecondStringFromArray(StringBuilder dest, int capacity, string[] source);

    private static string[] strings = new string[] { "foo", "rah" };

    public static void Main () {
        var sb = new StringBuilder("butt");
        sb.Capacity = 256;

        int length = CopySecondStringFromArray(sb, sb.Capacity, strings);

        Console.WriteLine("{1} '{0}'", sb.ToString(), length); Console.WriteLine(strings[1]);
    }
}