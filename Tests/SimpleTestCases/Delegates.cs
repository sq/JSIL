using System;

public static class Program {
    public static void Main (string[] args) {
        Action<int> a =
            (i) => Console.WriteLine("a({0})", i);
        Action<int> b =
            (i) => Console.WriteLine("b({0})", i);

        a(1);
        b(2);
    }
}