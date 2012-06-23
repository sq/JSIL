using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Test.MyArray.Length);
    }
}

public class Test {
    public static readonly Test[] MyArray;

    // Suppress generation of JS for the static constructor so that it isn't run by the javascript interpreter to assign the right value.
    [JSIgnore]
    static Test () {
        MyArray = new Test[2];
    }
}