using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        foreach (var item in PackedArray)
            Console.WriteLine(item);

        PackedArray[0] = new IntFloatPair {
            Int = 1,
            Float = 2
        };
        PackedArray[1] = new IntFloatPair {
            Int = 3,
            Float = 4
        };

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }
}