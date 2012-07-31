using System;

public class Program
{
    public static void Main()
    {
        var i = 0;
        var x = 1L;
        Console.WriteLine("Left shift");
        Console.WriteLine(x);
        Console.WriteLine("{0} {1}", 1, x >> 1);
                          
        Console.WriteLine("{0} {1}", 8, x << 8);
        Console.WriteLine("{0} {1}", 16, x << 16);
                          
        Console.WriteLine("{0} {1}", 23, x << 23);
        Console.WriteLine("{0} {1}", 24, x << 24);
        Console.WriteLine("{0} {1}", 25, x << 25);
                          
        Console.WriteLine("{0} {1}", 32, x << 32);
        Console.WriteLine("{0} {1}", 47, x << 47);
        Console.WriteLine("{0} {1}", 48, x << 48);
        Console.WriteLine("{0} {1}", 49, x << 49);
                          
        Console.WriteLine("{0} {1}", 63, x << 63);
        Console.WriteLine("{0} {1}", 64, x << 64);
                          
        var y = x << 63;
        Console.WriteLine("Right shift");
        Console.WriteLine(y);
        Console.WriteLine("{0} {1}", 1, y >> 1);
        Console.WriteLine("{0} {1}", 2, y >> 2);
        Console.WriteLine("{0} {1}", 3, y >> 3);
        Console.WriteLine("{0} {1}", 8, y >> 8);
        Console.WriteLine("{0} {1}", 10, y >> 10);
        Console.WriteLine("{0} {1}", 12, y >> 12);
        Console.WriteLine("{0} {1}", 15, y >> 15);
        Console.WriteLine("{0} {1}", 16, y >> 16);
                          
        Console.WriteLine("{0} {1}", 23, y >> 23);
        Console.WriteLine("{0} {1}", 24, y >> 24);
        Console.WriteLine("{0} {1}", 25, y >> 25);
                          
        Console.WriteLine("{0} {1}", 32, y >> 32);
        Console.WriteLine("{0} {1}", 47, y >> 47);
        Console.WriteLine("{0} {1}", 48, y >> 48);
        Console.WriteLine("{0} {1}", 49, y >> 49);
                          
        Console.WriteLine("{0} {1}", 63, y >> 63);
        Console.WriteLine("{0} {1}", 64, y >> 64);

    }
}