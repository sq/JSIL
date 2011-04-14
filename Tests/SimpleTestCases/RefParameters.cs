using System;

public static class Program {
    public static void Increment (ref int x) {
        x += 1;
    }

    public static void Main (string[] args) {
        int a = 0;

        Console.WriteLine("a = {0}", a);
        Increment(ref a);
        Console.WriteLine("a = {0}", a);
    }
}