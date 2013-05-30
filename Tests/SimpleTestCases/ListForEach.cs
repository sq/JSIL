using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var list = new List<string> { "zero", "one", "two", "three" };
        list.ForEach(Console.WriteLine);
    }
}