using System;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        var result = ClosureDelegate(
            10,
            item => ClosureDelegate(100, item2 => item));

        Console.WriteLine(result);
    }

    public static int ClosureDelegate (int input, Func<int, int> func) {
        return func(input);
    }
}