using System;

public class GenericClass<T> {
    public void Method (T value) {
        Console.WriteLine("GenericClass<{0}>.Method({1})", typeof(T), value);
    }
}

public static class Program {
    public static void Main (string[] args) {
        (new GenericClass<int>()).Method(1);
        (new GenericClass<string>()).Method("a");
    }
}