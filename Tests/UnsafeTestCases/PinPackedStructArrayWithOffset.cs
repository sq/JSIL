using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        PackedArray[1] = new IntFloatPair {
            Int = 1,
            Float = 3.14f
        };

        fixed (IntFloatPair* pStruct = &PackedArray[1])
            Console.WriteLine(*pStruct);
    }
}