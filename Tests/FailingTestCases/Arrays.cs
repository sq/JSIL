using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new [] { 1, 2, 3 };
        var b = new int[3];

        for (int i = 0; i < a.Length; i++)
            b[i] = a[i];

        // JavaScript provides no sane way to subclass Array.
        // All existing approaches cause deoptimization in TraceMonkey, which is unacceptable.
        Console.WriteLine(a);
        Console.WriteLine(b);
    }
}