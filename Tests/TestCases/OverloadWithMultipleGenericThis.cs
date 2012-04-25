using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var t = new CustomType();
        var s = "a";
        t.OverloadedMethod(ref s);
        t.OverloadedMethod(new[] { "a", "b" });
    }
}

public class CustomType {
    [JSRuntimeDispatch]
    public void OverloadedMethod<T> (ref T t) {
        Console.WriteLine("OverloadedMethod<{0}>(ref <T>) this={1}", typeof(T), this);
    }

    [JSRuntimeDispatch]
    public void OverloadedMethod<T> (T[] t) {
        Console.WriteLine("OverloadedMethod<{0}>(<T[]>) this={1}", typeof(T), this);
    }
}