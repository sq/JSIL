using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var i = new Impl();
        var ii = (IInterface)i;
        ii.Method();
        ii.Method2();
    }
}

public class Impl : IInterface {
    public void Method () {
        Console.WriteLine("Impl.Method");
    }

    void IInterface.Method2 () {
        Console.WriteLine("Impl.Method2");
    }
}

[JSChangeName("IRenamedInterface")]
public interface IInterface {
    void Method ();
    void Method2 ();
}