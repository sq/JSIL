
using System;
enum E { a, b }

public static class Program
{
    public static ulong GetULong() { return 0UL; }
    public static long GetLong() { return 0L; }

    public static void Main(string[] args)
    {
        Console.WriteLine((E)GetULong());
        Console.WriteLine((E)GetLong());
    }
}