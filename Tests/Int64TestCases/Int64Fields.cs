using System;

public class Program
{
    public const long TEST_CONST = 1000L;
    public readonly long TEST_READONLY_FIELD = 2000L;
    public static long TEST_STATIC_FIELD = 3000L;
    public long TEST_NORMAL_FIELD = 4000L;

    public static void Main()
    {
        // fields
        Print(long.MaxValue);
        Print(TEST_CONST);
        Print(new Program().TEST_READONLY_FIELD);
        Print(TEST_STATIC_FIELD);
        Print(new Program().TEST_NORMAL_FIELD);
    }

    private static void Print<T>(T t)
    {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}