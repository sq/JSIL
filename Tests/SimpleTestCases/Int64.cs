using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var x = 100000000000L;
        var y = 10000053450L;
        Console.WriteLine(x);
        Console.WriteLine(y);
        Console.WriteLine(x + y);
        Console.WriteLine(x - y);
        Console.WriteLine(x * y);
        Console.WriteLine(x / y);
        Console.WriteLine(x % y);
        Console.WriteLine(x & y);
        Console.WriteLine(x | y);
        Console.WriteLine(x >> 3);
        Console.WriteLine(y << 3);
        Console.WriteLine(2 * x + 3 * y / 7);

        //object z = x;
        //Console.WriteLine(x * (long)z);
    }
}