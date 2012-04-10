using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        OverloadedMethod<int>(1);
        OverloadedMethod<string>(1, "a");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod<T> (int i) {
        Console.WriteLine("OverloadedMethod<{0}>({1})", typeof(T), i);
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod<T> (int i, string s) {
        Console.WriteLine("OverloadedMethod<{0}>({1}, {2})", typeof(T), i, s);
    }
}