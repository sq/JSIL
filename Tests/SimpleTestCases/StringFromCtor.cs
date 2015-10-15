using System;

public static class Program
{
    public static void Main()
    {
        var str = GetString();
        Console.WriteLine(str.GetType().Name);
        Console.WriteLine(str is string ? "true" : "false");
    }

    public static object GetString()
    {
        return new string('a', 10);
    }
}