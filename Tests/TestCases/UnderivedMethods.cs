using System;

public class Foo {
    public void Func1 () {
        Console.WriteLine("Foo.Func1");
    }

    public void Func2 () {
        Console.WriteLine("Foo.Func2");
        this.Func1();
    }
}

public static class Program {
    public static void Main (string[] args) {
        var test = new Foo();
        test.Func1();
        test.Func2();
    }
}