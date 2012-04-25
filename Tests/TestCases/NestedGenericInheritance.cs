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

public class MyClass2 : MyClass<int> {
    public override void Method (int value) {
        Console.WriteLine("MyClass2.Method<{0}>({1})", typeof(int), value);
        base.Method(value);
    }
}

public class MyClass3 : MyClass2 {
    public override void Method (int value) {
        Console.WriteLine("MyClass3.Method<{0}>({1})", typeof(int), value);
        base.Method(value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new MyClass3();
        a.Method(1);
    }
}