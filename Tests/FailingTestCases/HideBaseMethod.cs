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

public class Bar : Foo {
    new public void Func1 () {
        Console.WriteLine("Bar.Func1");
    }
}

public static class Program {
    public static void Main (string[] args) {
        Bar test = new Bar();
        test.Func2();
    }
}