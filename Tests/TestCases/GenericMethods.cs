using System;

public static class Program {
    public static void Main (string[] args) {
        GenericMethod<int>();
        GenericMethod<string>();
        GenericMethod<float[]>();
    }

    public static void GenericMethod<T> () {
        var t = typeof(T);
        Console.WriteLine("GenericMethod<{0}>()", t);
    }
}