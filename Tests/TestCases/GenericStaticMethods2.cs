using System;

public static class Program {
    public static void Main (string[] args) {
        GenericStaticClass<int>.Method(1, 2);
        GenericStaticClass<string>.Method(3, "4");
    }
}

public static class GenericStaticClass<T> {
    public static void Method (int a, T b) {
        Console.WriteLine("GenericStaticClass<{0}>.Method({1}, {2})", typeof(T), a, b);
    }
}