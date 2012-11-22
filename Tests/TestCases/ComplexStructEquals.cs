using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new Bar { val = 1 };
        var b = new Bar { val = 2 };
        var c = a;
        Console.WriteLine("a==a: {0}, a==b: {1}, a==c: {2}", a.Equals(a), a.Equals(b), a.Equals(c));
        c.b = 3;
        Console.WriteLine("a==a: {0}, a==b: {1}, a==c: {2}", a.Equals(a), a.Equals(b), a.Equals(c));
    }
}

public enum SomeEnum {
    Alpha,
    Beta,
    Gamma
}

public enum SomeShortEnum : short {
    Alpha,
    Beta,
    Gamma
}

public struct Foo {
    public static int s = 100;
    public double dval;
    public char c;
    public int x;
    public int y;
    public SomeEnum v;
    public SomeShortEnum vs;
}

public struct Bar {
    public int val;
    public Foo foo;
    public byte b;
}