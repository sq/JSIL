using System;
using System.Collections.Generic;
using System.Linq;

public static class Program { 
    public static void Main (string[] args) {
        var items = new Dictionary<int, int>();
        items.Add(1, 1);
        items.Add(2, 2);
        items.Add(3, 3);
        items.Add(4, 4);
        var list = items.ToList();
        var str = string.Empty;
        foreach (var kvp in list) {
            str += kvp.Value;
        }
        Console.WriteLine(str);
    }
}