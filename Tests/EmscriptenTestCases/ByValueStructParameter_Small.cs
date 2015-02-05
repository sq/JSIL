using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct FlagStruct {
        public uint Flags;
    }

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern FlagStruct PassFlagInStruct (FlagStruct arg);

    public static void Main () {
        var a = new FlagStruct { Flags = 7 };
        var b = PassFlagInStruct(a);

        Console.WriteLine("Flags={0}", b.Flags);
    }
}