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
        (new MyClass<int>()).Method(1);
        (new MyClass<string>()).Method("a");
    }
}