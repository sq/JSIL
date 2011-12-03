using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(MyClass.GetString());
    }
}

[JSExternal]
public static class MyClass {
    public static string GetString () {
        return "MyClass";
    }
}