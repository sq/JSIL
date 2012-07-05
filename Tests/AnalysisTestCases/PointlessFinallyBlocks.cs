using System;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        var l1 = new List<int> { 1, 2, 3 };
        var l2 = new List<int> { 4, 5, 6 };

        foreach (var i1 in l1)
            foreach (var i2 in l2)
                Console.WriteLine("{0} {1}", i1, i2);
    }
}