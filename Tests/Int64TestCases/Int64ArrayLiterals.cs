using System;

public class Program
{
    public static void Main()
    {
        var array = new long[] { 0, 1, 1L, long.MaxValue };

        foreach (var l in array)
            Print(l);
    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}