using System;

public interface GenericInterface<T> {
    void Method (T value);
}

public class MyClass : GenericInterface<int>, GenericInterface<string> {
    public void Method (int value) {
        Console.WriteLine("MyClass.Method(int)", value);
    }

    public void Method (string value) {
        Console.WriteLine("MyClass.Method(string)", value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = (new MyClass());
        a.Method(1);
        a.Method("a");
    }
}