using System;
using System.Text;

using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ReadInt (IntPtr buffer);
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void WriteInt (int value, IntPtr buffer);

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Alloc (int size);
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Free (IntPtr ptr);

    public static void Main () {
        var buf = Alloc(64);

        WriteInt(10, buf);

        int i = ReadInt(buf);

        Free(buf);

        Console.WriteLine(i);
    }
}