using System;
public static class Program
{
    public static void Main()
    {
        Console.WriteLine(typeof(object).GetConstructors().Length);
    }
}