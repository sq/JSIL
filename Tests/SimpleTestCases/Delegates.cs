using System;

public static class Program {
    public static void Main (string[] args) {
        Action<int> a =
            (i) => Console.WriteLine("a({0})", i);
        Action<int> b =
            (i) => Console.WriteLine("b({0})", i);
        Action<int> c = PrintNumber;

        a(1);
        b(2);
        c(3);
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("PrintNumber({0})", x);
    }
}