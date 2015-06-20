using System;
using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Explicit)]
    public struct Padded {
        [FieldOffset(0)]
        public byte A;

        [FieldOffset(2)]
        public uint B;

        [FieldOffset(9)]
        public float C;

        public override string ToString () {
            return String.Format(
                "A={0:X2}, B={1:X8}, C={2:00.000}",
                A, B, C
            );
        }
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[32];

        fixed (byte *pBuffer = buffer) {
            Array.Clear(buffer, 0, 32);
            *((Padded*)pBuffer) = new Padded { A = 0x1B, B = 0xFAEBDCCD, C = 3.3f };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Padded*)pBuffer));
        }
    }
}