using System;

public class Program
{
    public static void Main()
    {
        var x = long.Parse("100000000000");
        var y = long.Parse("10000053450");
        var n = long.Parse("-10000053450");
        var w = long.Parse("0");
        var z = 10000053450L;

        Print(x + y);
        Print(x + z);
        Print(z + y);
        Print(z + w);
        Print(z + n);
        Print(y + n);
    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}