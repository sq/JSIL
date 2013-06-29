using System;
using JSIL.Meta;
using JSIL.Runtime;

public static class Program {
    public static unsafe void Main (string[] args) {
        var temp = MakeArray();

        Console.WriteLine(temp.Length);
    }

    [JSPackedArrayReturnValue]
    public static IntFloatPair[] MakeArray () {
        return PackedArray.New<IntFloatPair>(16);
    }
}