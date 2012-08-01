using System;

public class Program
{
    public static void Main()
    {
        var x = ulong.MaxValue;

        Console.WriteLine(0UL + 0UL);
        Console.WriteLine(1UL + 0UL);
        Console.WriteLine(1UL + 1UL);
        Console.WriteLine(0xffffffUL + 1UL); // overflow a -> b
        Console.WriteLine(0xffffffffffffUL + 1UL); // overflow b -> c
        Console.WriteLine(x + 1UL); // overflow Max -> 0
        Console.WriteLine(0xffffffUL + 0xffffffUL); 


    }
}