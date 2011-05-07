using System;

public static class Program {
    public static int Y = 0;

    public static int X () {
        return Y * 2;
    }

    public static void Main (string[] args) {
        var a = X();
        Y = 1;
        var b = a;
        var c = X();
        Y = 2;
        var d = c;
        Y = 3;
        var e = X();

        Console.WriteLine("{0} {1} {2} {3} {4}", a, b, c, d, e);
    }
}