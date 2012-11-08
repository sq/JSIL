using System;

public class Program
{
    public static void Main()
    {
        var x = 100000000000L;
        var y = 10000053450L;
        var z = -10000053450L;

        Print(0L);
        Print(1000000000000L);
        Print(x);
        Print(y);
        Print(z);
        Print(y.GetType().ToString());
    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}