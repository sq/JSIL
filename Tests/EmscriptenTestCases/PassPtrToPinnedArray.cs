using System;
using System.Runtime.InteropServices;

public static unsafe class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort FirstElementOfUshortArray(IntPtr ptr);

    public static void Main () {
        var arr = new ushort[5] { 1, 2, 3, 4, 5 };        
        GCHandle dataHandle = GCHandle.Alloc(arr, GCHandleType.Pinned);
        Console.WriteLine("Test: {0}", FirstElementOfUshortArray(dataHandle.AddrOfPinnedObject()));
    }
}