using System;
using System.Collections.Generic;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        var fn = Builtins.CreateNamedFunction<Func<int, int>>(
            "Test", new[] { "argInt" }, 
            @"return argInt + closureInt;",
            new {
                closureInt = 2
            }
        );

        if (fn == null)
            Console.WriteLine("fn == null");
        else
            Console.WriteLine(fn(1));
    }
}