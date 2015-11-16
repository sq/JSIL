using System;

public static class Program {
    public static void Main (string[] args)
    {
        IsIconvertible("str1");
        AsIconvertible("42");
        WriteIconvertible("blah");
    }

    public static void IsIconvertible(object input)
    {
        Console.WriteLine(input is IConvertible ? "true" : "false");
    }

    public static void AsIconvertible(object input)
    {
        Console.WriteLine((input as IConvertible).ToInt32(null));
    }

    public static void WriteIconvertible(IConvertible input)
    {
        Console.WriteLine(input.ToString(null));
    }
}