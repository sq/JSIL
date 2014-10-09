using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(typeof(Delegate).IsAssignableFrom(typeof(Action)) ? "true" : "false");
        Console.WriteLine(typeof(MulticastDelegate).IsAssignableFrom(typeof(Action)) ? "true" : "false");
        Console.WriteLine(typeof(Delegate).IsAssignableFrom(typeof(MulticastDelegate)) ? "true" : "false");
    }
}
