using System;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(((ICollection<string>)new[] { "one", "two" }).Contains("one").ToString().ToLower());
    }
}