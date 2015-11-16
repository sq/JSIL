using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var s = "a";
        OverloadedMethod(ref s);
        OverloadedMethod(new[] { "a", "b" });
        OverloadedMethod(new G<string>());
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod<T> (ref T t) {
        Console.WriteLine("OverloadedMethod<{0}>(ref <T>)", typeof(T));
    }

    [JSRuntimeDispatch]
    public static void OverloadedMethod<T> (T[] t) {
        Console.WriteLine("OverloadedMethod<{0}>(<T[]>)", typeof(T));
    }


    [JSRuntimeDispatch]
    public static void OverloadedMethod<T>(G<T> t)
    {
        Console.WriteLine("OverloadedMethod<{0}>(<G<T>>)", typeof(T));
    }
}

public class G<T>
{
}