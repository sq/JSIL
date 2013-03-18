using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        PackedArray[0] = new IntFloatPair {
            Int = 1,
            Float = 2.22f
        };
        PackedArray[1] = PackedArray[0];

        PackedArray[0].Int += 2;
        PackedArray[0].Float += 1.1f;

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }
}