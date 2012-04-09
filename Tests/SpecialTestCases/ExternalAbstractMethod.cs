using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        new Derived().Method();
    }
}

public abstract class Abstract {
    [JSExternal]
    public abstract void Method ();
}

public class Derived : Abstract {
    public override void Method () {
        Console.WriteLine("Derived.Method()");
    }
}