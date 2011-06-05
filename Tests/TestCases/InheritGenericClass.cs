using System;

public class GenericClass<T> {
    public virtual void Method (T value) {
        Console.WriteLine("GenericClass<{0}>.Method({1})", typeof(T), value);
    }
}

public class MyClass : GenericClass<int> {
    public override void Method (int value) {
        Console.WriteLine("MyClass.Method({0})", value);
        base.Method(value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        (new MyClass()).Method(1);
    }
}