using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new Test();
        instance.Foo();
        object o = instance;
        Console.WriteLine(o is TestInterface);
    }
}

[JSIgnore]
public interface TestInterface {
    void Foo ();
}

public class Test : TestInterface {
    public void Foo () {
        Console.WriteLine("Test.Foo");
    }
}