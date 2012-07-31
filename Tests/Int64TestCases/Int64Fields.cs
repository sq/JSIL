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
        Console.WriteLine(TEST_CONST);
        Console.WriteLine(new Program().TEST_READONLY_FIELD);
        Console.WriteLine(TEST_STATIC_FIELD);
        Console.WriteLine(new Program().TEST_NORMAL_FIELD);
    }
}