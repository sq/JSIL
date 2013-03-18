using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[8];

    public static unsafe void Main (string[] args) {
        for (int i = 0, l = PackedArray.Length; i < l; i++) {
            PackedArray[i].Int = i;
            PackedArray[i].Float = (i * 1.1f);
        }

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }
}