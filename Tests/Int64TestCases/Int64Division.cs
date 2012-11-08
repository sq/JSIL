using System;

public class Program
{
    public static void Main()
    {
        var x = 1000000L;
        var y = 1450L;
        var z = -15033L;

        Print(x / y);
        Print(x / z);
        Print(y / z);
    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}