using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var action = (Action)Test;
        action.Invoke();
    }

    public static void Test()
    {
        Console.WriteLine("Test");
    }
}
