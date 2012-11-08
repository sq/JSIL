using System;

public static class Program {
    public static void Main (string[] args) {
        var ict = new ImmutableCustomType(2);
        var ictCopy = ict;

        Console.WriteLine("{0} {1}", ict, ictCopy);

        ict.Naughty(1);

        Console.WriteLine("{0} {1}", ict, ictCopy);
    }
}

public struct ImmutableCustomType {
    public readonly int Value;

    public ImmutableCustomType (int value) {
        Value = value;
    }

    public void Naughty (int newValue) {
        this = new ImmutableCustomType(newValue);
    }

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}