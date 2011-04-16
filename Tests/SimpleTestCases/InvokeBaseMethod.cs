using System;

public class CustomTypeBase {
    public virtual void Method () {
        Console.WriteLine("CustomTypeBase.Method");
    }

    public virtual void Method2 (int x) {
        Console.WriteLine("CustomTypeBase.Method2({0})", x);
    }
}

public class CustomType : CustomTypeBase {
    override public void Method () {
        Console.WriteLine("CustomType.Method");
    }

    override public void Method2 (int x) {
        Console.WriteLine("CustomType.Method2({0})", x);
    }

    public void BaseMethod () {
        base.Method();
    }

    public void BaseMethod2 (int x) {
        base.Method2(x);
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        instance.BaseMethod();
        instance.Method();
        instance.BaseMethod2(3);
        instance.Method2(4);
    }
}