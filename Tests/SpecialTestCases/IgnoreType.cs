using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        var instance = new Test();
        Console.WriteLine(instance);
    }
}

[JSIgnore]
public class Test {
}