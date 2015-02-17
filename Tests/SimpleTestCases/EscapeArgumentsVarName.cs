using System;

public static class Program {
    public static void Main (string[] args)
    {
        Test1("Str1");
        Test2();
    }

    public static void Test1(string arguments)
    {
        Console.WriteLine(arguments);
    }

    public static void Test2()
    {
        var arguments = "Some String";
        arguments += "second";
        Console.WriteLine(arguments);
    }
}