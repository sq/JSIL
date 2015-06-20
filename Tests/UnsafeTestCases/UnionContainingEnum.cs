using System;
using System.Runtime.InteropServices;

public static class Program {
    public enum ByteEnum : byte {
        A, B, C, D
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Union {
        [FieldOffset(0)]
        public uint UI;

        [FieldOffset(1)]
        public ByteEnum E;

        public override string ToString () {
            return String.Format(
                "UI={0:X8}, E={1}",
                UI, E
            );
        }
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[32];

        fixed (byte *pBuffer = buffer) {
            var u = new Union();
            u.UI = 0xFAEBDCCD;
            Console.WriteLine(u);

            Array.Clear(buffer, 0, 32);
            *((Union*)pBuffer) = u;
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Union*)pBuffer));

            Console.WriteLine(Marshal.SizeOf(typeof(Union)));
        }
    }
}