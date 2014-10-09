using System;
using System.Linq.Expressions;

public static class Program
{
    public static void Main()
    {
        Expression<Action> ex = () => RunMe();
        var action = ex.Compile();
        action();
        ex = () => WriteLine(Convert.ToString(2 + Get40()));
        action = ex.Compile();
        action();
    }
    public static void WriteLine(string line)
    {
        Console.WriteLine(line);
    }

    public static void RunMe()
    {
        Console.WriteLine("Test");
    }

    public static int Get40()
    {
        return 40;
    }
}
