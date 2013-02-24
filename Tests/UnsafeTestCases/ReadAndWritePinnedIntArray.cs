using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        var ints = new int[] { 0, 1, 2, 3 };

        fixed (int* pInts = ints)
        for (var i = 0; i < ints.Length; i++)
            pInts[i] += 1;

        foreach (var i in ints)
            Console.WriteLine(i);
    }
}