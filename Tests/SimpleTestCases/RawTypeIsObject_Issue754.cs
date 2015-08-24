using System;

public static class Program
{
    public static void Main(string[] args)
    {
        IsType<object>("qwerty");
        IsType<object>(1);
        IsType<object>(true);
        IsType<object>(new object());
    }

    public static void IsType<T>(object obj)
    {
        Console.Write(obj is T ? "true" : "false");
    }
}