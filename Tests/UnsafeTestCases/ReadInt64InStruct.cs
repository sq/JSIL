using System;
using System.Runtime.InteropServices;

public static class Program {
    public struct StructWithInt64 {
        public int A;
        public Int64 B;
        public int C;

        public override string ToString () {
            return String.Format("<A={0}, B={1}, C={2}>", A, B, C);
        }
    }

    public static unsafe void Main (string[] args) {
        var bytes = new byte[20] { 
            0x02, 0x00, 0x00, 0x00,

            // Padding fill
            0xFF, 0xFF, 0xFF, 0xFF, 

            0x02, 0x08, 0x00, 0x02,
            0x12, 0x00, 0x60, 0x40,

            0x00, 0x00, 0x60, 0x40 
        };

        fixed (byte* pBytes = bytes) {
            var pStruct = (StructWithInt64*)pBytes;

            Console.WriteLine(*pStruct);
        }
    }
}