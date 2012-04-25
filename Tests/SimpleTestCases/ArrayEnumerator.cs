using System;
using System.Collections;

public static class Program {
    public static void Main (string[] args) {
        var a = new int[] { 0, 1, 2, 3, 4 };

        var e = a.GetEnumerator();
        while (e.MoveNext())
            Console.WriteLine(e.Current);
    }
}