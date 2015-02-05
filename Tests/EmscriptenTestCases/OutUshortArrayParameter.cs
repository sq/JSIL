using System;
using System.Runtime.InteropServices;

public static class Program {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FillUshortArray(
        [Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)]
				ushort[] arr
    );

    public static void Main () {
        var arr = new ushort[256];
        FillUshortArray(arr);
        Console.WriteLine("Test: {0}", arr[0]);
    }
}