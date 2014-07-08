using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var target = new List<string> { "hello", "world" };
        var method = typeof(IList<string>).GetMethod("get_Item");

        method.Invoke(target, new object[] { 0 });
    }
}