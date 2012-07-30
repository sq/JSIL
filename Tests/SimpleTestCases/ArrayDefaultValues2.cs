using System;

public static class Program {
    public static void Main (string[] args) {
        var bools = new bool[4];
        var chars = new char[4];
        var strings = new string[4];

        foreach (var b in bools) {
            // Some JS implementations return true for (0 === false) :|
            Console.WriteLine(b.ToString().ToLower());
        }

        foreach (var c in chars)
            Console.WriteLine(c == '\0' ? "\\0" : "error");

        foreach (var s in strings)
            Console.WriteLine(s == null ? "null" : "error");
    }
}