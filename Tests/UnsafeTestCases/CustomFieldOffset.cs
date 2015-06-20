using System;
using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Explicit)]
    public struct Test {
        [FieldOffset(0)]
        public byte A;

        [FieldOffset(1)]
        public byte B;

        [FieldOffset(0)]
        public uint C;

        [FieldOffset(6)]
        public float D;

        public override string ToString () {
            return String.Format(
                "A={0:X2}, B={1:X2}, C={2:X8}, D={3:00.000}",
                A, B, C, D
            );
        }
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[32];

        fixed (byte *pBuffer = buffer) {
            Array.Clear(buffer, 0, 32);
            *((Test*)pBuffer) = new Test { C = 0xFAEBDCCD, D = 3.3f };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Test*)pBuffer));
        }
    }
}