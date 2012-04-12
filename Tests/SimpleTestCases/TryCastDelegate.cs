using System;

public static class Program {
    public static void Main (string[] args) {
        Action<int> a =
            (i) => Console.WriteLine("a({0})", i);
        object b = a;

        var c = (b as Action<int>);

        a(1);
        c(2);
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("PrintNumber({0})", x);
    }
}