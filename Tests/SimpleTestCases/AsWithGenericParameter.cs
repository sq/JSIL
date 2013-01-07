using System;

struct F { }

public class Program
{

    public static bool A<T>(T t) { return true; }

    public static bool N<T>(object t)
    {
        return t is T;
    }

    public static bool M<T>(object t)
    {
        return (t is T) && A(t);
    }

    public static void Main()
    {
        Console.WriteLine(N<F>("foo") ? "True" : "False");
        Console.WriteLine(M<F>("foo") ? "True" : "False");
    }

}