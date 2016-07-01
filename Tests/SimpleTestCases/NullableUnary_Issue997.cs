using System;

public static class Program
{
    public static void Main()
    {
        bool? value1 = !GetNullBool();
        Console.WriteLine(value1 == null ? "null" : "not null");
        Console.WriteLine(value1 == true ? "true" : "false");
    }

    public static bool? GetNullBool()
    {
        return true;
    }
}
