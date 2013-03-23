//@assertFailureString Return value of method 'ReturnPackedArray' must be a packed array
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
        var pa = ReturnPackedArray();

        foreach (var item in pa)
            Console.WriteLine(item);
    }

    [JSPackedArrayReturnValue]
    public static IntFloatPair[] ReturnPackedArray () {
        return PackedArray;
    }
}