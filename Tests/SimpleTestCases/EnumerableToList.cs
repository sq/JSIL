using System;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        IEnumerable<string> strs = new string[] { "foo", "bar", "baz", "qux" };
        List<string> list = strs.ToList();

        Console.WriteLine(list.Count);
        Console.WriteLine(list[0]);
    }
}