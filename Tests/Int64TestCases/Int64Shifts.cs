using System;

public class Program
{
    public static void Main()
    {
        var i = 0;
        var x = 1L;
        Console.WriteLine("Left shift");
        Console.WriteLine(x);
        Console.WriteLine("{0} {1}", 1, Format(x << 1));
                          
        Console.WriteLine("{0} {1}", 8, Format(x << 8));
        Console.WriteLine("{0} {1}", 16, Format(x << 16));
                          
        Console.WriteLine("{0} {1}", 23, Format(x << 23));
        Console.WriteLine("{0} {1}", 24, Format(x << 24));
        Console.WriteLine("{0} {1}", 25, Format(x << 25));
                          
        Console.WriteLine("{0} {1}", 32, Format(x << 32));
        Console.WriteLine("{0} {1}", 47, Format(x << 47));
        Console.WriteLine("{0} {1}", 48, Format(x << 48));
        Console.WriteLine("{0} {1}", 49, Format(x << 49));
                                         
        Console.WriteLine("{0} {1}", 63, Format(x << 63));
        Console.WriteLine("{0} {1}", 64, Format(x << 64));

        var t = 1L;
        Console.WriteLine("{0} {1}", 1, Format(t >> 1));

        var y = x << 63;
        Console.WriteLine("Right shift");
        Console.WriteLine(y);
        Console.WriteLine("{0} {1}", 1, Format(y >> 1));
        Console.WriteLine("{0} {1}", 2, Format(y >> 2));
        Console.WriteLine("{0} {1}", 3, Format(y >> 3));
        Console.WriteLine("{0} {1}", 8, Format(y >> 8));
        Console.WriteLine("{0} {1}", 10, Format(y >> 10));
        Console.WriteLine("{0} {1}", 12, Format(y >> 12));
        Console.WriteLine("{0} {1}", 15, Format(y >> 15));
        Console.WriteLine("{0} {1}", 16, Format(y >> 16));
                          
        Console.WriteLine("{0} {1}", 23, Format(y >> 23));
        Console.WriteLine("{0} {1}", 24, Format(y >> 24));
        Console.WriteLine("{0} {1}", 25, Format(y >> 25));
                                         
        Console.WriteLine("{0} {1}", 32, Format(y >> 32));
        Console.WriteLine("{0} {1}", 47, Format(y >> 47));
        Console.WriteLine("{0} {1}", 48, Format(y >> 48));
        Console.WriteLine("{0} {1}", 49, Format(y >> 49));
                                         
        Console.WriteLine("{0} {1}", 63, Format(y >> 63));
        Console.WriteLine("{0} {1}", 64, Format(y >> 64));
    }

    private static string Format<T>(T t)
    {
        return string.Format("{0} {1}", t, t.GetType());
    }
}