using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var hashSet = new HashSet<string>();

        Console.WriteLine(hashSet.Count);

        hashSet.Add("a");
        Console.WriteLine(hashSet.Count);

        hashSet.Add("a");
        Console.WriteLine(hashSet.Count);

        hashSet.Add("b");
        Console.WriteLine(hashSet.Count);

        hashSet.Clear();
        Console.WriteLine(hashSet.Count);
    }
}