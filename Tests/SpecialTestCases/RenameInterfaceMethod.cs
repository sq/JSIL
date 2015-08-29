using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var i = new Impl();
        i.Method();

        var ii = (IInterface)i;
        ii.Method();
    }
}

public class Impl : IInterface {
    public void Method () {
        Console.WriteLine("Impl.Method");
    }
}

public interface IInterface {
    [JSChangeName("RenamedMethod")]
    void Method ();
}