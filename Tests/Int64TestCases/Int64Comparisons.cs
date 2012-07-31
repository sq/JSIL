using System;

public class Program
{
    public static void Main()
    {
        var x = 1000000L;
        var x2 = x;
        var y = 1450L;
        var z = -15033L;

        // comparison
        WriteBool(x == y);
        WriteBool(x == x2);
        WriteBool(x != y);
        WriteBool(x != x2);
        WriteBool(x < y);
        WriteBool(x <= x2);
        WriteBool(x > y);
        WriteBool(x >= x2);

        WriteBool(x == z);
        WriteBool(x != z);
        WriteBool(x < z);
        WriteBool(x > y);
        WriteBool(x >= z);

        var y2 = y;
        WriteBool(y == z);
        WriteBool(y != z);
        WriteBool(y < z);
        WriteBool(y > y2);
        WriteBool(y >= z);

    }

    public static void WriteBool(bool b)
    {
        // System.Bool.ToString() is lower-case in JS
        Console.WriteLine(b ? "True" : "False");
    }
}