using System;

public static class Program { 
    public static void Main (string[] args) {
        Console.WriteLine(new Base());
        Console.WriteLine(new Base(5));
        Console.WriteLine(new Base(5, 6));

        Console.WriteLine(new Derived());
        Console.WriteLine(new Derived(7));
        Console.WriteLine(new Derived(5, 6, 7));
    }
}

public class Derived : Base {
    public int C;

    public Derived ()
        : this(3) {
    }

    public Derived (int c) 
        : base() {
        C = c;
    }

    public Derived (int a, int b, int c)
        : base(a, b) {
        C = c;
    }

    public override string ToString () {
        return String.Format("<Derived A={0} B={1} C={2}>", this.A, this.B, this.C);
    }
}

public class Base {
    public int A, B;

    public Base () : this(1) {
    }

    public Base (int a) : this (a, 2) {
    }

    public Base (int a, int b) {
        A = a;
        B = b;
    }

    public override string ToString () {
        return String.Format("<Base A={0} B={1}>", this.A, this.B);
    }
}