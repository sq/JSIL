using System;
using System.Collections.Generic;

public static class Program {
    public static readonly List<int> List = new List<int> { 1, 2, 3 };

    public static void Main (string[] args) {
        var local = new List<int> { 4, 5, 6 };

        foreach (var i in List)
            Console.WriteLine(i);

        foreach (var i in local)
            Console.WriteLine(i);
    }
}