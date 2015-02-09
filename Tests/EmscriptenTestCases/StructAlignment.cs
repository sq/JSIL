using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    public enum TestEnum : uint {
        ERROR = 0x00000010,
        WARNING = 0x00000020,
        INFORMATION = 0x00000040
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AlignmentStruct {
        public ushort us;
        public TestEnum choice;
    }

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern AlignmentStruct TestAlignment(AlignmentStruct s);

    public static void Main () {       
        var s = new AlignmentStruct { us = 1, choice = TestEnum.ERROR };
        var s2 = TestAlignment(s);
        Console.WriteLine("Result: {0} {1}", s2.us, s2.choice);
    }
}