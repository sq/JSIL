using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Program {
    public static void Main () {
        foreach (var i in Enumerable.Range(0, 5))
            Console.WriteLine(i);
    }
}