using System;

public class Program
{
    public static void Main()
    {
        var values = new long[2] { 0, 5 };
        var x = 1L;
        var y = 2L;

        Print(values[0]);
        Print(values[1]);

        for (var i = 0; i < 10; i++) {
            values[0] += x;
            values[1] += y;
        }

        Print(values[0]);
        Print(values[1]);
    }

    private static void Print<T> (T t) {
        Console.WriteLine(t);
        Console.WriteLine(t.GetType());
    }
}