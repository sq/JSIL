using System;

public static class Program {
    public static void Main () {
        double dbl = 123.45;
        decimal dec;

        dec = (decimal)dbl;

        Console.WriteLine(dec);
    }
}