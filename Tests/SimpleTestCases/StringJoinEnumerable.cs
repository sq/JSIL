using System;
using System.Collections.Generic;

static class Program {
    public static void Main () {
        IEnumerable<A> items = new List<A>() { new A("Foo"), new A("Bar"), new A("Baz"), new A("Qux") };
        Console.WriteLine(string.Join("; ", items));

        IEnumerable<string> strings = new List<string> { "Foo", "Bar", "Baz", "Qux" };
        Console.WriteLine(string.Join("; ", strings));
    }
}

class A {
    public A (string text) {
        Text = text;
    }

    public override string ToString () {
        return Text;
    }

    public string Text { get; private set; }
}
