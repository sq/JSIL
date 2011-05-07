using System;

public static class Program {
    public static void Main (string[] args) {
        Func<int> a = () => 1;
        Func<int, int> b = (x) => x * 2;

        Console.WriteLine("a()={0}, b(a())={1}", a(), b(a()));
    }
}