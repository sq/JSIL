using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {

        foreach (var x in Enumerable.Repeat("x", 5))
            Console.WriteLine(x);

    }
}
