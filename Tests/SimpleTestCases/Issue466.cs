using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var list = (IList<string>)new List<string>() { "hello" };
        var @delegate = Delegate.CreateDelegate(typeof(Func<int, string>), list, typeof(IList<string>).GetMethod("get_Item"));
    }
}