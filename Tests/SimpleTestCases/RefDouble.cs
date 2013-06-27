using System;

public static class Program {
    public static void Main (string[] args) {
        double a = 1;
        double b = 2;

        Console.WriteLine("{0} {1}", a, b);

        AddOnePointFive(ref a);
        AddOnePointFive(ref b);

        Console.WriteLine("{0} {1}", a, b);
    }

    public static void AddOnePointFive (ref double value) {
        value += 1.5;
    }
}