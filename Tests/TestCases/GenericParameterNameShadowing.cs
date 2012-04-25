using System;

public class X<T> {
}

public class A<T> {
    public virtual void M () {
        Console.WriteLine("A<{0}>", typeof(T));
    }
}

public class B<T> : A<X<T>> {
    public override void M () {
        base.M();
        Console.WriteLine("B<{0}>", typeof(T));
    }
}

public class C<T> : B<X<T>> {
    public override void M () {
        base.M();
        Console.WriteLine("C<{0}>", typeof(T));
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new C<int>();
        var b = new C<string>();
        var c = new B<float>();

        a.M();
        b.M();
        c.M();
    }
}