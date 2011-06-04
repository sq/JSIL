using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        OverloadedMethod(1);
        OverloadedMethod("a");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod (int i) {
        Console.WriteLine("OverloadedMethod(<int>)");
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod<T> (T t) {
        Console.WriteLine("OverloadedMethod<{0}>(<T>)", typeof(T));
    }
}