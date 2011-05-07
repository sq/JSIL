using System;

public static class Program {
    public static int Y;

    public static int X () {
        return Y * 2;
    }

    public static void Main (string[] args) {
        Y = 1;
        var a = X();
        Y = 2;
        var b = X();
        Y = 3;
        var c = X();

        Console.WriteLine("{0} {1} {2} {3}", a, b, c, X());
    }
}