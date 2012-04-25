using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var t = new CustomType();
        var s = "a";
        t.OverloadedMethod<string>(1);
        t.OverloadedMethod<string>(s);
    }
}

public class CustomType {
    [JSRuntimeDispatch]
    public void OverloadedMethod<T> (int i) {
        Console.WriteLine("OverloadedMethod<{0}>(int {1}) this={2}", typeof(T), i, this);
        this.OverloadedMethod<T>(i.ToString());
    }

    [JSRuntimeDispatch]
    public void OverloadedMethod<T> (string s) {
        Console.WriteLine("OverloadedMethod<{0}>(string {1}) this={2}", typeof(T), s, this);
    }
}