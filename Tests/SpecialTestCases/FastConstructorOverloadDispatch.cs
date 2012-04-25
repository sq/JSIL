using System;

public static class Program {
    public static void Main (string[] args) {
        var a1 = new A();
        var a2 = new A(1);
        var b1 = new B();
        var b2 = new B(1);
        var b3 = new B("s");
    }
}

public class A {
    public A () {
        Console.WriteLine("A()");
    }

    public A (int i) {
        Console.WriteLine("A({0})", i);
    }
}

public class B {
    public B () {
        Console.WriteLine("B()");
    }

    public B (int i) {
        Console.WriteLine("B(int {0})", i);
    }

    public B (string s) {
        Console.WriteLine("B(string {0})", s);
    }
}