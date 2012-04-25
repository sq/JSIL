using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    static IEnumerable<TResult> CreateRepeatIterator<TResult> (TResult element, int count) {
        for (int i = 0; i < count; i++)
            yield return element;
    }

    static IEnumerable<int> CreateRangeIterator (int start, int count) {
        for (int i = 0; i < count; i++)
            yield return start + i;
    }

    public static void Main (string[] args) {
        foreach (var item in CreateRepeatIterator<string>("a", 5))
            Console.WriteLine("{0}", item);
    }
}