using System;
using System.Text;

using System.Runtime.InteropServices;

public static class Program {
    public enum TestEnum : int {
        FIRST = 0x1100,
        SECOND = 0x1200
    }

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern TestEnum ReturnInt(int i);

    public static void Main() {
        Console.WriteLine("{0}", ReturnInt(3));
        Console.WriteLine("{0}", ReturnInt(0x1200));
    }
}