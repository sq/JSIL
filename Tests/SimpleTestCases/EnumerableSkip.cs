using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var ints = new[] { 1, 2, 3, 4 };

        foreach (var i in ints.Skip(2))
            Console.WriteLine(i);

    }
}
