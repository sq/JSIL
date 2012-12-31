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
        l.RemoveRange(1, 1);
        PrintItems(l);
        l.RemoveRange(2, 2);
        PrintItems(l);

        try {
            l.RemoveRange(-5, 1);
        } catch (Exception exc) {
            Console.WriteLine(exc.GetType().Name);
        }

        try {
            l.RemoveRange(17, 5);
        } catch (Exception exc) {
            Console.WriteLine(exc.GetType().Name);
        }
    }
}