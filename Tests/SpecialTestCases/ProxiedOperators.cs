using System;
using JSIL.Meta;
using JSIL.Proxy;

public class ProxiedClass {
    public readonly int Value;

    public ProxiedClass (int value) {
        Value = value;
    }

    public static ProxiedClass operator + (ProxiedClass lhs, ProxiedClass rhs) {
        return new ProxiedClass(lhs.Value + rhs.Value);
    }

    public static ProxiedClass operator * (ProxiedClass lhs, ProxiedClass rhs) {
        return new ProxiedClass(lhs.Value * rhs.Value);
    }

    public override string ToString () {
        return String.Format("P {0}", Value);
    }
}

[JSProxy(
    typeof(ProxiedClass),
    JSProxyMemberPolicy.ReplaceDeclared
)]
public abstract class ProxiedClassProxy {
    [JSReplacement("$lhs.Value + $rhs.Value")]
    public static ProxiedClassProxy operator + (ProxiedClassProxy lhs, ProxiedClassProxy rhs) {
        throw new NotImplementedException();
    }

    [JSReplacement("$lhs.Value * $rhs.Value")]
    public static ProxiedClassProxy operator * (ProxiedClassProxy lhs, ProxiedClassProxy rhs) {
        throw new NotImplementedException();
    }
}

public static class Program {
    public static void Main (string[] args) {
        var one = new ProxiedClass(1);
        var two = new ProxiedClass(2);
        Console.WriteLine("{0} {1}", one + one, two * two);
    }
}