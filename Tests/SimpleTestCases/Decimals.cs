using System;

public static class Program {
    public static decimal Double (decimal value) {
        return value * 2;
    }

    public static void Main (string[] args) {
        var a = 1;
        decimal b = (decimal)(a / 2.0);
        decimal c = b * 4;
        var d = Double(b);

        Console.WriteLine("{0:f2}, {1:f2}, {2:f2}, {3:f2}", a, b, c, d);
    }
}