using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine((int)Test.MyChar);
    }
}

public static class Test {
    public static readonly char MyChar;

    // Suppress generation of JS for the static constructor so that it isn't run by the javascript interpreter to assign the right value.
    [JSIgnore]
    static Test () {
    }
}