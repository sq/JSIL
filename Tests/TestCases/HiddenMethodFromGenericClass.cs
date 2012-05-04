using System;

public class GenericClass<T> {
    public void Method (T value) {
        Console.WriteLine("GenericClass<{0}>.Method({1})", typeof(T), value);
    }
}

public class MyClass : GenericClass<string> {
    public new void Method (string value) {
        Console.WriteLine("MyClass.Method<{0}>({1})", typeof(string), value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var b = (new MyClass());
        b.Method("b");
        ((GenericClass<string>)b).Method("b");
    }
}