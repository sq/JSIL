using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var l = new List<int> {
            1, 2, 3 
        };

        foreach (var v in l)
            Console.WriteLine(v);

        foreach (var v in l)
            Console.WriteLine(v);

        foreach (var v in l)
            Console.WriteLine(v);
    }
}