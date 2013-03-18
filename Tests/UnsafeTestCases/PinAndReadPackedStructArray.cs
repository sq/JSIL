using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        fixed (IntFloatPair* pStruct = PackedArray)
            Console.WriteLine(*pStruct);
    }
}