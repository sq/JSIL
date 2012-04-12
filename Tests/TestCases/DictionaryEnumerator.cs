using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var d = new Dictionary<string, int> {
            {"a", 1},
            {"b", 2},
            {"z", 3},
            {"c", 4}
        };

        using (var e = d.GetEnumerator())
        while (e.MoveNext())
            Console.WriteLine(e.Current);
    }
}