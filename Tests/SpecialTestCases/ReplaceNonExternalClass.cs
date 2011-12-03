using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(MyClass.GetString());
    }
}

[JSReplacement("UnqualifiedTypeName")]
public static class MyClass {
    public static string StringField = "StringField";
    public static string StringProperty {
        get;
        set;
    }

    public static string GetString () {
        return "MyClass";
    }
}