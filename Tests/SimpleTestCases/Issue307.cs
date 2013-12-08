using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var array = new[] { "one" };
        var list = (IList<string>)array;
        object oList = list;

        var aA = list as string[];
        var aB = (string[])list;
        var aC = oList as string[];
        var aD = (string[])oList;
        var aE = oList as int[];

        Console.WriteLine(aA.Length);
        Console.WriteLine(aB.Length);
        Console.WriteLine(aC.Length);
        Console.WriteLine(aD.Length);
        Console.WriteLine(aE == null ? "null" : "not null");
        Console.WriteLine(oList is string[] ? "true" : "false");
        Console.WriteLine(oList is int[] ? "true" : "false");
    }
}