using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var a = new List<int> { 1, 2, 3 };
        if (a.Remove(1)) {
            Console.WriteLine("Removed 1");
        }
    }
}