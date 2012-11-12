using System;

public static class Program {
    public static ImmutableCustomType ICT1, ICT2, ICT3;

    public static bool @false () {
        return false;
    }
    
    public static bool @true () {
        return true;
    }

    public static void Main (string[] args) {
        var ict = new ImmutableCustomType(2);
        ICT1 = ict;

        Console.WriteLine("{0} {1} {2} {3}", ICT1, ICT2, ICT3, ict);

        if (@true()) {
            ict = new ImmutableCustomType(3);
            ICT2 = ict;
        }

        Console.WriteLine("{0} {1} {2} {3}", ICT1, ICT2, ICT3, ict);

        if (@false()) {
            ict = new ImmutableCustomType(4);
            ICT3 = ict;
        }

        Console.WriteLine("{0} {1} {2} {3}", ICT1, ICT2, ICT3, ict);

        ict = new ImmutableCustomType(5);
        Console.WriteLine("{0} {1} {2} {3}", ICT1, ICT2, ICT3, ict);
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