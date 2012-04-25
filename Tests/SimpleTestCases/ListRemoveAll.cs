using System;
using System.Collections.Generic;

public static class Program {
    public static void PrintItems (IEnumerable<string> items) {
        Console.WriteLine();
        foreach (var item in items)
            Console.WriteLine(item);
    }

    public static void Main (string[] args) {
        var l = new List<string> {
            "A", "A", "B", "C", "D"
        };

        PrintItems(l);
        Console.WriteLine(l.RemoveAll((s) => s == "A"));
        PrintItems(l);
        Console.WriteLine(l.RemoveAll((s) => s == "Q"));
        PrintItems(l);
    }
}