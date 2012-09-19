using System;

public class Program
{
    public static void Main()
    {
        int? x = 1000;
        long? y = 2000;
        long? z = null;

        Console.WriteLine(x);
        Console.WriteLine(y);
        Console.WriteLine(x + y);
        Console.WriteLine(z == null ? "True" : "False");
        Console.WriteLine((x + y) == null ? "True" : "False");
        Console.WriteLine((x + z) == null ? "True" : "False");
        Console.WriteLine((z + z) == null ? "True" : "False");

    }
}