using System;

public static class Program {
    public static void Main (string[] args) {
        Action<int> a =
            (i) => Console.WriteLine("a({0})", i);
        Func<Action<int>> b =
            () => a;
        Func<Func<Action<int>>> c =
            () => b;

        a(1);
        b()(2);
        c()()(3);
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("PrintNumber({0})", x);
    }
}