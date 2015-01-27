using System;
using System.Text;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ReadInt (IntPtr buffer);
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void WriteInt (int value, IntPtr buffer);

    public static void Main () {
        var buf = Asm.Js.malloc(64);

        WriteInt(10, buf);

        int i = ReadInt(buf);

        Asm.Js.free(buf);

        Console.WriteLine(i);
    }
}