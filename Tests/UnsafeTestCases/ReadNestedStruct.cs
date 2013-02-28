using System;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var bytes = new byte[] { 
            0x02, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x60, 0x40, 
            0x10, 0x00, 0x00, 0x00, 
            0xD1, 0x22, 0xAB, 0xBF 
        };

        fixed (byte* pBytes = bytes) {
            var pStruct = (TwoIntFloatPairs*)pBytes;

            Console.WriteLine(*pStruct);
        }
    }
}