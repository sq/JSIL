using System;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        var array = new int[] { 1, 2, 3, 4, 5 };
        var enumerable = (from i in array select i * 2);
        var toArrayed = enumerable.ToArray();

        Console.WriteLine(toArrayed is int[] ? "True" : "False");
        foreach (var i in toArrayed)
            Console.WriteLine("{0}", i);
    }
}