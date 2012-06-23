using System;
using System.Collections.Generic;

public static class CommonExtensionMethodsSimple {
    public static void IsNullOrEmpty<T> (this IEnumerable<T> items) {
        Console.WriteLine("IsNullOrEmpty with 1 parameters");
    }

    public static void IsNullOrEmpty<T> (this IEnumerable<T> items, Func<T, bool> expression) {
        Console.WriteLine("IsNullOrEmpty with 2 parameters");
    }

    public static bool IsNullOrEmpty (this string items) // If you comment this one out, the test will pass
    {
        Console.WriteLine("IsNullOrEmpty string");
        return String.IsNullOrEmpty(items);
    }
}

public static class Program {
    public static IEnumerable<string> GetSomeStrings () {
        yield return "aaaa";
        yield return "bbb";
    }

    public static void Main (string[] args) {
        IEnumerable<string> en = GetSomeStrings();
        en.IsNullOrEmpty();
        Console.WriteLine("Done");
    }
}