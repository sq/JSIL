using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static IEnumerable<int> OneToNine {
        get {
            for (int i = 0; i < 10; i++)
                yield return i;
        }
    }

    public static void Main (string[] args) {
        foreach (var i in OneToNine)
            Console.WriteLine("{0}", i);
    }
}