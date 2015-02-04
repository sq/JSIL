using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [Flags]
    public enum TestFlags : uint {
        FLAG_ERROR = 0x00000010,
        FLAG_WARNING = 0x00000020,
        FLAG_INFORMATION = 0x00000040
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FlagStruct {
        public TestFlags flags;
    }

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern FlagStruct PassFlagInStruct (FlagStruct s);

    public static void Main () {       
        var s = new FlagStruct { flags = TestFlags.FLAG_INFORMATION };
        var s2 = PassFlagInStruct(s);
        Console.WriteLine("Flags: {0}", s2.flags);
    }
}