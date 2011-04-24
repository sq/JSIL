using System;

public static class Program {
    public static void Main (string[] args) {
        // Have to use functions, otherwise the compiler statically performs the arithmetic for us
        Func<int> one = () => 1;
        Func<int> two = () => 2;

        var a = one() / two();
        var b = (one() / two()) * 2.0f;
        var c = (one() / 2.0f) * two();

        Console.WriteLine("{0}, {1}, {2}", a, b, c);
    }
}