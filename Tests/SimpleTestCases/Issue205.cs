using System;
using System.Linq.Expressions;

public static class Program {
    public static void Main (string[] args) {
        Expression<Func<int, bool>> expr = (value) => value == 42;
    }
}