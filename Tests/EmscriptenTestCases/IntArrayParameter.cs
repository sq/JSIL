using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ReturnSecondIntFromArray(int[] ints);

    private static int[] ints = new int[] { 1, 2, 3 };

    public static void Main () {
        Console.WriteLine("{0}", ReturnSecondIntFromArray(ints));
    }
}