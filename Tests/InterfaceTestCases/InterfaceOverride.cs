using System;

public interface I {
    void Foo ();
}

public class A : I {
    public virtual void Foo () {
        Console.WriteLine("A");
    }
}

public class B : A, I {
    public override void Foo () {
        Console.WriteLine("B");
    }
}

public static class Program {
    public static void Main (string[] args) {
        I i;

        var a = new A();
        i = a;
        a.Foo(); // expected: "A"
        i.Foo(); // expected: "A"

        var b = new B();
        i = b;
        b.Foo(); // expected: "B"
        i.Foo(); // expected: "B"
    }
}