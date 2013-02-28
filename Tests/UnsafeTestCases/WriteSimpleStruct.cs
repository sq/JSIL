using System;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var bytes = new byte[32];

        fixed (byte* pBytes = bytes) {
            var pStruct = (MyStruct*)pBytes;
            *pStruct = new MyStruct {
                Int = 2,
                Float = 3.5f
            };
        }

        for (var i = 0; i < bytes.Length; i++)
            Console.Write("{0:X2} ", bytes[i]);
    }
}

public struct MyStruct {
    public int Int;
    public float Float;
}