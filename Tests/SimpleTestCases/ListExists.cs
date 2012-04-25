using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var l = new List<string> {
            "A", "A", "B", "C", "D"
        };

        Console.WriteLine(l.Exists((s) => s == "A") ? "1" : "0");
        Console.WriteLine(l.Exists((s) => s == "Q") ? "1" : "0");
    }
}