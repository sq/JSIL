using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var ints = new[] { 1, 2, 3, 4 };

        Console.WriteLine(ints.ElementAt(0));
        Console.WriteLine(ints.ElementAt(1));
        Console.WriteLine(ints.ElementAt(3));

        Console.WriteLine(ints.ElementAtOrDefault(0));
        Console.WriteLine(ints.ElementAtOrDefault(1));
        Console.WriteLine(ints.ElementAtOrDefault(3));
        Console.WriteLine(ints.ElementAtOrDefault(-1));
        Console.WriteLine(ints.ElementAtOrDefault(5));
    }
}