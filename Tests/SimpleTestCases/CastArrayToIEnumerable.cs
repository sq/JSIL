using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void AcceptIEnumerable (IEnumerable ie) {
        object o = ie;
        Console.WriteLine(o is IEnumerable ? "true" : "false");

        foreach (var item in ie)
            Console.WriteLine(item);
    }

    public static void AcceptIEnumerableT<T> (IEnumerable<T> ie) {
        object o = ie;
        Console.WriteLine(o is IEnumerable ? "true" : "false");
        Console.WriteLine(o is IEnumerable<T> ? "true" : "false");

        foreach (var item in ie)
            Console.WriteLine(item);
    }

    public static void Main (string[] args) {
        var a = new int[] { 0, 1, 2, 3, 4 };
        object o = a;

        AcceptIEnumerable(a);
        AcceptIEnumerableT<int>(a);

        AcceptIEnumerable((IEnumerable)o);
        AcceptIEnumerableT<int>((IEnumerable<int>)o);
    }
}