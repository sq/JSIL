using System;

public static class Program {
    public static void Assign (out int x, int newValue) {
        x = newValue;
    }

    public static void Main (string[] args) {
        int a = 0, b;

        Console.WriteLine("a = {0}", a);
        Assign(out a, 1);
        Console.WriteLine("a = {0}", a);
        Assign(out b, 2);
        Console.WriteLine("a = {0}, b = {1}", a, b);
    }
}