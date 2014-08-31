using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public static class Program {
    public static void Main (string[] args) {
        var coll = new List<int>() { 5, 25, 50, 125 };
        var arr = coll.ToArray();

        Console.WriteLine("{0} {1}", coll.GetType(), arr.GetType());
        Console.WriteLine("{0} {1}", coll[0].GetType(), arr[0].GetType());
    }
}