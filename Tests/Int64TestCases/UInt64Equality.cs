using System;

public class Program
{ 
    public static void Main()
    {
        var x = ulong.MaxValue;
        var y = ulong.MinValue;
        var z = y - 1;
        Console.WriteLine(x == z ? "True" : "False");
        Console.WriteLine((x + 1UL) == 0UL ? "True" : "False");
        Console.WriteLine(x != z ? "True" : "False");
        Console.WriteLine((x + 1UL) != 0UL ? "True" : "False");
        Console.WriteLine(x == y ? "True" : "False");
        Console.WriteLine((x + 1UL) == 2UL ? "True" : "False");
        Console.WriteLine(x != y ? "True" : "False");
        Console.WriteLine((x + 1UL) != 2UL ? "True" : "False");
    }
}