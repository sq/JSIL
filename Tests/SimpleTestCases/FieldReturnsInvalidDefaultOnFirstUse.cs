using System;

static class MyClass {
    public static readonly string[] strings;

    static MyClass () {
        strings = new string[3];
        strings[0] = "a";
        strings[1] = "b";
        strings[2] = "c";
    }
}

static class Program {
    public static void Main () {
        Console.WriteLine(MyClass.strings[0] ?? "null");
        Console.WriteLine(MyClass.strings[0] ?? "null");
    }
}