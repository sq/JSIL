﻿using System;
using System.Collections.Generic;

class Program {
    public static void Main () {
        var hs = new HashSet<string>() {
            "a",
            "b",
            "c"
        };

        foreach (var s in hs)
            Console.WriteLine(s);

        Console.WriteLine(hs.Count);

        var test = hs.GetEnumerator();
    }
}