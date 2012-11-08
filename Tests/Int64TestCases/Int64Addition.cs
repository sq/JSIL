using System;

public class Program
{
    public static void Main()
    {
        var x = 100000000000L;
        var y = 10000053450L;

        // addition
        Console.WriteLine(-x);
        Console.WriteLine(1000000000000L + x);
        Console.WriteLine(1L + 0L);
        Console.WriteLine(1L + x);
        Console.WriteLine(1L + -x);
        Console.WriteLine(1 + 0L);
        Console.WriteLine(1 + x);
        Console.WriteLine(1 + -x);

        Console.WriteLine((x + y).GetType());
    }
}