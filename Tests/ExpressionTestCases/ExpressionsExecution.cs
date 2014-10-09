using System;
using System.Linq.Expressions;

public static class Program
{
    public static void Main()
    {
        Expression<Action> ex = () => RunMe();
        var action = ex.Compile();
        action();
    }

    public static void RunMe()
    {
        Console.WriteLine("Test");
    }
}
