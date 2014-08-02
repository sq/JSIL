using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
    }

    public static void ShouldBeExternal () {
        Console.WriteLine("This shouldn't be translated!");
    }
}

[JSNeverStub]
public static class T {
    public static void ShouldNotBeExternal () {
        Console.WriteLine("This should be translated!");
    }
}