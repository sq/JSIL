
using System;

public class Program
{ 
    public static void Swap(ref ulong x, ref ulong y)
    {
        var tmp = x;
        x = y;
        y = tmp;
    }

    public static void Main()
    {
        var s = ulong.MaxValue;
        var t = 0xf0f0f0f0f0f0f0f0UL;
        Swap(ref s, ref t);
        Console.WriteLine(s);
        Console.WriteLine(t);
        Console.WriteLine(s - t);
    }
}