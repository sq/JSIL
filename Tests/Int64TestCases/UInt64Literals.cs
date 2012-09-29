
using System;

public class Program
{
    public static void Main()
    {
        var x = 100000000000UL;
        var y = 10000053450123333333UL;

        Console.WriteLine(0UL);
        Console.WriteLine(10UL);
        Console.WriteLine(10000053450123333333UL);
        Console.WriteLine(0UL.GetType());
        Console.WriteLine(10UL.GetType());
        Console.WriteLine(10000053450123333333UL.GetType());
        Console.WriteLine(x);
        Console.WriteLine(y);
        Console.WriteLine(y.GetType().ToString());
    }
}