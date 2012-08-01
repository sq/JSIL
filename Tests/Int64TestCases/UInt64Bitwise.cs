using System;

public class Program
{
    public static void Main()
    {
        ulong x = ulong.MaxValue;
        ulong y = ulong.MaxValue;
        ulong z = 0xf0f0f0f0f0f0f0f0UL;
        ulong w = 0xfff000f0f0f0ff00UL;
        Console.WriteLine(~x);
        Console.WriteLine(y ^ z);
        Console.WriteLine(x & y);
        Console.WriteLine(w | y);
    }
}