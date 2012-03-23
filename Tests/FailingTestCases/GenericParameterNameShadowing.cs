using System;

public class A<T> {
    public A () {
        Console.WriteLine("A<{0}>", typeof(T));
    }
}

public class B<T> : A<T[]> {
    public B () {
        Console.WriteLine("B<{0}>", typeof(T));
    }
}

public class C<T> : B<T[]> {
    public C () {
        Console.WriteLine("C<{0}>", typeof(T));
    }
}

public static class Program {
    public static void Main (string[] args) {
        var test = new C<int>();
    }
}