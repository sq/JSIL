using System;

public class Foo {
    public void Func1 () {
        Console.Write("F1 ");
    }

    public virtual void Func2 () {
        Console.Write("F2 ");
        this.Func1();
    }
}

public sealed class Bar : Foo {
    new public void Func1 () {
        Console.Write("B1 ");
    }

    public override void Func2 () {
        Console.Write("B2 ");
        base.Func2();
    }

    public void Func3 () {
        Console.Write("B3 ");
        this.Func2();
    }
}

public static class Program {
    public static void Main (string[] args) {
        var test = new Foo();
        test.Func1();
        test.Func2();

        var test2 = new Bar();
        ((Foo)test2).Func1();
        test2.Func1();
        test2.Func2();
        test2.Func3();
    }
}