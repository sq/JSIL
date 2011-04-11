using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new Test();
        instance.Foo();
    }
}

public class Test {
    [JSIgnore]
    public void Foo () {
        Console.WriteLine("Foo");
    }
}