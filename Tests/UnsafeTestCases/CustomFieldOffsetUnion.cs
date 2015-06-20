using System;
using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Explicit)]
    public struct Union {
        [FieldOffset(0)]
        public uint UI;

        [FieldOffset(0)]
        public byte B1;

        [FieldOffset(1)]
        public byte B2;

        [FieldOffset(3)]
        public float F;

        public override string ToString () {
            return String.Format(
                "UI={0:X8}, F={1:00.000}, B1={2:X2}, B2={3:X2}",
                UI, F, B1, B2
            );
        }
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[32];

        fixed (byte *pBuffer = buffer) {
            var u = new Union();
            u.B1 = 3;
            u.B2 = 7;
            u.UI = 0xFAEBDCCD;
            u.F = 3.3f;
            Console.WriteLine(u);

            Array.Clear(buffer, 0, 32);
            *((Union*)pBuffer) = u;
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Union*)pBuffer));

            Console.WriteLine(Marshal.SizeOf(typeof(Union)));
        }
    }
}