using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        var ints1 = new[] { 1, 2, 3, 4 };
        var ints2 = new List<int> { 5, 6, 7 };

        foreach (var prod in ints1.Zip(ints2, (l, r) => l * r ))
            Console.WriteLine(prod);

    }
}
