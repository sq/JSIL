using System;

class A {
    private string foo;

    public void Do<T> () {
        Console.WriteLine(foo.Length);
    }

    public A () {
        foo = "asdf";
    }
}

class B : A {
    public B () {
        Do<int>();
    }
}

public class Program {
    public static void Main (string[] args) {
        var b = new B();
    }
}

