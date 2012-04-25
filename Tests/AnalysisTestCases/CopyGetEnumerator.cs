using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var l = new List<int> {
            1, 2, 3 
        };

        using (var e = l.GetEnumerator())
        while (e.MoveNext())
            Console.WriteLine(e.Current);
    }
}