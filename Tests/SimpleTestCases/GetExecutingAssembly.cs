using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var executingAssembly = Assembly.GetExecutingAssembly();
        Console.WriteLine(executingAssembly.ToString());
    }
}