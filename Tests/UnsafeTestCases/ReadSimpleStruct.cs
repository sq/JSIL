using System;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var bytes = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x40 };

        fixed (byte* pBytes = bytes) {
            var pStruct = (MyStruct*)pBytes;

            Console.WriteLine(*pStruct);
        }
    }
}

public struct MyStruct {
    public int Int;
    public float Float;

    public override string ToString () {
        return String.Format("Int={0:0000}, Float={1:000.000}", Int, Float);
    }
}