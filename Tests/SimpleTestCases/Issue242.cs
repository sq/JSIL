using System;
using System.Collections.Generic;

public class A<TKey, TValue> {
    public KeyValuePair<TKey, TValue> Kvp;
}

public static class Program { 
    public static void Main (string[] args) {
        var a = new A<int, string>();
        var b = new KeyValuePair<string, string>("1", "qqq");
        var c = b;
        b = new KeyValuePair<string, string>("2", "aaa");
        Console.WriteLine(b);
        Console.WriteLine(c);
    }
}