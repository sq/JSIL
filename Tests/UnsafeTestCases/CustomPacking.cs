using System;
using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct Packed1 {
        public int B;
        public short A;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct Packed1R {
        public short A;
        public int B;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct Packed8 {
        public int B;
        public short A;
    }

    [StructLayout(LayoutKind.Sequential, Pack=16)]
    public struct Packed32 {
        public int B;
        public short A;
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[32];

        fixed (byte *pBuffer = buffer) {
            *((Packed1*)pBuffer) = new Packed1 { A = 1, B = 32 };
            Util.PrintBytes(buffer);

            *((Packed1R*)pBuffer) = new Packed1R { A = 1, B = 32 };
            Util.PrintBytes(buffer);

            *((Packed8*)pBuffer) = new Packed8 { A = 1, B = 32 };
            Util.PrintBytes(buffer);

            *((Packed32*)pBuffer) = new Packed32 { A = 1, B = 32 };
            Util.PrintBytes(buffer);
        }
    }
}