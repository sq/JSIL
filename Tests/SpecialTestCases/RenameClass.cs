using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine((new MyClass()).ToString());
    }
}

[JSChangeName("RenamedClass")]
public class MyClass {
    public string ToString () {
        return String.Format("{0} {1}", "MyClass", this.GetType());
    }
}