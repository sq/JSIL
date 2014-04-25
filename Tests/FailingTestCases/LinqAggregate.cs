using System;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(new[] { "one", "two" }.Aggregate((x, y) => x + ", " + y));
    }
}