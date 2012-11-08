
using System;
public class Program
{
    public static void Main()
    {
        long x = 789L;
        Console.WriteLine(123 + (int)x);
        Console.WriteLine(123UL + (ulong)x);
        Console.WriteLine((123UL + (ulong)x).GetType());
        Console.WriteLine(123U + (uint)x);
        Console.WriteLine(123d + (double)x);
        Console.WriteLine(123f + (float)x);
    }
}