using System;
using System.Threading;

public static class Program {
    public static Int32 Value;

    public static void Main (string[] args) {
        Value = 1;
        Console.WriteLine("{0}", Value);

        Console.WriteLine("{0}", Interlocked.CompareExchange(ref Value, 2, 0));
        Console.WriteLine("{0}", Value);

        Console.WriteLine("{0}", Interlocked.CompareExchange(ref Value, 2, 1));
        Console.WriteLine("{0}", Value);
    }
}