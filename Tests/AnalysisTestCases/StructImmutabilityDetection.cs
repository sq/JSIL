using System;

public static class Program {
    public static CustomType CT;
    public static ImmutableCustomType ICT;

    public static void Main (string[] args) {
        var ct = new CustomType(1);
        var ict = new ImmutableCustomType(2);

        CT = ct;
        ICT = ict;

        Console.WriteLine("{0} {1} {2} {3}", ct, CT, ict, ICT);

        CT = CT.Add(1);
        ICT = ICT.Add(3);

        Console.WriteLine("{0} {1} {2} {3}", ct, CT, ict, ICT);

        ct = CT;
        ict = ICT;

        Console.WriteLine("{0} {1} {2} {3}", ct, CT, ict, ICT);
    }
}

public struct CustomType {
    public int Value;

    public CustomType (int value) {
        Value = value;
    }

    public CustomType Add (int amount) {
        return new CustomType(Value + amount);
    }

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}

public struct ImmutableCustomType {
    public readonly int Value;

    public ImmutableCustomType (int value) {
        Value = value;
    }

    public ImmutableCustomType Add (int amount) {
        return new ImmutableCustomType(Value + amount);
    }

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}