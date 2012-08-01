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

        Console.WriteLine(x + y);
        Console.WriteLine(x + z);
        Console.WriteLine(z + y);
        Console.WriteLine(z + w);
        Console.WriteLine(z + n);
        Console.WriteLine(y + n);
    }
}