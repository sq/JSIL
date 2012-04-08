using System;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        var array = new int[] { 1, 2, 3, 4, 5 };
        var enumerable = (from i in array select i * 2);

        foreach (var i in enumerable)
            Console.WriteLine("{0}", i);
    }
}