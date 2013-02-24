using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        var ints = new int[] { 0, 1, 2, 3 };

        fixed (int* pInts = &ints[1]) {
            Console.WriteLine(pInts[0]);
            Console.WriteLine(pInts[2]);
        }
    }
}