using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Create<MyClass>());
    }

    public static T Create<T> () where T : new() {
        return new T();
    }
}

public class MyClass {
}