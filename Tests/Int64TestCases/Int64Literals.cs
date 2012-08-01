using System;

public class Program
{
    public static void Main()
    {
        var x = 100000000000L;
        var y = 10000053450L;
        var z = -10000053450L;

        Console.WriteLine(0L);
        Console.WriteLine(1000000000000L);
        Console.WriteLine(x);
        Console.WriteLine(y);
        Console.WriteLine(z);
        Console.WriteLine(y.GetType().ToString());
    }
}