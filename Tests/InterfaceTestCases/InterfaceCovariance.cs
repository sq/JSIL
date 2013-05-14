using System;
using System.Collections.Generic;
using System.Linq;

public class A {
}

public class B : A {
}

public static class Program {
    public static void Main (string[] args) {
        object listB = new List<B> {
            new B(),
            new B()
        };

        Console.WriteLine(listB is IEnumerable<object> ? "true" : "false");
        Console.WriteLine(listB is IEnumerable<A> ? "true" : "false");
        Console.WriteLine(listB is IEnumerable<B> ? "true" : "false");

        object item1 = ((List<B>)listB)[0];

        Console.WriteLine(item1 is object ? "true" : "false");
        Console.WriteLine(item1 is A ? "true" : "false");
        Console.WriteLine(item1 is B ? "true" : "false");
    }
}