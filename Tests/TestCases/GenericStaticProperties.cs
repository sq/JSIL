using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(GenericStaticClass<int>.Default);
    }
}

public static class GenericStaticClass<T> {
    public static string Default {
        get { return typeof(T).FullName; }
    }
}