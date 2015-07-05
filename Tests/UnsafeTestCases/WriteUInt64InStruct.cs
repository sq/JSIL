using System;
using System.Runtime.InteropServices;

public static class Program {
    public struct StructWithUInt64 {
        public uint A;
        public UInt64 B;
        public uint C;
    }

    public static unsafe void Main (string[] args) {
        var bytes = new byte[20];

        fixed (byte* pBytes = bytes) {
            var pStruct = (StructWithUInt64*)pBytes;

            *pStruct = new StructWithUInt64 {
                A = 0xAABBCCDD,
                B = 0x0011223344556677UL,
                C = 0xDDCCBBAA
            };
        }

        Util.PrintBytes(bytes);
    }
}