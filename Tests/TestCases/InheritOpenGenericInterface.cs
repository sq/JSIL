using System;

public interface GenericInterface<T> {
    void Method (T value);
}

public class MyClass<T> : GenericInterface<T> {
    public void Method (T value) {
        Console.WriteLine("MyClass.Method<{0}>({1})", typeof(T), value);
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