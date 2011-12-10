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

        int z = 2;
        Console.WriteLine(((long)z) * x);

        // check JS to see if this uses long operations or not
        int a = 5;
        int b = 6;
        long w = a - b;
        Console.WriteLine(w);

        //object box = x;
        //Console.WriteLine(x * (long)box);
    }
}