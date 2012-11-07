using System;

public class Program
{
    public static void Main()
    {
        var x = 1L;
        var y = 2L;

        for (var i = 0; i < 10; i++)
            x += y;

        Print(x);
        Print(y);
    }

    private static void Print<T> (T t) {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}