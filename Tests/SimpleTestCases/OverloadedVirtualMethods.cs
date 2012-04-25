using System;

abstract class A {
    public void M (int i) {
        Console.WriteLine("A.M(int i)");
    }

    public virtual void M (string s) {
        Console.WriteLine("A.M(string s)");
    }

    public abstract void M (float f);
}

class B : A {
    public override void M (string s) {
        Console.WriteLine("B.M(string s)");
    }

    public override void M (float f) {
        Console.WriteLine("B.M(float f)");
    }
}

class C : B { }

class D : A {
    public override void M (string s) {
        Console.WriteLine("D.M(string s)");
    }

    public override void M (float f) {
        Console.WriteLine("D.M(float f)");
    }
}

class E : A {
    public override void M (float f) {
        Console.WriteLine("E.M(float f)");
    }
}

public class Program {
    public static void Main (string[] args) {
        A i = new B();
        i.M(string.Empty);
        i.M(0);
        i.M(0f);

        A j = new C();
        j.M(string.Empty);
        j.M(0);
        j.M(0f);

        A k = new D();
        k.M(string.Empty);
        k.M(0);
        k.M(0f);

        A l = new E();
        l.M(string.Empty);
        l.M(0);
        l.M(0f);
    }
}