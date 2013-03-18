using System;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var bytes = new byte[] { 
            0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x60, 0x40 
        };

        fixed (byte* pBytes = bytes) {
            var pStruct = (IntFloatPair*)pBytes;

            Console.WriteLine(*pStruct);

            pStruct += 2;

            Console.WriteLine(*pStruct);
        }
    }
}