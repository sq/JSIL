using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Test.MyArray.Length);
    }
}

public static class Test {
    public static readonly int[] MyArray = null;
    public static readonly int MyArraySize = 32;

    // Suppress generation of JS for the static constructor so that it isn't run by the javascript interpreter to assign the right value.
    [JSIgnore]
    static Test () {
        MyArray = new int[MyArraySize];
    }
}