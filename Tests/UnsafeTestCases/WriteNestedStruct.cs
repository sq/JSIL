using System;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var bytes = new byte[16];

        fixed (byte* pBytes = bytes) {
            var pStruct = (TwoIntFloatPairs*)pBytes;
            *pStruct = new TwoIntFloatPairs {
                A = new IntFloatPair {
                    Int = 2,
                    Float = 3.5f
                },
                B = new IntFloatPair {
                    Int = 16,
                    Float = -1.337f
                }
            };
        }

        for (var i = 0; i < bytes.Length; i++)
            Console.Write("{0:X2} ", bytes[i]);

        Console.WriteLine();
    }
}