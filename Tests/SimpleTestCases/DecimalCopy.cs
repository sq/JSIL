using System;

public static class Program {
    public static void Main () {
        decimal a, b, c;

        a = 1.0m;
        b = 2.0m;
        c = a + b;

        Console.WriteLine("{0:0.0} {1:0.0} {2:0.0}", a, b, c);

        a = 3.0m;
        c = a + b;

        Console.WriteLine("{0:0.0} {1:0.0} {2:0.0}", a, b, c);
    }
}