//@assertFailureString Invalid attempt to pass a normal array as parameter
//@assertThrows JavaScriptEvaluatorException

using System;
using JSIL.Meta;

public struct IntFloatPair {
    public int Int;
    public float Float;

    public override string ToString () {
        return String.Format("Int={0:0000}, Float={1:000.000}", Int, Float);
    }
}

public static class Program {
    public static IntFloatPair[] PackedArray = new IntFloatPair[2];

    public static unsafe void Main (string[] args) {
        FillPackedArray(PackedArray);

        foreach (var item in PackedArray)
            Console.WriteLine(item);
    }

    [JSPackedArrayArguments("array")]
    public static void FillPackedArray (IntFloatPair[] array) {
        Console.WriteLine(array.Length);
    }
}