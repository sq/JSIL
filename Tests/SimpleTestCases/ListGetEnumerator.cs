using System;
using System.Collections.Generic;

public static class Program {
    public static void Main () {
        var list1 = new List<string> {
            "a", "b", "c"
        };
        var list2 = new List<string>(list1);
        list2.Add("d");

        foreach (var s in list2)
            Console.WriteLine(s);
    }
}