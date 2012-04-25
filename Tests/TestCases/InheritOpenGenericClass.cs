using System;

public class GenericClass<T> {
    public virtual void Method (T value) {
        Console.WriteLine("GenericClass<{0}>.Method({1})", typeof(T), value);
    }
}

public class MyClass<T> : GenericClass<T> {
    public override void Method (T value) {
        Console.WriteLine("MyClass.Method<{0}>({1})", typeof(T), value);
        base.Method(value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = (new MyClass<int>());
        var b = (new MyClass<string>());
        a.Method(1);
        b.Method("a");
        a.Method(1);
        b.Method("a");
    }
}