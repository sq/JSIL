using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Callback(int i);

    [StructLayout(LayoutKind.Sequential)]
    public struct CallbackStruct {
        public ushort us;
        public Callback callback;
    }

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void CallFunctionInStruct (CallbackStruct s, int i);

    public static void Main () {
        Callback c = PrintSuccess;
        var cHandle = GCHandle.Alloc(c);

        var s = new CallbackStruct { us = 4, callback = c };
        CallFunctionInStruct(s, 747);
    }

    public static void PrintSuccess (int i) {
        Console.WriteLine("Success: {0}", i);
    }
}