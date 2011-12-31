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

        int days = 100, hours = 20, minutes = 1, seconds = 1, milliseconds = 20;
        int hrssec = (hours * 3600); // break point at (Int32.MaxValue - 596523)
        int minsec = (minutes * 60);
        long t = ((long)(hrssec + minsec + seconds) * 1000L + (long)milliseconds);
        t *= 10000;
        Console.WriteLine(t);

        //object box = x;
        //Console.WriteLine(x * (long)box);

        var n = 634590720000000000L;
        var m = 864000000000L;
        Console.WriteLine(n);
        Console.WriteLine(m);
        Console.WriteLine(1 + (int)(n / m));
    }
}