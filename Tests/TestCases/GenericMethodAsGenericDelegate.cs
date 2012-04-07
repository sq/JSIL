using System;

public class Program {
    public static void InvokeDelegate<T> (Action<T> del, T value) {
        del(value);
    }

    static void GenericMethod<T> (T t) {
        Console.WriteLine("GenericMethod<{0}>({1})", typeof(T), t);
    }

    public static void Main (string[] args) {
        InvokeDelegate<int>(GenericMethod, 1);
        InvokeDelegate<string>(GenericMethod, "a");
    }
}
