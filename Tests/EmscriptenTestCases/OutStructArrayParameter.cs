using System;

using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int FillStructBuffer(
        [Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)]
			TestStruct[] buffer, int capacity);

    public static void Main() {
        TestStruct[] buffer = new TestStruct[1];
        FillStructBuffer(buffer, 1);
        Console.WriteLine("I={0} F={1:F4}", buffer[0].I, buffer[0].F);
    }
}