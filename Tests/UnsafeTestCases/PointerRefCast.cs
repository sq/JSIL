using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        var bytes = new byte[16];

        fixed (byte* pBytes = bytes) {
            var p1 = pBytes;
            var p2 = ReturnArg(pBytes);

            CastTest(ref p1);

            Console.WriteLine(p1 != p2 ? "true" : "false");
        }
    }

    public static unsafe byte* ReturnArg (byte* arg) {
        return arg;
    }

    public static unsafe void CastTest (ref byte* pointer) {
        pointer = (byte*)0;
    }
}