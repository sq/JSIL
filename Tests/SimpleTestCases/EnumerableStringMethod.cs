using System;
using System.Collections.Generic;

public static class Program {

    public static IEnumerable<string> GetSomeStrings () {
        yield return "aaaa";
        yield return "bbb";
    }

    public static void Main () {
        IEnumerable<string> en = GetSomeStrings();
        var x = new List<String>(en);
        Console.WriteLine(x.Count);
    }
}