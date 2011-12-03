using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(MyClass.GetString());
    }
}

[JSChangeName("RenamedClass")]
public static class MyClass {
    public static string GetString () {
        return String.Format("{0} {1}", "MyClass", typeof(MyClass));
    }
}