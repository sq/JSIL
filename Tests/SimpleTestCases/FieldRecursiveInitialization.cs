using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("a" + Test.Empty);
        Console.WriteLine(Test.Empty + "a");
        Console.WriteLine("a" + Test.Empty2);
        Console.WriteLine(Test.Empty2 + "a");
    }
}

public static class Test {
    public static readonly string Empty = null;
    public static readonly string Empty2 = null;

    // Suppress generation of JS for the static constructor so that it isn't run by the javascript interpreter to assign the right value.
    [JSIgnore]
    static Test () {
        Empty = "";
        Empty2 = Empty;
    }
}