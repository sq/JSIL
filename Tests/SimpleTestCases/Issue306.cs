using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var array = new[] { "one", "two" };
        var list = (IList<string>)array;
        Console.WriteLine(list[0]);
        list[1] = "three";
        
        foreach (var item in array)
            Console.WriteLine(item);
    }
}