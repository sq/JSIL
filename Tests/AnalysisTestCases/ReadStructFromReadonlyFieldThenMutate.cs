using System;

public static class Program {
    public static readonly MutableStruct ReadonlyField = new MutableStruct(1);

    public static void Main (string[] args) {
        var local1 = ReadonlyField;
        var local2 = ReadonlyField;

        Console.WriteLine("{0}, {1}", local1, local2);

        local2.Value += 2;

        Console.WriteLine("{0}, {1}", local1, local2);
    }
}

public struct MutableStruct {
    public int Value;

    public MutableStruct (int value) {
        Value = value;
    }

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}