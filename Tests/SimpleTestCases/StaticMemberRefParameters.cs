using System;

public static class Program {
    public static int A;

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
        A = 0;
        Console.WriteLine("a = {0}", A);
        Increment(ref A);
        Console.WriteLine("a = {0}, a + 1 = {1}", A, Incremented(A));
        IncrementTwice(ref A);
        Console.WriteLine("a = {0}", A);
    }
}