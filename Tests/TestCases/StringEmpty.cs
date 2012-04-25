using System;
using JSIL.Meta;

public static class Program {
    // In the real mscorlib, Empty is a field initialized to null that is then assigned a value of "" by a static cctor.
    // So, if we translate mscorlib stubbed, String.Empty will be initialized to null unless we process its static cctor.
    public static void Main (string[] args) {
        Console.WriteLine("a" + String.Empty);
        Console.WriteLine(String.Empty + "a");
        Console.WriteLine(String.Empty.Length);

        Console.WriteLine("a" + Test.Empty);
        Console.WriteLine(Test.Empty + "a");
        Console.WriteLine(Test.Empty.Length);
    }
}

public static class Test {
    public static readonly string Empty = null;

    // Suppress generation of JS for the static constructor so that it isn't run by the javascript interpreter to assign the right value.
    [JSIgnore]
    static Test () {
        Empty = "";
    }
}