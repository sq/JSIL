using System;

public static class Program {
    public static unsafe void Main (string[] args) {
        const int count = 8;

        var ints = stackalloc int[count];

        for (var i = 0; i < count; i++)
            ints[i] = i;

        for (var i = 0; i < count; i++)
            Console.WriteLine(ints[i]);
    }
}