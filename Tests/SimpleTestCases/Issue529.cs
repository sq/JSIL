using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var list = new List<string> { "hello", "world" };
        var method = list.GetType().GetMethod("get_Item");

        Console.WriteLine(method.Invoke(list, new object[] { 0 }));
    }
}
