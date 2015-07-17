using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        // This test is sensitive to string hash algorithm. Order of keys may be diffirent.
        var d = new Dictionary<string, int> {
            {"a", 1},
            {"b", 2},
            {"c", 4},
            {"z", 3}
        };

        using (var e = d.GetEnumerator())
        while (e.MoveNext())
            Console.WriteLine(e.Current);
    }
}