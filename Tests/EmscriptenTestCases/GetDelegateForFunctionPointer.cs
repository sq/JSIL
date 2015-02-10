using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    public delegate float AddFloat(float a, float b);

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr ReturnAddFloat();

    public static void Main () {
        var fp = ReturnAddFloat();

        var addFloat = (AddFloat) Marshal.GetDelegateForFunctionPointer(fp, typeof(AddFloat));

        Console.WriteLine("%f", addFloat(0.5f, 1.9f));
    }
}