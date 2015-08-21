//@useroslyn

using System;

public static class Program
{
    public static void Main(string[] args)
    {
        int? a = 0;
        MoveNextCore(ref a);
        Console.WriteLine(a);
    }

    public static void MoveNextCore(ref int? currentElement)
    {
        currentElement = 100;
    }
}