using System;
using System.Collections.Generic;

public static class Program {
    public static int x = 10;
    public static int y = 20;

    public static void Main () {
        var x = new string[] { "a", "b" };
        var s = Join(x);
        Console.WriteLine("poco=" + s);
    }

    static string Join (IList<string> strings) {
        var l = "";
        foreach (var x in strings) {
            l += x;
        }
        return l;
    }
}