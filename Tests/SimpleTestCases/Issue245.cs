using System;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main (string[] args) {
        var items = new IEnumerable<string>[0];
        var simple = SelectManyIterator(items, item => item).ToList();

        foreach (var kvp in simple) 
            Console.WriteLine(kvp);
    }

    private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
        foreach (TSource source1 in source)
            foreach (TResult result in selector(source1))
                yield return result;
    }
}