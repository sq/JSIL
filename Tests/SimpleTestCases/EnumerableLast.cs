using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var ints = new[] { 1, 2, 3, 4 };

        Console.WriteLine(ints.Last());

        Console.WriteLine(ints.Last((i) => i < 3));

        // Test the non-IList path
        Console.WriteLine(ints.Skip(1).Last());

        Console.WriteLine(ints.Skip(1).Last((i) => i < 3));
    }
}
