using System;
using JSIL.Meta;
using JSIL.Proxy;

public class ProxiedClass {
    public int Field;
    public int Property { get; set; }

    public ProxiedClass () {
        Field = 1;
        Property = 2;
    }

    public void PrintValues () {
        Console.WriteLine("Field = {0}, Property = {1}", Field, Property);
    }
}

[JSProxy(
    typeof(ProxiedClass),
    JSProxyMemberPolicy.ReplaceDeclared
)]
public abstract class ProxiedClassProxy {
    public int Field;
    public int Property { get; set; }

    [JSReplaceConstructor]
    public ProxiedClassProxy () {
        Field = 2;
        Property = 4;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var pc = new ProxiedClass();
        pc.PrintValues();
    }
}