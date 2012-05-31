using System;
using JSIL;
using JSIL.Meta;
using System.Collections.Generic;

public static class CommonExtensionMethodsSimple {
    public static bool Any<TSource> (this IEnumerable<TSource> source) {
        Console.WriteLine("Any with one argument");
        using (var enumerator = source.GetEnumerator())
            return enumerator.MoveNext();
    }

    public static bool Any<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate) {

        Console.WriteLine("Any with two arguments");

        foreach (TSource element in source)
            if (predicate(element))
                return true;

        return false;
    }

    public static bool IsNullOrEmpty<T> (this IEnumerable<T> items) {
        Console.WriteLine("IsNullOrEmpty with 1 parameters");
        return items == null || !items.Any();
        return true;
    }

    public static bool IsNullOrEmpty<T> (this IEnumerable<T> items, Func<T, bool> expression) {
        Console.WriteLine("IsNullOrEmpty with 2 parameters");
        return items == null || !items.Any(expression);
        return true;
    }

    public static bool IsNullOrEmpty(this string items)
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

    public static void Main () {
        IEnumerable<string> en = GetSomeStrings();
        Console.WriteLine(en.IsNullOrEmpty() ? "true" : "false");
    }
}