using System;

public class Program
{
    public static void Main()
    {
        ulong x = ulong.MaxValue;
        ulong y = ulong.MaxValue;
        ulong z = 0xf0f0f0f0f0f0f0f0UL;
        ulong w = 0xfff000f0f0f0ff00UL;
        Print(~x);
        Print(y ^ z);
        Print(x & y);
        Print(w | y);

    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}