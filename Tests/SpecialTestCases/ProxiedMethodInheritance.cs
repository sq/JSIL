using System;
using JSIL.Meta;
using JSIL.Proxy;

public class BaseClass {
    public virtual void Method1 () {
        Console.WriteLine("BaseClass.Method1");
    }

    public virtual void Method2 () {
        Console.WriteLine("BaseClass.Method2");
    }
}

public class DerivedClass : BaseClass {
    public override void Method1 () {
        Console.WriteLine("DerivedClass.Method1");
    }

    public override void Method2 () {
        Console.WriteLine("DerivedClass.Method2");
    }
}

public class DerivedClass2 : DerivedClass {
    public override void Method1 () {
        Console.WriteLine("DerivedClass2.Method1");
    }

    public override void Method2 () {
        Console.WriteLine("DerivedClass2.Method2");
    }
}

[JSProxy(
    typeof(BaseClass),
    JSProxyMemberPolicy.ReplaceDeclared,
    inheritable: true
)]
public abstract class BaseClassProxy {
    public void Method1 () {
        Console.WriteLine("BaseClassProxy.Method1");
    }
}

[JSProxy(
    typeof(DerivedClass),
    JSProxyMemberPolicy.ReplaceDeclared,
    inheritable: false
)]
public abstract class DerivedClassProxy {
    public void Method2 () {
        Console.WriteLine("DerivedClassProxy.Method2");
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new DerivedClass();
        var b = new DerivedClass2();

        a.Method1();
        a.Method2();
        b.Method1();
        b.Method2();
    }
}