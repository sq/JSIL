using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var emptyInts = new int[0];
        var ints = new[] { 1, 2, 3, 4 };

        Console.WriteLine(emptyInts.FirstOrDefault());
        Console.WriteLine(ints.FirstOrDefault());

        Console.WriteLine(ints.First());

        Console.WriteLine(emptyInts.FirstOrDefault((i) => i > 4));
        Console.WriteLine(ints.FirstOrDefault((i) => i > 4));

        Console.WriteLine(ints.First((i) => i > 2));
    }
}