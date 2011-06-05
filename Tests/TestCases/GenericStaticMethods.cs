using System;

public static class Program {
    public static void Main (string[] args) {
        GenericStaticClass<int>.Method();
        GenericStaticClass<string>.Method();
    }
}

public static class GenericStaticClass<T> {
    public static void Method () {
        Console.WriteLine("GenericStaticClass<{0}>.Method()", typeof(T));
    }
}