using System;
using System.Linq.Expressions;

public static class Program {
    public static void Main () {
        Expression<Func<int>> expr = () => 5;
        System.Console.WriteLine(expr);
    }
}