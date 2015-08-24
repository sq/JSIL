using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(typeof(int).BaseType.Name);
        Console.WriteLine(typeof(double).BaseType.Name);
        Console.WriteLine(typeof(bool).BaseType.Name);
        Console.WriteLine(typeof(char).BaseType.Name);
    }
}