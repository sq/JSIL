using System;
using System.Collections.Generic;
using System.Linq;

public class A {
}

public class B : A {
}

public static class Program {
    public static void Main (string[] args) {
        var listA = new List<A> {
            new A(),
            new B()
        };

        var listB = new List<B> {
            new B(),
            new B()
        };

        foreach (var b in listA.OfType<B>())
            Console.WriteLine(b);

        var bAsEnumerableA = (IEnumerable<A>)listB;

        foreach (var a in bAsEnumerableA)
            Console.WriteLine(a);
    }
}