using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = new Action(new Action(A));
        a();
        a += A;
        a = new Action(a);
        a();
    }

    public static void A()
    {
        Console.WriteLine("A");
    }
}