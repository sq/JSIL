using System;
using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct Packed1 {
        public short A;
        public int B;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PackedDefault {
        public short A;
        public int B;
    }

    [StructLayout(LayoutKind.Sequential, Pack=16)]
    public struct Packed16 {
        public short A;
        public int B;
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[64];

        fixed (byte *pBuffer = buffer) {
            *((Packed1*)pBuffer) = new Packed1 { A = 1, B = 32 };
            Util.PrintBytes(buffer);

            *((PackedDefault*)pBuffer) = new PackedDefault { A = 1, B = 32 };
            Util.PrintBytes(buffer);

            *((Packed16*)pBuffer) = new Packed16 { A = 1, B = 32 };
            Util.PrintBytes(buffer);
        }
    }
}