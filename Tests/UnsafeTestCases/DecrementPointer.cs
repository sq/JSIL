using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        var ints = new int[8];

        fixed (int* pInts = ints) {
            var ptr = pInts + 7;

            for (var i = 0; i < ints.Length; i++) {
                *ptr = i;
                --ptr;
            }
        }

        foreach (var i in ints)
            Console.WriteLine(i);
    }
}