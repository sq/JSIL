using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int TBinaryOperator (int a, int b);

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CallBinaryOperator (IntPtr fp, int a, int b);

    public static void Main () {
        var d = new TBinaryOperator(Multiply);
        var fp = Marshal.GetFunctionPointerForDelegate(d);
        Console.WriteLine(CallBinaryOperator(fp, 5, 3));
    }

    public static int Multiply (int a, int b) {
        return a * b;
    }
}