using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        FillPackedArray(PackedArray);

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }

    [JSPackedArrayArguments("array")]
    public static void FillPackedArray (IntFloatPair[] array) {
        for (var i = 0; i < array.Length; i++)
            array[i] = new IntFloatPair {
                Int = i,
                Float = i * 1.5f
            };
    }
}