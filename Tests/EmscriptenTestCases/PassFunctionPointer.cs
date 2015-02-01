using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr ReturnAdd ();

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CallBinaryOperator (IntPtr fp, int a, int b);

    public static void Main () {
        var fp = ReturnAdd();
        Console.WriteLine(CallBinaryOperator(fp, 5, 3));
    }
}