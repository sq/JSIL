using System;
using JSIL.Meta;
using JSIL.Proxy;

public static class ProxiedClass {
    public static void ProxiedMethod () {
        Console.WriteLine("ProxiedClass.ProxiedMethod");
    }
}

[JSProxy(
    typeof(ProxiedClass),
    JSProxyMemberPolicy.ReplaceDeclared
)]
public abstract class ProxiedClassProxy {
    public static void ProxiedMethod () {
        Console.WriteLine("ProxiedClassProxy.ProxiedMethod");
    }
}

public static class Program {
    public static void Main (string[] args) {
        ProxiedClass.ProxiedMethod();
    }
}