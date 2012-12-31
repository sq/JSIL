using System;
using System.Collections.Generic;

public static class Program {
    public static void PrintItems<T> (IEnumerable<T> items) {
        Console.WriteLine();
        foreach (var item in items)
            Console.Write("{0} ", item);
        Console.WriteLine();
    }

    public static void Main (string[] args) {
        var l = new List<int>();
        for (var i = 0; i < 20; i++)
            l.Add(i);

        PrintItems(l);

        Console.WriteLine(l.BinarySearch(-1));
        Console.WriteLine(l.BinarySearch(0));
        Console.WriteLine(l.BinarySearch(7));
        Console.WriteLine(l.BinarySearch(15));
        Console.WriteLine(l.BinarySearch(20));
    }
}