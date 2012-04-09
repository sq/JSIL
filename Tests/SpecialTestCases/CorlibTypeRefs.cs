using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Method (string text) {
        Console.WriteLine("Method({0})", text);
    }

    public static void Method (MyType mt) {
        Console.WriteLine("Method({0})", mt);
    }

    public static void Main (string[] args) {
        Method("hello");

        Method(new MyType("world"));
    }
}

public class MyType {
    public readonly string Text;

    public MyType (string text) {
        Text = text;
    }

    public override string ToString () {
        return String.Format("MyType({0})", Text);
    }
}