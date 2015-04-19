using System;
using JSIL.Meta;

public static class Program {
    [JSPackedArray]
    public static TwoIntFloatPairs[] PackedArray = new TwoIntFloatPairs[2];

    public static unsafe void Main (string[] args) {
        PackedArray[0] = new TwoIntFloatPairs {
            A = new IntFloatPair {
                Int = 1,
                Float = 2.22f
            },
            B = new IntFloatPair {
                Int = 2,
                Float = 3.33f
            }
        };
        PackedArray[1] = PackedArray[0];

        PackedArray[0].A.Int += 2;
        PackedArray[0].B.Float += 1.1f;
        PackedArray[1].B.Int += 3;

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }
}