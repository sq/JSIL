using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(typeof(int).IsInterface ? 1 : 0);
        Console.WriteLine(typeof(IEnumerable<int>).IsInterface ? 1 : 0);
    }
}