using System;

public static class Program {
    public static int a;

    public static void Increment (ref int x) {
        x += 1;
    }

    public static void IncrementTwice (ref int x) {
        Increment(ref x);
        Increment(ref x);
    }

    public static int Incremented (int x) {
        Increment(ref x);
        return x;
    }

    public static void Main (string[] args) {
        a = 0;
        Console.WriteLine("a = {0}", a);
        Increment(ref a);
        Console.WriteLine("a = {0}, a + 1 = {1}", a, Incremented(a));
        IncrementTwice(ref a);
        Console.WriteLine("a = {0}", a);
    }
}