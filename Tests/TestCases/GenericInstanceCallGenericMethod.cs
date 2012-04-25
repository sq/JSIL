using System;

public static class Program {
    public static void Main (string[] args) {
        new GenericClass<int>().Method(1);
        new GenericClass<string>().Method("a");
    }
}

public class GenericClass<T> {
    public void Method (T value) {
        Console.WriteLine("GenericClass<{0}>.Method({1})", typeof(T), value);
        GenericStaticMethod(value);
    }

    public static void GenericStaticMethod<U> (U value) {
        Console.WriteLine("GenericStaticMethod<{0}>({1})", typeof(U), value);
    }
}