using System;
using System.Runtime.InteropServices;

public static class Program {
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct Packed1 {
        public int B;
        public short A;

        public override string ToString () {
            return String.Format("A={0}, B={1}", A, B);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct Packed1R {
        public short A;
        public int B;

        public override string ToString () {
            return String.Format("A={0}, B={1}", A, B);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct Packed8 {
        public int B;
        public short A;

        public override string ToString () {
            return String.Format("A={0}, B={1}", A, B);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct Packed8R {
        public short A;
        public int B;

        public override string ToString () {
            return String.Format("A={0}, B={1}", A, B);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=16)]
    public struct Packed32 {
        public int B;
        public short A;

        public override string ToString () {
            return String.Format("A={0}, B={1}", A, B);
        }
    }

    public static unsafe void Main (string[] args) {
        var buffer = new byte[32];

        fixed (byte *pBuffer = buffer) {
            Array.Clear(buffer, 0, 32);
            *((Packed1*)pBuffer) = new Packed1 { A = 1, B = 32 };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Packed1*)pBuffer));

            Array.Clear(buffer, 0, 32);
            *((Packed1R*)pBuffer) = new Packed1R { A = 2, B = 33 };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Packed1R*)pBuffer));

            Array.Clear(buffer, 0, 32);
            *((Packed8*)pBuffer) = new Packed8 { A = 3, B = 34 };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Packed8*)pBuffer));

            Array.Clear(buffer, 0, 32);
            *((Packed8R*)pBuffer) = new Packed8R { A = 4, B = 35 };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Packed8R*)pBuffer));

            Array.Clear(buffer, 0, 32);
            *((Packed32*)pBuffer) = new Packed32 { A = 5, B = 36 };
            Util.PrintBytes(buffer);
            Console.WriteLine(*((Packed32*)pBuffer));
        }
    }
}