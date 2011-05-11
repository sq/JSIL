using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var array = new[] { 1, 2, 4, 8, 16 };

        foreach (var i in array)
            Console.WriteLine(i);

        var list = new List<int>(array);

        foreach (var j in list)
            Console.WriteLine(j);
    }
}