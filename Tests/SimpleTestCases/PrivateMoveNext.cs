using System;
using System.Collections.Generic;

public static class Program {
    public static IEnumerable<string> GetSomeStrings () {
        yield return "aaaa";
        yield return "bbb";
    }

    public static void Main (string[] args) {
        IEnumerable<string> en = GetSomeStrings();
        var x = new List<String>(en);

        foreach (var item in x)
            Console.WriteLine(item);
    }
}