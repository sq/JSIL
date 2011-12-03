using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine((new MyClass()).ToString());
    }
}

[JSExternal]
public class MyClass {
    public string ToString () {
        return "MyClass";
    }
}