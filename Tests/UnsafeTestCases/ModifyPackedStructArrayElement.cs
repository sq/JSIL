using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        PackedArray[0] = new IntFloatPair {
            Int = 1,
            Float = 2
        };
        PackedArray[1] = PackedArray[0];

        // FIXME: This should fail right now. Need a solution for taking references to elements of a packed struct array.
        PackedArray[0].Int += 2;

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }
}