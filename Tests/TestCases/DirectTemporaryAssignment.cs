using System;

public static class Program {
    public static void Main (string[] args) {
        int a = 1, b = 2;
        int c = a, d = b;
        int e = c, f = d;
        int g = e, h = f;
        int i = g * h;
        int j = a * f;
        Console.WriteLine("g={0} h={1} i={2} j={3}", g, h, i, j);
    }
}