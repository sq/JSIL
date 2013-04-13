//@compileroption /unsafe

using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new Test();
        instance.Foo();
        instance.Bar();
        try {
            instance.Baz();
        } catch (Exception ex) {
            Console.WriteLine("Caught: {0}", ex);
        }
    }
}

public class Test {
    public unsafe void Foo () {
        Console.WriteLine("Foo");
    }

    public void Bar () {
        Console.WriteLine("Bar");
    }

    public unsafe void Baz () {
        var buffer = new int[4];
        fixed (int * pBuffer = buffer)
            Console.WriteLine("Baz");
    }
}