using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        var ints = new int[] { 0, 1, 2, 3 };

        fixed (int* pInts = ints) {
            Console.WriteLine(pInts[0]);

            var pBytes = (byte*)pInts;

            Console.WriteLine("{0:X2} {1:X2} {2:X2} {3:X2}", pBytes[0], pBytes[1], pBytes[2], pBytes[3]);
        }
    }
}