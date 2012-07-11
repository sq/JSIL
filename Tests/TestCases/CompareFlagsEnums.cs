using System;

public static class Program {
    [Flags]
    public enum SimpleEnum {
        A = 1,
        B = 2,
        C = 4
    }

    public static SimpleEnum Or (SimpleEnum lhs, SimpleEnum rhs) {
        return lhs | rhs;
    }

    public static void Main (string[] args) {
        var a = SimpleEnum.A;
        var b = SimpleEnum.B;

        var c = Or(a, b);
        var d = Or(b, a);

        Console.WriteLine("{0} {1} {2} {3}", a, b, c, d);
        Console.WriteLine("{0}", c == a ? "true" : "false");
        Console.WriteLine("{0}", c == d ? "true" : "false");
    }
}