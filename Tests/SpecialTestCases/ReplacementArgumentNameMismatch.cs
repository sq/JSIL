using System;
using JSIL.Meta;
using JSIL.Proxy;

public static class ProxiedClass {
    public static int Fn (int x) {
        return x * 2;
    }
}

[JSProxy(
    typeof(ProxiedClass),
    JSProxyMemberPolicy.ReplaceDeclared
)]
public abstract class ProxiedClassProxy {
    [JSReplacement("($value * 2)")]
    public static AnyType Fn (AnyType value) {
        throw new InvalidOperationException();
    }
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("{0} {1}", ProxiedClass.Fn(1), ProxiedClass.Fn(2));
    }
}