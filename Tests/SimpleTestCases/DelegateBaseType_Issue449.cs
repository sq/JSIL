using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(typeof(MulticastDelegate).IsAssignableFrom(typeof(Action)) ? "true" : "false");
    }
}
