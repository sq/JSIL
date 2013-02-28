using System;

public struct IntFloatPair {
    public int Int;
    public float Float;

    public override string ToString () {
        return String.Format("Int={0:0000}, Float={1:000.000}", Int, Float);
    }
}

public struct TwoIntFloatPairs {
    public IntFloatPair A, B;

    public override string ToString () {
        return String.Format("A=<{0}> B=<{1}>", A, B);
    }
}