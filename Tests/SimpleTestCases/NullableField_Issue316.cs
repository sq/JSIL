using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = new TestClass();
        if (a.Field.HasValue)
        {
            Console.WriteLine("Empty but HasValue");
        }
        if (a.Field != null)
        {
            Console.WriteLine("Empty but NotNull");
        }
        if (a.Field is int)
        {
            Console.WriteLine("Empty but Is Int");
        }
        if (a.Field is int?)
        {
            Console.WriteLine("Empty Is Int?");
        }

        a.Field = 10;
        if (a.Field.HasValue)
        {
            Console.WriteLine("NonEmpty HasValue");
        }
        if (a.Field != null)
        {
            Console.WriteLine("NonEmpty NotNull");
        }
        if (a.Field is int)
        {
            Console.WriteLine("NonEmpty Is Int");
        }
        if (a.Field is int?)
        {
            Console.WriteLine("NonEmpty Is Int?");
        }
    }
}

public class TestClass
{
    public int? Field;
}